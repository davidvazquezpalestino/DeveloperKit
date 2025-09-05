namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Implementación de <see cref="ISQLServerProvider"/> para SQL Server.</summary>
public partial class SQLServerProvider : ISQLServerProvider
{
    private readonly SqlConnection Connection;
    private SqlTransaction Transaccion;
    private readonly SqlOptions SqlOptions;

    /// <summary>Estado actual de la conexión.</summary>
    public ConnectionState ConnectionState => Connection.State;
    /// <summary>Cadena de conexión utilizada por el repositorio.</summary>
    public string ConnectionString { get; }

    /// <summary>Devuelve la cadena de conexión actual.</summary>
    public override string ToString() => Connection.ConnectionString;

    /// <summary>Ejecuta una consulta y mapea el primer registro a la entidad indicada.</summary>
    public T ExecuteQueryAsSingle<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null) =>
        ExecuteQueryAsList(query, expression, dbParameters).FirstOrDefault();


    /// <summary>Ejecuta un procedimiento almacenado y mapea el primer registro a la entidad indicada.</summary>
    public T ExecuteProcedureAsSingle<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)
    {
        return ExecuteProcedureAsList(storedProcedure, expression, dbParameters).FirstOrDefault();
    }

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado.
    /// </summary>
    public T First<T>(string query, Action<IDataParameterCollection> dbParameters = null) where T : class, new()
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = SqlOptions.CommandTimeout;
            dbParameters?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (IDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow | CommandBehavior.CloseConnection))
            {
                if (!reader.Read())
                {
                    throw new InvalidOperationException("La secuencia no contiene elementos");
                }

                return reader.GetItem<T>();
            }
        }
    }

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado o un valor predeterminado si no se encuentra ningún elemento.
    /// </summary>
    public T FirstOrDefault<T>(string query, Action<IDataParameterCollection> dbParameters = null) where T : class, new()
    {
        try
        {
            return First<T>(query, dbParameters);
        }
        catch (InvalidOperationException) when (typeof(T).IsClass)
        {
            return null;
        }
    }

    /// <summary>Obtiene la fecha y hora actuales del servidor.</summary>
    public DateTime GetCurrentDateTime()
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandText = "SELECT GETDATE()";
            command.CommandType = CommandType.Text;
            command.CommandTimeout = SqlOptions.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                if (reader.Read())
                {
                    return reader.GetDateTime(0);
                }
            }
            return DateTime.UtcNow;
        }
    }

    /// <summary>Ejecuta una consulta SQL y devuelve un valor escalar.</summary>
    public T ExecuteScalar<T>(string query, Action<IDataParameterCollection> parameter = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameter?.Invoke(command.Parameters);
            command.CommandTimeout = SqlOptions.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            object result = command.ExecuteScalar();

            if (Connection.State == ConnectionState.Open && Transaccion == null)
            {
                Connection.Close();
            }

            if (result == null || result == DBNull.Value)
            {
                return default;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }
    }

    /// <summary>Inicializa una nueva instancia de <see cref="SQLServerProvider"/> usando el patrón SqlOptions.</summary>
    public SQLServerProvider(IOptions<SqlOptions> options)
    {
        SqlOptions = options.Value;

        if (SqlOptions == null)
        {
            throw new ArgumentException("La configuración de SqlOptions no puede ser nula.");
        }

        if (string.IsNullOrWhiteSpace(SqlOptions.ConnectionString))
        {
            throw new ArgumentException("ConnectionString no puede estar vacío en las opciones.");
        }

        SqlConnectionStringBuilder builder = new(SqlOptions.ConnectionString);

        // Aplicar configuraciones de pooling
        if (SqlOptions.ConnectionPooling != null)
        {
            builder.Pooling = SqlOptions.ConnectionPooling.Pooling;
            builder.MinPoolSize = SqlOptions.ConnectionPooling.MinPoolSize;
            builder.MaxPoolSize = SqlOptions.ConnectionPooling.MaxPoolSize;
        }

        // Aplicar timeouts
        if (SqlOptions.ConnectionTimeout > 0)
        {
            builder.ConnectTimeout = SqlOptions.ConnectionTimeout;
        }

        if (SqlOptions.CommandTimeout > 0)
        {
            builder.CommandTimeout = SqlOptions.CommandTimeout;
        }

        // Aplicar nombre de aplicación si está configurado
        if (SqlOptions.ConfigureApplication != null)
        {
            string appName = SqlOptions.ConfigureApplication.Invoke();
            if (!string.IsNullOrWhiteSpace(appName))
            {
                builder.ApplicationName = appName;
            }
        }

        // Usar la cadena de conexión construida
        ConnectionString = builder.ConnectionString;
        Connection = new SqlConnection(ConnectionString);
    }

    #region Destructores
    /// <summary>Libera los recursos administrados utilizados por la instancia.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    /// <summary>Libera los recursos.</summary>
    /// <param name="disposing">Indica si se deben liberar recursos administrados.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Liberar recursos administrados
            Transaccion?.Dispose();
            Connection?.Dispose();
        }
    }
    /// <summary>Finalizador que asegura liberar los recursos si el usuario olvidó llamar a Dispose.</summary>
    ~SQLServerProvider() => Dispose(false);

    #endregion
}