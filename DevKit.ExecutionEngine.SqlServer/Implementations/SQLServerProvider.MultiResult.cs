namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <inheritdoc />
    public async Task<DataSet> ExecuteQueryMultiResultAsync(
        string query,
        Action<IDataParameterCollection> parametros = null,
        Action<string> logger = null,
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

                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken);

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken))
                {
                    // Modo DataSet
                    DataSet dataSet = new();
                    int resultSetCount = 0;

                    do
                    {
                        DataTable table = new($"Result_{resultSetCount}");
                        table.Load(reader);
                        dataSet.Tables.Add(table);
                        resultSetCount++;

                    } while (await reader.NextResultAsync(cancellationToken));

                    logger?.Invoke($"Query executed with {resultSetCount} result sets.");
                    return dataSet;
                }
            }
        }
    }

}