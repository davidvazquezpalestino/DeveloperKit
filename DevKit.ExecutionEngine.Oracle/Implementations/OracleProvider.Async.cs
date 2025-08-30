namespace DevKit.ExecutionEngine.Oracle.Implementations;
public partial class OracleProvider
{
    /// <inheritdoc/>
    public async Task<DataTable> ExecuteQueryAsTableAsync(string query, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = OracleOptions.CommandTimeout;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task<DataTable> ExecuteProcedureAsTableAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = OracleOptions.CommandTimeout;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    DataTable tabla = new DataTable();
                    tabla.Load(reader);
                    return tabla;
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteQueryAsDictionaryAsync(string query, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = OracleOptions.CommandTimeout;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
                {
                    List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        result.Add(row);
                    }
                    return result;
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteProcedureAsDictionaryAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = OracleOptions.CommandTimeout;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                await using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        result.Add(row);
                    }
                    return result;
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteQueryAsSingleAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.Connection = connection;
                command.CommandText = query;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (reader.HasRows && await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return expression(reader);
                    }
                }
            }
        }

        return default;
    }
    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado, CancellationToken cancellationToken = default) where T : new()
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.CommandTimeout = OracleOptions.CommandTimeout;

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return reader.GetItem<T>();
                    }
                }
            }
        }

        return new T();
    }
    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = OracleOptions.CommandTimeout;

                parameter?.Invoke(command.Parameters);

                await using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return expression(reader);
                    }

                    return default;
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                command.CommandTimeout = OracleOptions.CommandTimeout;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    List<T> result = new List<T>();

                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        result.Add(expression(reader));
                    }

                    await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                    return result;
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string procedimientoAlmacenado, CancellationToken cancellationToken = default) where T : new()
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.CommandTimeout = OracleOptions.CommandTimeout;

                List<T> items = new List<T>();

                await using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        items.Add(reader.GetItem<T>());
                    }
                    await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);

                    return items;
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = storedProcedure;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = OracleOptions.CommandTimeout;

                parameter?.Invoke(command.Parameters);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    List<T> list = new List<T>();
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        T item = expression(reader);
                        list.Add(item);
                    }

                    await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);

                    return list;
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task<int> ExecuteProcedureCommandAsync(string storedProcedure, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = storedProcedure;
                command.Transaction = Transaccion;
                command.CommandTimeout = OracleOptions.CommandTimeout;
                parameter?.Invoke(command.Parameters);

                if (ConnectionState == ConnectionState.Closed)
                {
                    await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }

                return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
    /// <inheritdoc/>
    public async Task<int> ExecuteNonQueryAsync(string command, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand dbCommand = connection.CreateCommand())
            {
                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = command;
                dbCommand.CommandTimeout = OracleOptions.CommandTimeout;
                parameter?.Invoke(dbCommand.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return await dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
    /// <inheritdoc/>
    public async Task ExecuteBulkInsertToTableAsync(DataTable source, string target, CancellationToken cancellationToken = default)
    {
        await using (OracleConnection connection = new OracleConnection(ConnectionString))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);


            await using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandTimeout = OracleOptions.CommandTimeout;
                command.CommandText = DropTableScriptSQL(target);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                command.CommandText = CreateTableScriptSQL(source, target);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection, OracleBulkCopyOptions.Default))
                {
                    bulkCopy.DestinationTableName = target;
                    bulkCopy.BatchSize = source.Rows.Count;
                    bulkCopy.BulkCopyTimeout = OracleOptions.BulkCopyTimeout;

                    foreach (DataColumn column in source.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    bulkCopy.WriteToServer(source);
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task ExecuteBulkInsertAsync(DataTable source, string target, CancellationToken cancellationToken = default)
    {
        await using (OracleConnection connection = new OracleConnection(ConnectionString))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection, OracleBulkCopyOptions.Default))
            {
                bulkCopy.DestinationTableName = target;
                bulkCopy.BatchSize = source.Rows.Count;
                bulkCopy.BulkCopyTimeout = OracleOptions.BulkCopyTimeout;

                foreach (DataColumn column in source.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                bulkCopy.WriteToServer(source);
            }
        }
    }
}