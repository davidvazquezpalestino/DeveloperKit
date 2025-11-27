namespace DevKit.ExecutionEngine.MySQL.Implementations;

/// <summary>
/// Provides asynchronous methods for the MySQL database provider.
/// </summary>
public partial class MySqlProvider
{
    /// <inheritdoc/>
    public async Task<DataTable> ExecuteQueryAsTableAsync(string query, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                DataTable table = new DataTable();
                table.Load(reader);
                return table;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<DataTable> ExecuteProcedureAsTableAsync(string storedProcedure, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                DataTable table = new DataTable();
                table.Load(reader);
                return table;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteQueryAsSingleAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? expression(reader) : default;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? expression(reader) : default;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string storedProcedure, CancellationToken cancellationToken = default) where T : new()
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? new T() : default;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteQueryAsDictionaryAsync(string query, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = await reader.GetFieldValueAsync<object>(i, cancellationToken).ConfigureAwait(false);
                    }

                    result.Add(row);
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteProcedureAsDictionaryAsync(string storedProcedure, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = await reader.GetFieldValueAsync<object>(i, cancellationToken).ConfigureAwait(false);
                    }

                    result.Add(row);
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                List<T> result = new List<T>();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    result.Add(expression(reader));
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, CancellationToken cancellationToken = default) where T : new()
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                List<T> result = new List<T>();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    result.Add(new T());
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                List<T> result = new List<T>();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    result.Add(expression(reader));
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteScalarAsync<T>(string query, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            try
            {
                object result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                return result == null || result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
            }
            finally
            {
                if (Connection.State == ConnectionState.Open && Transaction == null)
                {
                    await Connection.CloseAsync().ConfigureAwait(false);
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteNonQueryAsync(string commandText, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteProcedureCommandAsync(string storedProcedure, Action<IDataParameterCollection> parameters = null, CancellationToken cancellationToken = default)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteBulkInsertToTableAsync(DataTable source, string target, CancellationToken cancellationToken = default)
    {
        if (Connection.State == ConnectionState.Closed)
        {
            await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        // Detect temporary-table marker (#) and prepare sanitized table name
        bool temporary = !string.IsNullOrWhiteSpace(target) && target.StartsWith("#");
        string sanitizedTarget = temporary ? target.TrimStart('#').Trim() : target?.Trim();

        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandTimeout = Options.CommandTimeout;

            // Elimina la tabla si existe
            if (temporary)
            {
                // Use DROP TEMPORARY TABLE for temporary tables
                string dropTemp = $"DROP TEMPORARY TABLE IF EXISTS `{sanitizedTarget.Replace("`", "``")}`;";
                command.CommandText = dropTemp;
            }
            else
            {
                command.CommandText = DropTableScriptMySQL(sanitizedTarget);
            }

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            // Crea la tabla
            string createSql = CreateTableScriptMySQL(source, sanitizedTarget);
            if (temporary)
            {
                // Replace the CREATE TABLE with CREATE TEMPORARY TABLE (case-insensitive)
                createSql = createSql.Replace("CREATE TABLE", "CREATE TEMPORARY TABLE", StringComparison.InvariantCultureIgnoreCase);
            }

            command.CommandText = createSql;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        // Inserción masiva con MySqlBulkCopy (sin archivos temporales)
        MySqlBulkCopy bulkCopy = new MySqlBulkCopy(Connection)
        {
            DestinationTableName = sanitizedTarget,
            BulkCopyTimeout = Options.BulkCopy.BulkCopyTimeout
        };

        // Mapear columnas por índice (ordinal)
        for (int index = 0; index < source.Columns.Count; index++)
        {
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(index, source.Columns[index].ColumnName));
        }

        await bulkCopy.WriteToServerAsync(source, cancellationToken)
            .ConfigureAwait(false);

        await Connection.CloseAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ExecuteBulkInsertAsync(DataTable source, string destinationTable, CancellationToken cancellationToken = default)
    {
        if (Connection.State == ConnectionState.Closed)
        {
            await Connection.OpenAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        MySqlBulkCopy bulkCopy = new MySqlBulkCopy(Connection)
        {
            DestinationTableName = destinationTable,
            BulkCopyTimeout = Options.BulkCopy.BulkCopyTimeout
        };

        for (int index = 0; index < source.Columns.Count; index++)
        {
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(index, source.Columns[index].ColumnName));
        }

        await bulkCopy.WriteToServerAsync(source, cancellationToken)
                        .ConfigureAwait(false);

        await Connection.CloseAsync()
            .ConfigureAwait(false);
    }
}