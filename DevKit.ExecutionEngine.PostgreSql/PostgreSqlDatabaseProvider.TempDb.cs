namespace DevKit.ExecutionEngine.PostgreSql;

public partial class PostgreSqlDatabaseProvider
{
    /// <inheritdoc/>
    public void CreateTable(DataTable schema, string tableName)
    {
        string command = $"CREATE TABLE \"{tableName}\" ({GetColumnDeclarations(schema)});";
        ExecuteNonQuery(command);
    }

    /// <inheritdoc/>
    public void DropTable(string tableName)
    {
        ExecuteNonQuery($"DROP TABLE IF EXISTS \"{tableName}\";");
    }

    /// <summary>
    /// Asynchronously creates a new table in the database based on the schema of a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="schema">The <see cref="DataTable"/> whose schema defines the table structure.</param>
    /// <param name="tableName">The name of the table to create.</param>
    public async Task CreateTableAsync(DataTable schema, string tableName)
    {
        string command = $"CREATE TABLE \"{tableName}\" ({GetColumnDeclarations(schema)});";
        await ExecuteNonQueryAsync(command);
    }

    /// <summary>
    /// Asynchronously drops a table from the database if it exists.
    /// </summary>
    /// <param name="tableName">The name of the table to drop.</param>
    public async Task DropTableAsync(string tableName)
    {
        await ExecuteNonQueryAsync($"DROP TABLE IF EXISTS \"{tableName}\";");
    }

    private string GetColumnDeclarations(DataTable schema)
    {
        List<string> declarations = new List<string>();
        foreach (DataColumn column in schema.Columns)
        {
            string type = GetPostgreSqlType(column.DataType);
            declarations.Add($"\"{column.ColumnName}\" {type}");
        }
        return string.Join(", ", declarations);
    }

    private string GetPostgreSqlType(Type dataType)
    {
        if (dataType == typeof(int))
        {
            return "INTEGER";
        }

        if (dataType == typeof(long))
        {
            return "BIGINT";
        }

        if (dataType == typeof(short))
        {
            return "SMALLINT";
        }

        if (dataType == typeof(decimal))
        {
            return "DECIMAL";
        }

        if (dataType == typeof(double))
        {
            return "DOUBLE PRECISION";
        }

        if (dataType == typeof(float))
        {
            return "REAL";
        }

        if (dataType == typeof(bool))
        {
            return "BOOLEAN";
        }

        if (dataType == typeof(DateTime))
        {
            return "TIMESTAMP";
        }

        if (dataType == typeof(Guid))
        {
            return "UUID";
        }

        if (dataType == typeof(byte[]))
        {
            return "BYTEA";
        }

        return "TEXT"; // Default to TEXT for strings and other types
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string QuoteQualifiedName(string name)
    {
        string[] parts = name.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join('.', parts.Select(QuoteIdent));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ident"></param>
    /// <returns></returns>
    public static string QuoteIdent(string ident) => "\"" + ident.Replace("\"", "\"\"") + "\"";
    /// <summary>
    /// 
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    public string GetPgTypeName(DataColumn col)
    {
        Type t = col.DataType;
        if (t == typeof(string))
        {
            return "text";
        }

        if (t == typeof(int))
        {
            return "int4";
        }

        if (t == typeof(long))
        {
            return "int8";
        }

        if (t == typeof(short))
        {
            return "int2";
        }

        if (t == typeof(byte))
        {
            return "int2"; // no int1 in PG; often smallint is acceptable
        }

        if (t == typeof(bool))
        {
            return "bool";
        }

        if (t == typeof(decimal))
        {
            return "numeric";
        }

        if (t == typeof(double))
        {
            return "float8";
        }

        if (t == typeof(float))
        {
            return "float4";
        }

        if (t == typeof(DateTime))
        {
            return "timestamp";
        }

        if (t == typeof(DateTimeOffset))
        {
            return "timestamptz";
        }

        if (t == typeof(Guid))
        {
            return "uuid";
        }

        if (t == typeof(byte[]))
        {
            return "bytea";
        }

        // fallback: let Npgsql infer
        return null;
    }
}