namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Funciones relacionadas con tablas temporales para <see cref="SQLServerProvider"/>.</summary>
public partial class SQLServerProvider
{
    /// <summary>Elimina una tabla de la base de datos si existe.</summary>
    public void DropTable(string tableName) => ExecuteNonQuery(DropTableScriptSQL(tableName));

    /// <summary>Crea una nueva tabla en la base de datos basada en la estructura de un DataTable.</summary>
    /// <param name="source">DataTable que contiene la estructura de la tabla a crear.</param>
    /// <param name="target">Nombre de la tabla de destino.</param>
    public void CreateTable(DataTable source, string target) =>
        ExecuteNonQuery(CreateTableScriptSQL(source, target));


    /// <summary>Genera el script SQL para crear una tabla basada en un objeto fuente.</summary>
    internal static string CreateTableScriptSQL<T>(T source, string destination)
    {
        string query;

        bool isTemp = destination.Contains("#");

        // Build CREATE target and OBJECT_ID target
        string createTarget;
        string objectIdTarget;

        if (isTemp)
        {
            // Temp tables: CREATE TABLE #tmp, OBJECT_ID('tempdb..#tmp')
            createTarget = destination;
            objectIdTarget = $"tempdb..{destination}";
        }
        else
        {
            // Permanent tables: ensure schema and use unbracketed for OBJECT_ID, bracketed for CREATE/DROP
            string fullName = destination.Contains(".") ? destination : $"dbo.{destination}";
            string[] parts = fullName.Split(new[] { '.' }, 2);
            string bracketed = $"[{parts[0]}].[{parts[1]}]";
            createTarget = bracketed;     // for CREATE
            objectIdTarget = fullName;    // for OBJECT_ID
        }

        switch (source)
        {
            case DataTable table:
                IEnumerable<string> columnDefinitions = table.Columns
                    .Cast<DataColumn>()
                    .Select(column =>
                        $"[{column.ColumnName}] {GetSqlDataType(column.ColumnName, column.DataType, column.MaxLength)}");

                query = $@"
                        IF OBJECT_ID('{objectIdTarget}', 'U') IS NULL
                        BEGIN
                            CREATE TABLE {createTarget}
                            (
                                [PrincipalID] INT IDENTITY(1,1) PRIMARY KEY,
                                {string.Join("," + Environment.NewLine, columnDefinitions)}
                            )
                        END";
                break;

            case IDataReader reader:
                DataTable schemaTable = reader.GetSchemaTable();
                if (schemaTable == null || schemaTable.Rows.Count == 0)
                {
                    throw new InvalidOperationException("No se pudo obtener el esquema del IDataReader.");
                }

                List<string> columns = new List<string>();

                foreach (DataRow row in schemaTable.Rows)
                {
                    string columnName = row["ColumnName"].ToString() ?? throw new ArgumentException("ColumnName no encontrado.");
                    Type type = row["DataType"] as Type ?? throw new ArgumentException("DataType no encontrado.");
                    int columnSize = row["ColumnSize"] != DBNull.Value ? Convert.ToInt32(row["ColumnSize"]) : -1;

                    columns.Add($"[{columnName}] {GetSqlDataType(columnName, type, columnSize)}");
                }

                columns.Insert(0, "[PrincipalID] INT IDENTITY(1, 1) PRIMARY KEY");

                query = $@"
                        IF OBJECT_ID('{objectIdTarget}', 'U') IS NULL
                        BEGIN
                            CREATE TABLE {createTarget}
                            (
                                {string.Join("," + Environment.NewLine, columns)}
                            )
                        END";
                break;

            default:
                throw new ArgumentException("El tipo de tabla proporcionado no es compatible.");
        }

        return query;
    }

    /// <summary>Genera el script SQL para eliminar una tabla si existe.</summary>
    /// <param name="table">Nombre de la tabla a eliminar.</param>
    /// <returns>Script SQL para eliminar la tabla.</returns>
    internal static string DropTableScriptSQL(string table)
    {
        bool isTemp = table.Contains("#");

        if (isTemp)
        {
            return $@"
            IF OBJECT_ID('tempdb..{table}', 'U') IS NOT NULL
            BEGIN
                DROP TABLE {table}
            END";
        }

        string fullName = table.Contains(".") ? table : $"dbo.{table}";
        string[] parts = fullName.Split(['.'], 2);
        string bracketed = $"[{parts[0]}].[{parts[1]}]";

        return $@" 
            IF OBJECT_ID('{fullName}', 'U') IS NOT NULL
            BEGIN
                DROP TABLE {bracketed}
            END";
    }

    /// <summary>Obtiene el tipo de dato SQL correspondiente a un tipo de dato .NET.</summary>
    /// <param name="columnName">Nombre de la columna (para mensajes de error).</param>
    /// <param name="dataType">Tipo de dato .NET.</param>
    /// <param name="columnSize">Tamaño de la columna.</param>
    /// <returns>Cadena que representa el tipo de dato SQL.</returns>
    /// <exception cref="Exception">Se lanza cuando el tipo de dato no está soportado.</exception>
    internal static string GetSqlDataType(string columnName, Type dataType, int columnSize)
    {
        switch (dataType)
        {
            case not null when dataType == typeof(string):
                return $"{Enum.GetName(typeof(SqlDbType), SqlDbType.NVarChar)}({(columnSize == -1 ? "MAX" : columnSize.ToString())}) COLLATE DATABASE_DEFAULT";

            case not null when dataType == typeof(object):
                return $"{Enum.GetName(typeof(SqlDbType), SqlDbType.NVarChar)}({(columnSize == -1 ? "MAX" : columnSize.ToString())}) COLLATE DATABASE_DEFAULT";

            case not null when dataType == typeof(decimal):
                return "DECIMAL(18,6)";

            case not null when dataType == typeof(double) || dataType == typeof(float):
                return Enum.GetName(typeof(SqlDbType), SqlDbType.Float);

            case not null when dataType == typeof(byte[]):
                return Enum.GetName(typeof(SqlDbType), SqlDbType.VarBinary) + "(MAX)";

            case not null when dataType == typeof(long):
                return Enum.GetName(typeof(SqlDbType), SqlDbType.BigInt);

            case not null when dataType == typeof(short) || dataType == typeof(int):
                return Enum.GetName(typeof(SqlDbType), SqlDbType.Int);

            case not null when dataType == typeof(DateTime):
                return Enum.GetName(typeof(SqlDbType), SqlDbType.DateTime);

            case not null when dataType == typeof(bool):
                return Enum.GetName(typeof(SqlDbType), SqlDbType.Bit);

            case not null when dataType == typeof(Guid):
                return Enum.GetName(typeof(SqlDbType), SqlDbType.UniqueIdentifier);

            default:
                throw new Exception($"Tipo de dato no considerado: {columnName} {dataType?.FullName}");
        }
    }
}