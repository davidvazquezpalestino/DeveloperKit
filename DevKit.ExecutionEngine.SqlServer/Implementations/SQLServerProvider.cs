namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Implementación de <see cref="ISQLServerProvider"/> para SQL Server.</summary>
public partial class SQLServerProvider : ISQLServerProvider, IAsyncDisposable
{
    private readonly ISqlConnectionFactory ConnectionFactory;
    public SqlConnection Connection { get; set; }
    public SqlTransaction Transaction { get; set; }
    private readonly SqlOptions SqlOptions;
    public SemaphoreSlim TransactionSemaphore { get; private set; }
    private bool Disposed = false;

    /// <summary>Estado actual de la conexión.</summary>
    public ConnectionState ConnectionState => Connection?.State ?? ConnectionState.Closed;

    /// <summary>Cadena de conexión utilizada por el repositorio.</summary>
    public string ConnectionString => Connection?.ConnectionString;

    /// <summary>Devuelve la cadena de conexión actual.</summary>
    public override string ToString() => ConnectionString;

    /// <summary>
    /// Obtiene la conexión actual, creándola si es necesario.
    /// </summary>
    private SqlConnection GetConnection()
    {
        if (Connection == null)
        {
            Connection = ConnectionFactory.CreateConnection(ConnectionString);
        }
        return Connection;
    }

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

            if (Connection.State == ConnectionState.Open && Transaction == null)
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
    public SQLServerProvider(IOptions<SqlOptions> options, ISqlConnectionFactory connectionFactory = null)
    {
        SqlOptions = options.Value;
        ConnectionFactory = connectionFactory ?? new DefaultSqlConnectionFactory();

        if (SqlOptions == null)
        {
            throw new ArgumentException("La configuración de SqlOptions no puede ser nula.");
        }

        if (string.IsNullOrWhiteSpace(SqlOptions.ConnectionString))
        {
            throw new ArgumentException("ConnectionString no puede estar vacío en las opciones.");
        }

        // Construir y aplicar la cadena de conexión usando los ajustes de SqlOptions.
        Connection = new SqlConnection(BuildConnectionString(SqlOptions.ConnectionString));

        // Inicializar semáforo para control de concurrencia en transacciones
        // Permitir hasta 3 transacciones concurrentes por defecto
        TransactionSemaphore = new SemaphoreSlim(3, 3);
    }

    /// <inheritdoc/>
    public void SetConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("La cadena de conexión no puede ser nula o vacía.", nameof(connectionString));
        }

        if (Transaction != null)
        {
            throw new InvalidOperationException(
                "No se puede cambiar la cadena de conexión mientras hay una transacción activa. Confirme o revierta la transacción primero.");
        }

        // Cerrar y liberar la conexión actual para que la próxima operación cree una nueva con la cadena nueva.
        if (Connection != null)
        {
            if (Connection.State != ConnectionState.Closed)
            {
                try { Connection.Close(); } catch { /* ignorar errores al cerrar */ }
            }
            Connection.Dispose();
            Connection = null;
        }

        Connection = new SqlConnection(BuildConnectionString(connectionString));
    }

    /// <summary>
    /// Aplica las configuraciones de <see cref="SqlOptions"/> (pooling, timeouts, ApplicationName)
    /// sobre una cadena de conexión base y devuelve la cadena resultante.
    /// </summary>
    private string BuildConnectionString(string baseConnectionString)
    {
        SqlConnectionStringBuilder builder = new(baseConnectionString);

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

        return builder.ConnectionString;
    }

    #region Dispose Pattern
    /// <summary>Libera los recursos administrados utilizados por la instancia.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Libera los recursos de forma asíncrona.</summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>Libera los recursos.</summary>
    /// <param name="disposing">Indica si se deben liberar recursos administrados.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Disposed)
            return;

        if (disposing)
        {
            // Liberar recursos administrados
            Transaction?.Dispose();
            TransactionSemaphore?.Dispose();
            Connection?.Dispose();
        }

        Disposed = true;
    }

    /// <summary>Libera los recursos de forma asíncrona.</summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (Disposed)
            return;

        // Liberar recursos administrados de forma asíncrona
        if (Transaction != null)
        {
            Transaction.Dispose();
            Transaction = null;
        }

        TransactionSemaphore?.Dispose();

        if (Connection != null)
        {
            Connection.Dispose();
            Connection = null;
        }
    }

    #endregion
}