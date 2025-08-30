namespace DevKit.ExecutionEngine.PostgreSQL;

public partial class PostgreSqlProvider
{

    /// <inheritdoc/>
    public async Task<T> ExecuteQueryAsSingleAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        await using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parametros?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                List<T> result = new List<T>();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    result.Add(expression(reader));
                }

                return result.FirstOrDefault();
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado, CancellationToken cancellationToken = default) where T : new()
    {
        return await ExecuteQueryAsSingleAsync($"CALL {procedimientoAlmacenado}", reader => new T(), cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteQueryAsSingleAsync($"CALL {procedimientoAlmacenado}", expression, parametros, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DataTable> ExecuteQueryAsTableAsync(string query, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        await using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parametros?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                DataTable table = new DataTable();
                table.Load(reader);
                return table;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<DataTable> ExecuteProcedureAsTableAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteQueryAsTableAsync($"CALL {procedimientoAlmacenado}", parametros, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteQueryAsDictionaryAsync(string query, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        ICollection<Dictionary<string, object>> result = await ExecuteQueryAsListAsync(query, reader =>
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dict.Add(reader.GetName(i), reader[i]);
            }
            return dict;
        }, parametros, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteProcedureAsDictionaryAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteQueryAsDictionaryAsync($"CALL {procedimientoAlmacenado}", parametros, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        await using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = Options.CommandTimeout;
            parametros?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
            {
                List<T> collection = new List<T>();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    collection.Add(expression(reader));
                }

                return collection;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, CancellationToken cancellationToken = default) where T : new()
    {
        return await ExecuteQueryAsListAsync($"CALL {storedProcedure}", reader => new T(), cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteQueryAsListAsync($"CALL {storedProcedure}", expression, parametros, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteProcedureCommandAsync(string storedProcedure, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteNonQueryAsync($"CALL {storedProcedure}", parametros, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteNonQueryAsync(string commandText, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        await using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandTimeout = Options.CommandTimeout;
            command.Transaction = Transaction;
            command.CommandText = commandText;
            parametros?.Invoke(command.Parameters);

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

        // Ensure destination table is recreated based on DataTable schema
        await DropTableAsync(target).ConfigureAwait(false);
        await CreateTableAsync(source, target).ConfigureAwait(false);

        // Perform COPY in binary format
        await ExecuteBulkInsertAsync(source, target, cancellationToken).ConfigureAwait(false);
        await Connection.CloseAsync().ConfigureAwait(false);
    }


    /// <summary>
    /// Performs a PostgreSQL binary COPY from a DataTable into the target table with enhanced handling:
    /// - Proper quoting of identifiers
    /// - Null handling via WriteNull
    /// - Type mapping from DataTable.DataType per column
    /// - Cancellation support
    /// </summary>
    /// <param name="source">Source DataTable.</param>
    /// <param name="target">Target table name. Supports "schema.table" or just "table".</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteBulkInsertAsync(DataTable source, string target, CancellationToken cancellationToken = default)
    {
        if (Connection.State == ConnectionState.Closed)
        {
            await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        string quotedTarget = QuoteQualifiedName(target);
        List<string> columnNames = source.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        string quotedColumns = string.Join(", ", columnNames.Select(QuoteIdent));

        // Build COPY command
        string copySql = $"COPY {quotedTarget} ({quotedColumns}) FROM STDIN (FORMAT BINARY)";

        await using (NpgsqlBinaryImporter writer = await Connection.BeginBinaryImportAsync(copySql, cancellationToken).ConfigureAwait(false))
        {
            foreach (DataRow row in source.Rows)
            {
                await writer.StartRowAsync(cancellationToken)
                            .ConfigureAwait(false);

                foreach (DataColumn col in source.Columns)
                {
                    object value = row[col];
                    if (value == DBNull.Value)
                    {
                        // NpgsqlBinaryImporter does not require an async variant for nulls
                        await writer.WriteNullAsync(cancellationToken);
                        continue;
                    }

                    string pgType = GetPgTypeName(col);

                    // Write with explicit type when known, fallback otherwise
                    if (!string.IsNullOrEmpty(pgType))
                    {
                        await writer.WriteAsync(value, pgType, cancellationToken)
                                    .ConfigureAwait(false);
                    }
                    else
                    {
                        await writer.WriteAsync(value, cancellationToken)
                                    .ConfigureAwait(false);
                    }
                }
            }

            await writer.CompleteAsync(cancellationToken)
                        .ConfigureAwait(false);
        }
    }
}