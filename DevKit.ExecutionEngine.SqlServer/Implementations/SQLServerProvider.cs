
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
    public string ConnectionString { get; private set; }

    /// <summary>Devuelve la cadena de conexión actual.</summary>
    public override string ToString() => Connection.ConnectionString;

    /// <summary>Inicia una transacción y abre la conexión si es necesario.</summary>
    public void BeginTransaction()
    {
        Connection.Open();
        Transaccion = Connection.BeginTransaction();
    }
    /// <summary>Confirma la transacción y cierra la conexión.</summary>
    /// <exception cref="InvalidOperationException">Se lanza cuando no hay una transacción activa.</exception>
    public void CommitTransaction()
    {
        if (Transaccion == null)
        {
            throw new InvalidOperationException("No hay una transacción activa para confirmar.");
        }

        try
        {
            Transaccion.Commit();
        }
        finally
        {
            Transaccion.Dispose();
            Transaccion = null;

            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }
    }
    /// <summary>Revierte la transacción y cierra la conexión.</summary>
    /// <exception cref="InvalidOperationException">Se lanza cuando no hay una transacción activa.</exception>
    public void RollbackTransaction()
    {
        if (Transaccion == null)
        {
            throw new InvalidOperationException("No hay una transacción activa para revertir.");
        }

        try
        {
            Transaccion.Rollback();
        }
        finally
        {
            Transaccion.Dispose();
            Transaccion = null;

            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }
    }

    /// <summary>Ejecuta una consulta y devuelve el resultado en un <see cref="DataTable"/>.</summary>
    public DataTable ExecuteQueryAsTable(string query, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parametros?.Invoke(command.Parameters);
            command.CommandTimeout = SqlOptions.CommandTimeout;

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

    /// <summary>Ejecuta un procedimiento almacenado y devuelve el resultado en un <see cref="DataTable"/>.</summary>
    public DataTable ExecuteProcedureAsTable(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedimientoAlmacenado;
            command.CommandTimeout = SqlOptions.CommandTimeout;
            parametros?.Invoke(command.Parameters);

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

    /// <summary>Ejecuta un procedimiento almacenado y devuelve los registros como colección de diccionarios.</summary>
    public ICollection<Dictionary<string, object>> ExecuteProcedureAsDictionary(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedimientoAlmacenado;
            parametros?.Invoke(command.Parameters);
            command.CommandTimeout = SqlOptions.CommandTimeout;
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

    /// <summary>Ejecuta una consulta y devuelve un <see cref="IDataReader"/> abierto.</summary>
    public IDataReader ExecuteQueryAsList(string query, Action<IDataParameterCollection> parametros = null)
    {
        DbCommand command = Connection.CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = SqlOptions.CommandTimeout;
        parametros?.Invoke(command.Parameters);

        if (Connection.State == ConnectionState.Closed)
        {
            Connection.Open();
        }

        return command.ExecuteReader(CommandBehavior.CloseConnection);
    }

    /// <summary>Ejecuta una consulta y mapea el primer registro a la entidad indicada.</summary>
    public T ExecuteQueryAsSingle<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null) =>
        ExecuteQueryAsList(query, expression, parametros).FirstOrDefault();

    /// <summary>Ejecuta una consulta y devuelve los registros como colección de diccionarios.</summary>
    public ICollection<Dictionary<string, object>> ExecuteQueryAsDictionary(string query, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parametros?.Invoke(command.Parameters);
            command.CommandTimeout = SqlOptions.CommandTimeout;
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

    /// <summary>Ejecuta un procedimiento almacenado y mapea el primer registro a la entidad indicada.</summary>
    public T ExecuteProcedureAsSingle<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)
    {
        return ExecuteProcedureAsList(procedimientoAlmacenado, expression, parametros).FirstOrDefault();
    }

    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve una lista de entidades.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver</typeparam>
    /// <param name="procedimientoAlmacenado">Nombre del procedimiento almacenado</param>
    /// <param name="expression">Función para mapear cada registro a una entidad</param>
    /// <param name="parametros">Parámetros del procedimiento</param>
    /// <returns>Lista de entidades mapeadas</returns>
    public ICollection<T> ExecuteProcedureAsList<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandText = procedimientoAlmacenado;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = SqlOptions.CommandTimeout;
            parametros?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                ICollection<T> results = new List<T>();
                while (reader.Read())
                {
                    results.Add(expression(reader));
                }
                return results;
            }
        }
    }

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado.
    /// </summary>
    public T First<T>(string query, Action<IDataParameterCollection> parametros = null) where T : class, new()
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = SqlOptions.CommandTimeout;
            parametros?.Invoke(command.Parameters);

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
    public T FirstOrDefault<T>(string query, Action<IDataParameterCollection> parametros = null) where T : class, new()
    {
        try
        {
            return First<T>(query, parametros);
        }
        catch (InvalidOperationException) when (typeof(T).IsClass)
        {
            return null;
        }
    }

    /// <summary>Ejecuta una consulta y devuelve una lista de entidades.</summary>
    public ICollection<T> ExecuteQueryAsList<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)
    {
        using (IDataReader reader = ExecuteQueryAsList(query, parametros))
        {
            ICollection<T> collection = new List<T>();
            while (reader.Read())
            {
                collection.Add(expression(reader));
            }
            return collection;
        }
    }

    /// <summary>Ejecuta un comando que no devuelve resultados.</summary>
    public void ExecuteNonQuery(string command, Action<IDataParameterCollection> parametros = null)
    {
        bool isConnectionOwner = false;
        try
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
                isConnectionOwner = true;
            }

            using (DbCommand sqlCommand = Connection.CreateCommand())
            {
                sqlCommand.CommandTimeout = SqlOptions.CommandTimeout;
                sqlCommand.Transaction = Transaccion;
                sqlCommand.CommandText = command;
                sqlCommand.CommandType = CommandType.Text;
                parametros?.Invoke(sqlCommand.Parameters);

                sqlCommand.ExecuteNonQuery();
            }
        }
        finally
        {
            if (isConnectionOwner && Connection?.State == ConnectionState.Open)
            {
                Connection.Close();
            }
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

        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(SqlOptions.ConnectionString);

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