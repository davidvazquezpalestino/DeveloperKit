
namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <summary>
    /// Realiza una inserción masiva de datos desde un DataTable a la tabla de destino de forma síncrona.
    /// </summary>
    /// <param name="source">DataTable que contiene los datos a insertar.</param>
    /// <param name="target">Nombre de la tabla de destino.</param>
    public void ExecuteBulkInsert(DataTable source, string target)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException("Nombre de tabla inválido", nameof(target));
        }

        using SqlBulkCopy bulkCopy = new(Connection, SqlBulkCopyOptions.Default, Transaccion);
        bulkCopy.DestinationTableName = target;
        bulkCopy.BatchSize = GetBatchSize(source.Rows.Count);
        bulkCopy.BulkCopyTimeout = SqlOptions.BulkCopy.BulkCopyTimeout;

        foreach (DataColumn column in source.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        if (Connection.State == ConnectionState.Closed)
        {
            Connection.Open();
        }

        bulkCopy.WriteToServer(source);
    }

    /// <summary>Realiza una copia masiva de un DataTable a la tabla destino de forma asíncrona.</summary>
    public async Task ExecuteBulkInsertToTableAsync(DataTable source, string target, CancellationToken cancellationToken = default)
    {

        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException("Nombre de tabla destino inválido", nameof(target));
        }

        bool shouldCloseConnection = Connection.State != ConnectionState.Open;

        try
        {
            if (shouldCloseConnection)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await CreateTableIfNotExistsAsync(source, target, cancellationToken).ConfigureAwait(false);
            await BulkInsertDataAsync(source, target, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (shouldCloseConnection && Connection?.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }
    }


    /// <summary>Copia masivamente datos de un DataTable a la tabla destino.</summary>
    public void ExecuteBulkInsertToTable(DataTable source, string target)
    {
        DropTableIfExists(target);
        CreateTable(source, target);
        ExecuteBulkInsert(source, target);
    }
    private async Task CreateTableIfNotExistsAsync(DataTable source, string target, CancellationToken cancellationToken)
    {
        using SqlCommand command = Connection.CreateCommand();
        command.CommandText = CreateTableScriptSQL(source, target);
        command.CommandTimeout = SqlOptions.CommandTimeout;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task BulkInsertDataAsync(DataTable source, string target, CancellationToken cancellationToken)
    {
        using SqlBulkCopy bulkCopy = new(ConnectionString, SqlBulkCopyOptions.Default)
        {
            DestinationTableName = target,
            BatchSize = GetBatchSize(source.Rows.Count),
            BulkCopyTimeout = SqlOptions.BulkCopy.BulkCopyTimeout,
            NotifyAfter = SqlOptions.BulkCopy.NotifyAfter
        };

        AddColumnMappings(bulkCopy, source);
        await bulkCopy.WriteToServerAsync(source, cancellationToken).ConfigureAwait(false);
    }

    private int GetBatchSize(int defaultSize) => SqlOptions.BulkCopy.BatchSize > 0 ? SqlOptions.BulkCopy.BatchSize : defaultSize;

    private void AddColumnMappings(SqlBulkCopy bulkCopy, DataTable source)
    {
        foreach (DataColumn column in source.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }
    }

    private string CreateTableScriptSQL(DataTable source, string targetTableName)
    {
        IEnumerable<string> columns = source.Columns.Cast<DataColumn>()
            .Select(column => $"[{column.ColumnName}] {GetSqlDataType(column)}");

        return $"IF OBJECT_ID('{targetTableName}') IS NULL CREATE TABLE {targetTableName} (\n{string.Join(",\n", columns)}\n)";
    }

}