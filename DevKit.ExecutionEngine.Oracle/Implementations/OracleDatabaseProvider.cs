namespace DevKit.ExecutionEngine.Oracle.Implementations;

/// <summary>Implementación de OracleRepository para operaciones con Oracle Database.</summary>
public partial class OracleDatabaseProvider : IOracleDatabaseProvider
{
    private readonly OracleConnection Connection;
    private OracleTransaction Transaccion;

    /// <summary>Estado actual de la conexión a la base de datos.</summary>
    public ConnectionState ConnectionState => Connection.State;
    /// <summary>Transacción actual en curso.</summary>
    /// <summary>Cadena de conexión a la base de datos.</summary>
    public string ConnectionString { get; }

    /// <summary>Devuelve la cadena de conexión actual.</summary>
    public override string ToString() => Connection.ConnectionString;

    /// <inheritdoc/>
    public void BeginTransaction()
    {
        Connection.Open();
        Transaccion = Connection.BeginTransaction();
    }

    /// <inheritdoc/>
    public void CommitTransaction()
    {
        Transaccion.Commit();
        Connection.Close();
        Transaccion = null;
    }

    /// <inheritdoc/>
    public void RollbackTransaction()
    {
        Transaccion.Rollback();
        Transaccion = null;
        Connection.Close();
    }

    /// <summary>Ejecuta una consulta SQL y devuelve los resultados en una DataTable.</summary>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="parameter">Parámetros de la consulta (opcional).</param>
    /// <inheritdoc/>
    public DataTable ExecuteQueryAsTable(string query, Action<IDataParameterCollection> parameter = null)
    {
        try
        {
            using (DbCommand command = Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                parameter?.Invoke(command.Parameters);
                command.CommandTimeout = 0;

                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }

                using (DbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }
        finally
        {
            Connection.Close();
        }
    }

    /// <inheritdoc/>
    public void ExecuteNonQuery(string command, Action<IDataParameterCollection> parameter = null)
    {
        using (DbCommand dbCommand = Connection.CreateCommand())
        {
            dbCommand.CommandTimeout = 0;
            dbCommand.Transaction = Transaccion;
            dbCommand.CommandText = command;
            parameter?.Invoke(dbCommand.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            dbCommand.ExecuteNonQuery();
        }
    }

    /// <inheritdoc/>
    public DataTable ExecuteProcedureAsTable(string procedimientoAlmacenado, Action<IDataParameterCollection> parameter = null)
    {
        try
        {
            using (DbCommand command = Connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.CommandTimeout = 0;
                parameter?.Invoke(command.Parameters);

                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }

                DataTable tabla = new DataTable();
                IDataReader reader = command.ExecuteReader();
                tabla.Load(reader);
                return tabla;
            }
        }
        finally
        {
            Connection.Close();
        }
    }

    /// <inheritdoc/>
    public T ExecuteQueryAsSingle<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null) =>
        ExecuteQueryAsList(query, expression, parameter).FirstOrDefault();

    /// <inheritdoc/>
    public T ExecuteProcedureAsSingle<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameterCollection) =>
        ExecuteProcedureAsList(procedimientoAlmacenado, expression, parameterCollection).FirstOrDefault();

    /// <inheritdoc/>
    public ICollection<Dictionary<string, object>> ExecuteQueryAsDictionary(string query, Action<IDataParameterCollection> parameter = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameter?.Invoke(command.Parameters);
            command.CommandTimeout = 0;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (DbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int ordinal = 0; ordinal < reader.FieldCount; ordinal++)
                    {
                        row.Add(reader.GetName(ordinal), reader.GetValue(ordinal));
                    }

                    result.Add(row);
                }
                return result;
            }
        }
    }

    /// <inheritdoc/>
    public ICollection<Dictionary<string, object>> ExecuteProcedureAsDictionary(string procedimientoAlmacenado, Action<IDataParameterCollection> parameter = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = procedimientoAlmacenado;
            parameter?.Invoke(command.Parameters);
            command.CommandTimeout = 0;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (DbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int ordinal = 0; ordinal < reader.FieldCount; ordinal++)
                    {
                        row.Add(reader.GetName(ordinal), reader.GetValue(ordinal));
                    }

                    result.Add(row);
                }
                return result;
            }
        }
    }

    /// <inheritdoc/>
    public ICollection<T> ExecuteProcedureAsList<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null)
    {
        try
        {
            using (DbCommand command = Connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;

                parameter?.Invoke(command.Parameters);

                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }

                using (IDataReader reader = command.ExecuteReader())
                {
                    ICollection<T> collection = new List<T>();
                    while (reader.Read())
                    {
                        collection.Add(expression(reader));
                    }

                    return collection;
                }
            }
        }
        finally
        {
            Connection.Close();
        }
    }

    /// <summary>Ejecuta una consulta SQL y devuelve una lista de resultados.</summary>
    /// <typeparam name="T">Tipo de los elementos en la lista.</typeparam>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="expression">Función que mapea cada fila.</param>
    /// <param name="parameter">Parámetros de la consulta (opcional).</param>
    /// <inheritdoc/>
    public ICollection<T> ExecuteQueryAsList<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null)
    {
        try
        {
            using (DbCommand command = Connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 0;

                parameter?.Invoke(command.Parameters);

                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }

                using (IDataReader reader = command.ExecuteReader())
                {
                    ICollection<T> collection = new List<T>();
                    while (reader.Read())
                    {
                        collection.Add(expression(reader));
                    }

                    return collection;
                }
            }
        }
        finally
        {
            Connection.Close();
        }
    }

    /// <summary>Ejecuta un procedimiento almacenado sin devolver resultados.</summary>
    /// <param name="procedimientoAlmacenado">Nombre del procedimiento.</param>
    /// <param name="parametros">Parámetros del procedimiento (opcional).</param>
    /// <inheritdoc/>
    public void ExecuteProcedureCommand(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            try
            {
                command.Connection = Connection;
                command.CommandTimeout = 0;
                command.Transaction = Transaccion;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                parametros?.Invoke(command.Parameters);

                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }

                command.ExecuteNonQuery();
            }
            finally
            {
                command.Parameters.Clear();
                Connection.Close();
            }
        }
    }

    /// <inheritdoc/>
    public void ExecuteBulkInsertToTable(DataTable source, string target)
    {
        DropTable(target);
        CreateTable(source, target);
        ExecuteBulkInsert(source, target);
    }

    /// <summary>Realiza una copia masiva de datos desde un DataTable a una tabla de Oracle.</summary>
    /// <param name="source">DataTable fuente con los datos.</param>
    /// <param name="target">Nombre de la tabla destino.</param>
    public void ExecuteBulkInsert(DataTable source, string target)
    {
        using (OracleBulkCopy bulkCopy = new OracleBulkCopy(Connection, OracleBulkCopyOptions.Default))
        {
            bulkCopy.DestinationTableName = target;
            bulkCopy.BatchSize = source.Rows.Count;
            bulkCopy.NotifyAfter = source.Rows.Count;
            bulkCopy.BulkCopyTimeout = 0;

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
    }

    /// <summary>Inicializa una nueva instancia de <see cref="OracleDatabaseProvider"/> usando el patrón Options.</summary>
    /// <param name="options">Opciones de configuración para Oracle.</param>
    /// <exception cref="ArgumentException">Si la cadena de conexión es inválida.</exception>
    public OracleDatabaseProvider(IOptions<OracleOptions> options)
    {
        OracleOptions oracleOptions = options.Value;
        if (string.IsNullOrWhiteSpace(oracleOptions.ConnectionString))
            throw new ArgumentException("ConnectionString no puede estar vacío en las opciones.");

        OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder(oracleOptions.ConnectionString);

        // Pooling
        if (oracleOptions.ConnectionPooling != null)
        {
            builder.Pooling = oracleOptions.ConnectionPooling.Pooling;
            builder.MinPoolSize = oracleOptions.ConnectionPooling.MinPoolSize;
            builder.MaxPoolSize = oracleOptions.ConnectionPooling.MaxPoolSize;
            if (oracleOptions.ConnectionPooling.ConnectionLifetime > 0)
            {
                builder.ConnectionLifeTime = oracleOptions.ConnectionPooling.ConnectionLifetime;
            }
        }

        ConnectionString = builder.ConnectionString;
        Connection = new OracleConnection(ConnectionString);
    }

    #region Destructores
    /// <summary>Libera los recursos administrados utilizados por la instancia.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            Connection.Dispose();
        }
    }

    /// <summary>Finalizador que asegura liberar los recursos si el usuario olvidó llamar a Dispose.</summary>
    ~OracleDatabaseProvider() => Dispose(false);

    #endregion
}