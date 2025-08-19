namespace DevKit.ExecutionEngine.MySql;

/// <inheritdoc/>
public partial class MySqlDatabaseProvider : IMySqlDatabaseProvider
{
    private readonly MySqlOptions Options;
    private readonly MySqlConnection Connection;
    private MySqlTransaction Transaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseProvider"/> class.
    /// </summary>
    /// <param name="options">The MySQL configuration options.</param>
    public MySqlDatabaseProvider(IOptions<MySqlOptions> options)
    {
        Options = options.Value;
        ConnectionString = BuildConnectionString();
        Connection = new MySqlConnection(ConnectionString);
    }

    /// <inheritdoc/>
    public ConnectionState ConnectionState => Connection?.State ?? ConnectionState.Closed;

    /// <inheritdoc/>
    public string ConnectionString { get; }

    /// <inheritdoc/>
    public void BeginTransaction()
    {
        if (Connection.State != ConnectionState.Open)
        {
            Connection.Open();
        }
        Transaction = Connection.BeginTransaction();
    }

    /// <inheritdoc/>
    public void CommitTransaction()
    {
        Transaction?.Commit();
        Connection?.Close();
    }

    /// <inheritdoc/>
    public void RollbackTransaction()
    {
        Transaction?.Rollback();
        Connection?.Close();
    }

    /// <inheritdoc/>
    public DataTable ExecuteQueryAsTable(string query, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (MySqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
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
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (MySqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                DataTable table = new DataTable();
                table.Load(reader);
                return table;
            }
        }
    }

    /// <inheritdoc/>
    public T ExecuteQueryAsSingle<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (MySqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                return reader.Read() ? expression(reader) : default;
            }
        }
    }

    /// <inheritdoc/>
    public T ExecuteProcedureAsSingle<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (MySqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                return reader.Read() ? expression(reader) : default;
            }
        }
    }

    /// <inheritdoc/>
    public ICollection<Dictionary<string, object>> ExecuteQueryAsDictionary(string query, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (MySqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
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
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (MySqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }

                    result.Add(row);
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public ICollection<T> ExecuteProcedureAsList<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (MySqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<T> result = new List<T>();
                while (reader.Read())
                {
                    result.Add(expression(reader));
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public ICollection<T> ExecuteQueryAsList<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (MySqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<T> result = new List<T>();
                while (reader.Read())
                {
                    result.Add(expression(reader));
                }

                return result;
            }
        }
    }

    /// <inheritdoc/>
    public void ExecuteNonQuery(string commandText, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.ExecuteNonQuery();
            Connection.Close();
        }
    }

    /// <inheritdoc/>
    public void ExecuteProcedureCommand(string storedProcedure, Action<IDataParameterCollection> parameters = null)
    {
        using (MySqlCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            parameters?.Invoke(command.Parameters);
            command.CommandTimeout = Options.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.ExecuteNonQuery();
            Connection.Close();
        }
    }

    /// <inheritdoc/>
    public void ExecuteBulkInsert(DataTable source, string target)
    {
        if (Connection.State == ConnectionState.Closed)
        {
            Connection.Open();
        }

        // Inserción masiva con MySqlBulkCopy (sin archivos temporales)
        MySqlBulkCopy bulkCopy = new MySqlBulkCopy(Connection)
        {
            DestinationTableName = target,
            BulkCopyTimeout = 0
        };

        // Mapear columnas por nombre
        for (int index = 0; index < source.Columns.Count; index++)
        {
            bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(index, source.Columns[index].ColumnName));
        }

        bulkCopy.WriteToServer(source);
        Connection.Close();
    }

    /// <inheritdoc/>
    public void ExecuteBulkInsertToTable(DataTable source, string target)
    {
        DropTable(target);
        CreateTable(source, target);
        ExecuteBulkInsert(source, target);
    }



    private string BuildConnectionString()
    {
        MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(Options.ConnectionString);

        if (Options.ConnectionPooling != null)
        {
            builder.Pooling = Options.ConnectionPooling.Pooling;
            builder.MinimumPoolSize = (uint)Options.ConnectionPooling.MinPoolSize;
            builder.MaximumPoolSize = (uint)Options.ConnectionPooling.MaxPoolSize;
        }

        // Propaga configuración avanzada para operaciones bulk
        if (Options.BulkCopy.AllowLoadLocalInfile)
        {
            builder.AllowLoadLocalInfile = true;
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Releases the resources used by the provider.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the provider and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Transaction?.Dispose();
            Connection?.Dispose();
        }
    }
}

