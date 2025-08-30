namespace DevKit.ExecutionEngine.Oracle.Implementations;

/// <summary>Clase parcial que extiende OracleRepository para manejar operaciones con tablas temporales. Proporciona funcionalidades para crear y eliminar tablas temporales en la base de datos.</summary>
public partial class OracleProvider
{
    /// <summary>Columna de identidad por defecto para las tablas temporales.</summary>
    private const string IdentityColumn = "PrincipalID INT IDENTITY(1, 1) PRIMARY KEY(PrincipalID) ";

    /// <summary>Mensaje de error para tipos de datos no reconocidos.</summary>
    private const string UnknownTypeMessage = "Tipo desconocido";

    /// <inheritdoc/>
    public void DropTable(string tableName) => ExecuteNonQuery(DropTableScriptSQL(tableName));

    /// <inheritdoc/>
    public void CreateTable(DataTable sourceTable, string destinationTable) => ExecuteNonQuery(CreateTableScriptSQL(sourceTable, destinationTable));

    /// <summary>Crea una nueva tabla basada en la estructura de un IDataReader.</summary>
    /// <param name="reader">IDataReader con la estructura de la tabla.</param>
    /// <param name="destinationTable">Nombre de la tabla de destino.</param>
    public void CreateTable(IDataReader reader, string destinationTable) => ExecuteNonQuery(CreateTableScriptSQL(reader, destinationTable));
    
    /// <summary>Genera el script SQL para crear una tabla basada en un DataTable o IDataReader.</summary>
    /// <typeparam name="T">Tipo del origen de datos (DataTable o IDataReader).</typeparam>
    /// <param name="sourceTable">Origen de datos con la estructura de la tabla.</param>
    /// <param name="destinationTable">Nombre de la tabla de destino.</param>
    /// <returns>Cadena con el script SQL para crear la tabla.</returns>
    /// <exception cref="ArgumentException">Se lanza cuando el tipo de origen no es compatible.</exception>
    internal static string CreateTableScriptSQL<T>(T sourceTable, string destinationTable)
    {
        switch (sourceTable)
        {
            case DataTable table:
                IEnumerable<string> columnDefinitions = table
                    .Columns
                    .Cast<DataColumn>()
                    .Select(columnName => $"[{columnName.ColumnName}] {GetSqlDataType(columnName.ColumnName, columnName.DataType, columnName.MaxLength)}{Environment.NewLine}");

                if (destinationTable.StartsWith("#"))
                {
                    destinationTable = $"tempdb..{destinationTable}";
                }
                return $@"IF NOT EXISTS (SELECT OBJECT_ID FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'{destinationTable}'))
                    BEGIN
                        CREATE TABLE {destinationTable}
                        ( 
                            {IdentityColumn}, {string.Join(",", columnDefinitions)}
                        )
                    END";
            case IDataReader reader:

                DataTable table2 = reader.GetSchemaTable();

                List<string> columnas = new List<string>();

                if (table2?.Rows != null)
                {
                    foreach (DataRow row in table2.Rows)
                    {
                        columnas.Add(
                            $"[{row.GetValue<string>("ColumnName")}] {GetSqlDataType(row.GetValue<string>("ColumnName"), Type.GetType(row.GetValue<string>("DataType")), row.GetValue<int>("ColumnSize"))}{Environment.NewLine}");
                    }
                }

                columnas.Insert(0, $"PrincipalID INT IDENTITY(1, 1) PRIMARY KEY(PrincipalID)  {Environment.NewLine} ");
                return $"IF NOT EXISTS (SELECT OBJECT_ID FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'{destinationTable}') AND TYPE in (N'U')) {Environment.NewLine} BEGIN {Environment.NewLine} CREATE TABLE {destinationTable}({Environment.NewLine} {string.Join(",", columnas)}){Environment.NewLine}  END";

            default:
                throw new ArgumentException(UnknownTypeMessage);
        }
    }
    /// <summary>Genera el script SQL para eliminar una tabla si existe.</summary>
    /// <param name="destinationTable">Nombre de la tabla a eliminar.</param>
    /// <returns>Cadena con el script SQL para eliminar la tabla.</returns>
    internal static string DropTableScriptSQL(string destinationTable)
    {
        if (destinationTable.StartsWith("#"))
        {
            destinationTable = $"tempdb..{destinationTable}";
        }
        return
            $"IF EXISTS (SELECT OBJECT_ID FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'{destinationTable}'))" +
            $"BEGIN " +
            $"  DROP TABLE {destinationTable} " +
            $"END";
    }
    /// <summary>Obtiene el tipo de dato SQL correspondiente a un tipo de .NET.</summary>
    /// <param name="columnName">Nombre de la columna (para mensajes de error).</param>
    /// <param name="dataType">Tipo de datos .NET.</param>
    /// <param name="columnSize">Tamaño de la columna.</param>
    /// <returns>Cadena con el tipo de dato SQL correspondiente.</returns>
    /// <exception cref="Exception">Se lanza cuando el tipo de dato no es compatible.</exception>
    internal static string GetSqlDataType(string columnName, Type dataType, int columnSize)
    {
        switch (dataType)
        {
            case not null when dataType == typeof(string):
                return $"{Enum.GetName(typeof(OracleDbType), OracleDbType.Varchar2)}({(columnSize == -1 ? "MAX" : columnSize.ToString())})";

            case not null when dataType == typeof(object):
                return $"{Enum.GetName(typeof(OracleDbType), OracleDbType.Varchar2)}({(columnSize == -1 ? "MAX" : columnSize.ToString())})";

            case not null when dataType == typeof(double) || dataType == typeof(decimal):
                return Enum.GetName(typeof(OracleDbType), OracleDbType.BinaryFloat);

            case not null when dataType == typeof(long):
                return Enum.GetName(typeof(OracleDbType), OracleDbType.Int64);

            case not null when dataType == typeof(short) || dataType == typeof(int):
                return Enum.GetName(typeof(OracleDbType), OracleDbType.Int32);

            case not null when dataType == typeof(DateTime):
                return Enum.GetName(typeof(OracleDbType), OracleDbType.Date);

            case not null when dataType == typeof(DateTime):
                return Enum.GetName(typeof(OracleDbType), OracleDbType.Date);

            case not null when dataType == typeof(float):
                return Enum.GetName(typeof(OracleDbType), OracleDbType.BinaryFloat);

            case not null when dataType == typeof(bool):
                return Enum.GetName(typeof(OracleDbType), OracleDbType.Boolean);

            case not null when dataType == typeof(Guid):
                return Enum.GetName(typeof(OracleDbType), OracleDbType.Raw);

            default:
                throw new Exception($"Tipo de dato no considerado: {columnName} {dataType?.FullName}");
        }
    }
}