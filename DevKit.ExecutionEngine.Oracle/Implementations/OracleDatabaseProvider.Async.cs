namespace DevKit.ExecutionEngine.Oracle.Implementations;
public partial class OracleDatabaseProvider
{
    /// <inheritdoc/>
    public async Task<DataTable> ExecuteQueryAsTableAsync(string query, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 0;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync();

                using (IDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task<DataTable> ExecuteProcedureAsTableAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync();

                await using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    DataTable tabla = new DataTable();
                    tabla.Load(reader);
                    return tabla;
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteQueryAsDictionaryAsync(string query, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 0;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync();

                await using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                    while (await reader.ReadAsync())
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
    public async Task<ICollection<Dictionary<string, object>>> ExecuteProcedureAsDictionaryAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync();
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                await using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await reader.ReadAsync())
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
    public async Task<T> ExecuteQueryAsSingleAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.Connection = connection;
                command.CommandText = query;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync();

                await using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows && await reader.ReadAsync())
                    {
                        return expression(reader);
                    }
                }
            }
        }

        return default;
    }
    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado) where T : new()
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.CommandTimeout = 0;

                await connection.OpenAsync();

                await using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    if (await reader.ReadAsync())
                    {
                        return reader.GetItem<T>();
                    }
                }
            }
        }

        return new T();
    }
    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await connection.OpenAsync();

            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;

                parameter?.Invoke(command.Parameters);

                await using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    if (await reader.ReadAsync())
                    {
                        return expression(reader);
                    }

                    return default;
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                command.CommandTimeout = 0;
                parameter?.Invoke(command.Parameters);

                await connection.OpenAsync();

                await using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    List<T> result = new List<T>();

                    while (await reader.ReadAsync())
                    {
                        result.Add(expression(reader));
                    }

                    await reader.NextResultAsync();
                    return result;
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string procedimientoAlmacenado) where T : new()
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await connection.OpenAsync();

            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.CommandTimeout = 0;

                List<T> items = new List<T>();

                await using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(reader.GetItem<T>());
                    }
                    await reader.NextResultAsync();

                    return items;
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;

                parameter?.Invoke(command.Parameters);
                await connection.OpenAsync();

                await using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    List<T> list = new List<T>();
                    while (await reader.ReadAsync())
                    {
                        T item = expression(reader);
                        list.Add(item);
                    }

                    await reader.NextResultAsync();

                    return list;
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task<int> ExecuteProcedureCommandAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.Transaction = Transaccion;
                command.CommandTimeout = 0;
                parameter?.Invoke(command.Parameters);

                if (ConnectionState == ConnectionState.Closed)
                {
                    await Connection.OpenAsync();
                }

                return await command.ExecuteNonQueryAsync();
            }
        }
    }
    /// <inheritdoc/>
    public async Task<int> ExecuteNonQueryAsync(string commandToExecute, Action<IDataParameterCollection> parameter = null)
    {
        await using (DbConnection connection = new OracleConnection(ConnectionString))
        {
            await using (DbCommand dbCommand = connection.CreateCommand())
            {
                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = commandToExecute;
                dbCommand.CommandTimeout = 0;
                parameter?.Invoke(dbCommand.Parameters);

                await connection.OpenAsync();
                return await dbCommand.ExecuteNonQueryAsync();
            }
        }
    }
    /// <inheritdoc/>
    public async Task ExecuteBulkInsertToTableAsync(DataTable source, string target)
    {
        await using (OracleConnection connection = new OracleConnection(ConnectionString))
        {
            await connection.OpenAsync();


            await using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandTimeout = 0;
                command.CommandText = DropTableScriptSQL(target);
                await command.ExecuteNonQueryAsync();

                command.CommandText = CreateTableScriptSQL(source, target);
                await command.ExecuteNonQueryAsync();

                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection, OracleBulkCopyOptions.Default))
                {
                    bulkCopy.DestinationTableName = target;
                    bulkCopy.BatchSize = source.Rows.Count;
                    bulkCopy.BulkCopyTimeout = 0;

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
    public async Task ExecuteBulkInsertAsync(DataTable source, string target)
    {
        await using (OracleConnection connection = new OracleConnection(ConnectionString))
        {
            await connection.OpenAsync();

            using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection, OracleBulkCopyOptions.Default))
            {
                bulkCopy.DestinationTableName = target;
                bulkCopy.BatchSize = source.Rows.Count;
                bulkCopy.BulkCopyTimeout = 0;

                foreach (DataColumn column in source.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                bulkCopy.WriteToServer(source);
            }
        }
    }
}