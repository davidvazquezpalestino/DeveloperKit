namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <inheritdoc />
    public async Task<DataSet> ExecuteQueryMultiResultAsync(
        string query,
        Action<IDataParameterCollection> dbParameters = null,
        Action<string> LogTo = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("La consulta no puede estar vacía.", nameof(query));
        }

        using (SqlConnection connection = new(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = SqlOptions.CommandTimeout;

                dbParameters?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
                {
                    // Modo DataSet
                    DataSet dataSet = new();
                    int resultSetCount = 1;

                    do
                    {
                        DataTable table = new($"Result_{resultSetCount}");
                        table.Load(reader);
                        dataSet.Tables.Add(table);
                        resultSetCount++;

                    } while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));

                    LogTo?.Invoke($"Query executed with {resultSetCount} result sets.");
                    return dataSet;
                }
            }
        }
    }

}