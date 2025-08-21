namespace DevKit.ExecutionEngine.MySQL;

/// <summary>
/// Provides asynchronous methods for the MySQL database provider.
/// </summary>
public partial class MySqlDatabaseProvider
{
    /// <inheritdoc/>
    public async Task<DataTable> ExecuteQueryAsTableAsync(string query, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                DataTable table = new DataTable();
                table.Load(reader);
                return table;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<DataTable> ExecuteProcedureAsTableAsync(string storedProcedure, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                DataTable table = new DataTable();
                table.Load(reader);
                return table;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteQueryAsSingleAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                return await reader.ReadAsync() ? expression(reader) : default;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                return await reader.ReadAsync() ? expression(reader) : default;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string storedProcedure) where T : new()
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                return await reader.ReadAsync() ? new T() : default;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteQueryAsDictionaryAsync(string query, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = await reader.GetFieldValueAsync<object>(i);
                    }

                    result.Add(row);
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteProcedureAsDictionaryAsync(string storedProcedure, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = await reader.GetFieldValueAsync<object>(i);
                    }

                    result.Add(row);
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                List<T> result = new List<T>();
                while (await reader.ReadAsync())
                {
                    result.Add(expression(reader));
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure) where T : new()
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                List<T> result = new List<T>();
                while (await reader.ReadAsync())
                {
                    result.Add(new T());
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                List<T> result = new List<T>();
                while (await reader.ReadAsync())
                {
                    result.Add(expression(reader));
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteNonQueryAsync(string commandText, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            return await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<int> ExecuteProcedureCommandAsync(string storedProcedure, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }

            return await command.ExecuteNonQueryAsync();
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteBulkInsertToTableAsync(DataTable source, string target)
    {
        if (Connection.State == ConnectionState.Closed)
        {
            await Connection.OpenAsync();
        }

        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandTimeout = 0;

            // Elimina la tabla si existe
            command.CommandText = DropTableScriptMySQL(target);
            await command.ExecuteNonQueryAsync();

            // Crea la tabla
            command.CommandText = CreateTableScriptMySQL(source, target);
            await command.ExecuteNonQueryAsync();
        }

        // Inserción masiva con MySqlBulkCopy (sin archivos temporales)
        MySqlBulkCopy bulkCopy = new MySqlBulkCopy(Connection)
        {
            DestinationTableName = target,
            BulkCopyTimeout = 0
        };

        // Mapear columnas por índice (ordinal)
        for (int index = 0; index < source.Columns.Count; index++)
        {
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(index, source.Columns[index].ColumnName));
        }

        await bulkCopy.WriteToServerAsync(source);

        await Connection.CloseAsync();
    }

    /// <inheritdoc/>
    public async Task ExecuteBulkInsertAsync(DataTable source, string destinationTable)
    {
        if (Connection.State == ConnectionState.Closed)
        {
            await Connection.OpenAsync();
        }

        MySqlBulkCopy bulkCopy = new MySqlBulkCopy(Connection)
        {
            DestinationTableName = destinationTable,
            BulkCopyTimeout = 0
        };

        for (int index = 0; index < source.Columns.Count; index++)
        {
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(index, source.Columns[index].ColumnName));
        }

        await bulkCopy.WriteToServerAsync(source);
        await Connection.CloseAsync();
    }
}