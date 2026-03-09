namespace DevKit.ExecutionEngine.PostgreSQL;
/// <summary>
/// Provides a data access layer for PostgreSQL, implementing the <see cref="IPostgreSqlProvider"/> interface.
/// </summary>
public partial class PostgreSqlProvider : IPostgreSqlProvider
{
    private NpgsqlConnection Connection;
    private NpgsqlTransaction Transaction;
    private readonly PostgreOptions Options;

    /// <inheritdoc/>
    public ConnectionState ConnectionState => Connection.State;

    /// <inheritdoc/>
    public string ConnectionString { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlProvider"/> class.
    /// </summary>
    /// <param name="options">The configuration options for the PostgreSQL provider.</param>
    /// <exception cref="ArgumentException">Thrown when the connection string is not provided in the options.</exception>
    public PostgreSqlProvider(IOptions<PostgreOptions> options)
    {
        Options = options.Value;
        if (string.IsNullOrWhiteSpace(Options.ConnectionString))
        {
            throw new ArgumentException("ConnectionString cannot be empty in options.");
        }

        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(Options.ConnectionString);

        if (Options.ConnectionPooling != null)
        {
            builder.Pooling = Options.ConnectionPooling.Pooling;
            builder.MinPoolSize = Options.ConnectionPooling.MinPoolSize;
            builder.MaxPoolSize = Options.ConnectionPooling.MaxPoolSize;
        }

        builder.Timeout = Options.CommandTimeout;

        if (Options.ConfigureApplication != null)
        {
            builder.ApplicationName = Options.ConfigureApplication.Invoke();
        }

        ConnectionString = builder.ConnectionString;
        Connection = new NpgsqlConnection(ConnectionString);
    }


    /// <inheritdoc/>
    public void BeginTransaction()
    {
        if (Connection.State == ConnectionState.Closed)
        {
            Connection.Open();
        }
        Transaction = Connection.BeginTransaction();
    }

    /// <inheritdoc/>
    public void CommitTransaction()
    {
        Transaction?.Commit();
        Connection.Close();
        Transaction = null;
    }

    /// <inheritdoc/>
    public void RollbackTransaction()
    {
        Transaction?.Rollback();
        Connection.Close();
        Transaction = null;
    }

    /// <inheritdoc/>
    public DataTable ExecuteQueryAsTable(string query, Action<IDataParameterCollection> parameters = null)
    {
        using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                DataTable table = new DataTable();
                table.Load(reader);
                return table;
            }
        }
    }

    /// <inheritdoc/>
    public DataTable ExecuteProcedureAsTable(string storedProcedure, Action<IDataParameterCollection> parameters = null)
    {
        using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            command.CommandTimeout = Options.CommandTimeout;
            parameters?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            DataTable table = new DataTable();
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                table.Load(reader);
            }
            return table;
        }
    }

    /// <inheritdoc/>
    public T ExecuteQueryAsSingle<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null) =>
        ExecuteQueryAsList(query, expression, parameters).FirstOrDefault();

    /// <inheritdoc/>
    public T ExecuteProcedureAsSingle<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null) =>
        ExecuteProcedureAsList(storedProcedure, expression, parameters).FirstOrDefault();

    /// <inheritdoc/>
    public ICollection<Dictionary<string, object>> ExecuteQueryAsDictionary(string query, Action<IDataParameterCollection> parameters = null)
    {
        using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                while (reader.Read())
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

    /// <inheritdoc/>
    public ICollection<Dictionary<string, object>> ExecuteProcedureAsDictionary(string storedProcedure, Action<IDataParameterCollection> parameters = null)
    {
        // Npgsql treats functions and procedures similarly. This method can be an alias for ExecuteQueryAsDictionary for simplicity if procedures return result sets.
        return ExecuteQueryAsDictionary($"SELECT * FROM {storedProcedure}", parameters);
    }

    /// <inheritdoc/>
    public ICollection<T> ExecuteProcedureAsList<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandText = storedProcedure;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = Options.CommandTimeout;
            parameters?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                List<T> collection = new List<T>();
                while (reader.Read())
                {
                    collection.Add(expression(reader));
                }
                return collection;
            }
        }
    }

    /// <inheritdoc/>
    public ICollection<T> ExecuteQueryAsList<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = Options.CommandTimeout;
            parameters?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<T> collection = new List<T>();
                while (reader.Read())
                {
                    collection.Add(expression(reader));
                }
                return collection;
            }
        }
    }

    /// <inheritdoc/>
    public void ExecuteNonQuery(string commandText, Action<IDataParameterCollection> parameters = null)
    {
        using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.CommandTimeout = Options.CommandTimeout;
            command.Transaction = Transaction;
            command.CommandText = commandText;
            parameters?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.ExecuteNonQuery();
        }
    }

    /// <inheritdoc/>
    public void ExecuteProcedureCommand(string storedProcedure, Action<IDataParameterCollection> parameters = null)
    {
        using (NpgsqlCommand command = Connection.CreateCommand())
        {
            command.Connection = Connection;
            command.CommandTimeout = Options.CommandTimeout;
            command.Transaction = Transaction;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.ExecuteNonQuery();
        }
    }

    /// <inheritdoc/>
    public void ExecuteBulkInsertToTable(DataTable source, string target)
    {
        DropTable(target);
        CreateTable(source, target);
        ExecuteBulkInsert(source, target);
    }

    /// <inheritdoc/>
    public void ExecuteBulkInsert(DataTable source, string target)
    {
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            string quotedTarget = QuoteQualifiedName(target);
            List<string> columnNames = source.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            string quotedColumns = string.Join(", ", columnNames.Select(QuoteIdent));

            // Build COPY command
            string copySql = $"COPY {quotedTarget} ({quotedColumns}) FROM STDIN (FORMAT BINARY)";

            using (NpgsqlBinaryImporter writer = Connection.BeginBinaryImport(copySql))
            {
                foreach (DataRow row in source.Rows)
                {
                    writer.StartRow();
                    foreach (DataColumn col in source.Columns)
                    {
                        object value = row[col];
                        if (value == DBNull.Value)
                        {
                            writer.WriteNullAsync();
                            continue;
                        }

                        string pgType = GetPgTypeName(col);

                        // Write with explicit type when known, fallback otherwise
                        if (!string.IsNullOrEmpty(pgType))
                        {
                            writer.Write(value, pgType);
                        }
                        else
                        {
                            writer.Write(value);
                        }
                    }
                }

                writer.Complete();
            }
        }
    }

    /// <summary>
    /// Releases the resources used by the provider asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Connection != null)
        {
            await Connection.DisposeAsync();
            Connection = null;
        }
    }

    /// <summary>
    /// Releases the resources used by the provider.
    /// </summary>
    public void Dispose()
    {
        Connection?.Dispose();
    }
}
