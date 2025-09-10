namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Funciones relacionadas con tablas temporales para <see cref="SQLServerProvider"/>.</summary>
public partial class SQLServerProvider
{
    /// <summary>Elimina una tabla de la base de datos si existe.</summary>
    public void DropTableIfExists(string tableName) => ExecuteCommand(DropTableScriptSQL(tableName));

    /// <summary>Crea una nueva tabla en la base de datos basada en la estructura de un DataTable.</summary>
    /// <param name="source">DataTable que contiene la estructura de la tabla a crear.</param>
    /// <param name="target">Nombre de la tabla de destino.</param>
    public void CreateTable(DataTable source, string target) =>
        ExecuteCommand(CreateTableScriptSQL(source, target));

    /// <summary>Genera el script SQL para eliminar una tabla si existe.</summary>
    internal static string DropTableScriptSQL(string table)
    {
        return table.Contains("#")
            ? $"IF OBJECT_ID('tempdb..{table}') IS NOT NULL DROP TABLE {table}"
            : GetDropTableScriptForPermanentTable(table);
    }

    private static string GetDropTableScriptForPermanentTable(string table)
    {
        string fullName = table.Contains(".") ? table : $"dbo.{table}";
        string[] parts = fullName.Split(['.'], 2);
        string bracketedName = parts.Length == 2 ? $"[{parts[0]}].[{parts[1]}]" : $"[{parts[0]}]";

        return $"IF OBJECT_ID('{fullName}') IS NOT NULL DROP TABLE {bracketedName}";
    }

    internal static string GetSqlDataType(DataColumn column)
    {
        Dictionary<Type, Func<int, string>> typeMapping = new()
        {
            [typeof(string)] = size => $"NVARCHAR({(size == -1 ? "MAX" : size.ToString())}) COLLATE DATABASE_DEFAULT",
            [typeof(object)] = size => $"NVARCHAR({(size == -1 ? "MAX" : size.ToString())}) COLLATE DATABASE_DEFAULT",
            [typeof(decimal)] = _ => "DECIMAL(18,6)",
            [typeof(double)] = _ => "FLOAT",
            [typeof(float)] = _ => "FLOAT",
            [typeof(byte[])] = _ => "VARBINARY(MAX)",
            [typeof(long)] = _ => "BIGINT",
            [typeof(short)] = _ => "SMALLINT",
            [typeof(int)] = _ => "INT",
            [typeof(DateTime)] = _ => "DATETIME",
            [typeof(bool)] = _ => "BIT",
            [typeof(Guid)] = _ => "UNIQUEIDENTIFIER",
            [typeof(byte)] = _ => "TINYINT"
        };

        if (typeMapping.TryGetValue(column.DataType, out Func<int, string> mapper))
            return mapper(column.MaxLength);

        throw new Exception($"Tipo de dato no considerado: {column.ColumnName} {column.DataType.FullName}");
    }
}