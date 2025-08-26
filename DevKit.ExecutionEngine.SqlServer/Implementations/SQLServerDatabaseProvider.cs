
namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Implementación de <see cref="ISQLServerDatabaseProvider"/> para SQL Server.</summary>
public partial class SQLServerDatabaseProvider : ISQLServerDatabaseProvider
{
    private readonly SqlConnection Connection;
    private SqlTransaction Transaccion;
    private readonly SqlOptions Options;

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
    public void CommitTransaction()
    {
        Transaccion.Commit();
        Connection.Close();
        Transaccion = null;
    }
    /// <summary>Revierte la transacción y cierra la conexión.</summary>
    public void RollbackTransaction()
    {
        Transaccion.Rollback();
        Transaccion = null;
        Connection.Close();
    }

    /// <summary>Ejecuta una consulta y devuelve el resultado en un <see cref="DataTable"/>.</summary>
    public DataTable ExecuteQueryAsTable(string query, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parametros?.Invoke(command.Parameters);
            command.CommandTimeout = Options?.CommandTimeout ?? 30;

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
            command.CommandTimeout = Options?.CommandTimeout ?? 30;
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
            command.CommandTimeout = Options?.CommandTimeout ?? 30;
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
        command.CommandTimeout = 0;
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
            command.CommandTimeout = Options?.CommandTimeout ?? 30;
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
    public T ExecuteProcedureAsSingle<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null) =>
        ExecuteProcedureAsList(procedimientoAlmacenado, expression, parametros).FirstOrDefault();

    /// <summary>Ejecuta un procedimiento almacenado y devuelve una lista de entidades.</summary>
    public ICollection<T> ExecuteProcedureAsList<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandText = procedimientoAlmacenado;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = Options?.CommandTimeout ?? 30;

            parametros?.Invoke(command.Parameters);

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

    /// <summary>Inserta una entidad en la tabla especificada.</summary>
    public void ExecuteInsert<T>(string tableName, T entity) where T : class, new()
    {

        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.PropertyType.IsSimpleType())
            .ToArray();

        if (properties.Length == 0)
            throw new ArgumentException("No hay propiedades válidas para insertar.", nameof(T));

        string columns = string.Join(", ", properties.Select(p => p.Name));
        string parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        string command = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";

        ExecuteNonQuery(command, param => param.AsSqlParameters(entity));
    }

    /// <summary>Inserta una colección de entidades en la tabla especificada.</summary>
    public void ExecuteInsert<T>(string tableName, ICollection<T> collection) where T : class, new()
    {
        if (!collection.Any())
            throw new ArgumentException("La colección no puede estar vacía.", nameof(collection));

        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.PropertyType.IsSimpleType())
            .ToArray();

        if (properties.Length == 0)
            throw new ArgumentException("No hay propiedades válidas para insertar.", nameof(T));

        string columns = string.Join(", ", properties.Select(p => p.Name));
        string parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        string command = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";

        foreach (T entity in collection)
            ExecuteNonQuery(command, param => param.AsSqlParameters(entity));
    }

    /// <summary>Ejecuta un comando que no devuelve resultados.</summary>
    public void ExecuteNonQuery(string command, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand sqlCommand = Connection.CreateCommand())
        {
            sqlCommand.CommandTimeout = 0;
            sqlCommand.Transaction = Transaccion;
            sqlCommand.CommandText = command;
            parametros?.Invoke(sqlCommand.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            sqlCommand.ExecuteNonQuery();
        }
    }


    /// <summary>Copia masivamente datos de un DataTable a la tabla destino.</summary>
    public void ExecuteBulkInsertToTable(DataTable source, string target)
    {
        DropTable(target);
        CreateTable(source, target);
        ExecuteBulkInsert(source, target);
    }

    /// <summary>Copia masivamente datos de un DataTable a la tabla destino.</summary>
    public void ExecuteBulkInsert(DataTable source, string target)
    {
        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Connection, SqlBulkCopyOptions.Default, Transaccion))
        {
            int defaultBatchSize = source.Rows.Count;

            bulkCopy.DestinationTableName = target;
            bulkCopy.BatchSize = defaultBatchSize;
            bulkCopy.NotifyAfter = defaultBatchSize;
            bulkCopy.BulkCopyTimeout = Options.BulkCopy.BulkCopyTimeout;

            if (Options.BulkCopy.BatchSize > 0)
                bulkCopy.BatchSize = Options.BulkCopy.BatchSize;

            if (Options.BulkCopy.NotifyAfter > 0)
                bulkCopy.NotifyAfter = Options.BulkCopy.NotifyAfter;

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

    /// <summary>Ejecuta un procedimiento almacenado sin esperar resultados.</summary>
    public void ExecuteProcedureCommand(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {

            command.Connection = Connection;
            command.CommandTimeout = Options?.CommandTimeout ?? 30;
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
    }

    /// <summary>Obtiene la fecha y hora actuales del servidor.</summary>
    public DateTime GetCurrentDateTime()
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandText = "SELECT GETDATE()";
            command.CommandType = CommandType.Text;
            command.CommandTimeout = Options?.CommandTimeout ?? 30;

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

    /// <summary>Inicializa una nueva instancia de <see cref="SQLServerDatabaseProvider"/> usando el patrón Options.</summary>
    public SQLServerDatabaseProvider(IOptions<SqlOptions> options)
    {
        Options = options.Value;

        if (string.IsNullOrWhiteSpace(Options.ConnectionString))
            throw new ArgumentException("ConnectionString no puede estar vacío en las opciones.");

        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(Options.ConnectionString);

        // Aplicar configuraciones de pooling
        if (Options.ConnectionPooling != null)
        {
            builder.Pooling = Options.ConnectionPooling.Pooling;
            builder.MinPoolSize = Options.ConnectionPooling.MinPoolSize;
            builder.MaxPoolSize = Options.ConnectionPooling.MaxPoolSize;
        }

        // Aplicar timeouts
        builder.ConnectTimeout = Options.ConnectionTimeout;
        builder.CommandTimeout = Options.CommandTimeout;

        // Aplicar nombre de aplicación si está configurado
        if (Options.ConfigureApplication != null)
        {
            builder.ApplicationName = Options.ConfigureApplication.Invoke();
        }

        ConnectionString = Options.ConnectionString;
        Connection = new SqlConnection(Options.ConnectionString);
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
            Connection?.Dispose();
        }
    }
    /// <summary>Finalizador que asegura liberar los recursos si el usuario olvidó llamar a Dispose.</summary>
    ~SQLServerDatabaseProvider() => Dispose(false);

    #endregion
}