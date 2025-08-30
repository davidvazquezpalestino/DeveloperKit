namespace DevKit.ExecutionEngine.MySQL.Implementations;

/// <summary>
/// Provides methods for temporary table management in MySQL.
/// </summary>
public partial class MySqlProvider
{
    /// <inheritdoc/>
    public void DropTable(string tableName) =>
        ExecuteNonQuery(DropTableScriptMySQL(tableName));

    /// <inheritdoc/>
    public void CreateTable(DataTable source, string target) =>
        ExecuteNonQuery(CreateTableScriptMySQL(source, target));

    internal static string CreateTableScriptMySQL<T>(T source, string target)
    {
        string query;

        switch (source)
        {
            case DataTable table:
                IEnumerable<string> columnDefinitions = table.Columns
                    .Cast<DataColumn>()
                    .Select(column =>
                        $"`{column.ColumnName}` {GetMySqlDataType(column.ColumnName, column.DataType, column.MaxLength)}");

                query = $@"
                    CREATE TABLE `{target}` (
                        `PrincipalID` INT AUTO_INCREMENT PRIMARY KEY,
                        {string.Join("," + Environment.NewLine, columnDefinitions)}
                    );";
                break;

            default:
                throw new ArgumentException("El tipo de tabla proporcionado no es compatible.");
        }

        return query;
    }
    internal static string DropTableScriptMySQL(string table)
    {
        return $"DROP TABLE IF EXISTS `{table}`;";
    }

    internal static string GetMySqlDataType(string columnName, Type dataType, int columnSize)
    {
        return dataType switch
        {
            _ when dataType == typeof(string) || dataType == typeof(object) => columnSize == -1 ? "TEXT" : $"VARCHAR({columnSize})",
            _ when dataType == typeof(decimal) => "DECIMAL(18,6)",
            _ when dataType == typeof(double) || dataType == typeof(float) => "DOUBLE",
            _ when dataType == typeof(byte[]) => "BLOB",
            _ when dataType == typeof(long) => "BIGINT",
            _ when dataType == typeof(short) || dataType == typeof(int) => "INT",
            _ when dataType == typeof(DateTime) => "DATETIME",
            _ when dataType == typeof(bool) => "BOOLEAN",
            _ when dataType == typeof(Guid) => "CHAR(36)",

            _ => throw new Exception($"Tipo de dato no considerado: {columnName} {dataType?.FullName}")
        };
    }
}