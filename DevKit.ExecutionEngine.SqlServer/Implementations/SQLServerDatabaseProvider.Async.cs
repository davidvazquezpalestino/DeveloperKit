namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Métodos asíncronos de <see cref="SQLServerDatabaseProvider"/>.</summary>
public partial class SQLServerDatabaseProvider
{
    /// <summary>Ejecuta una consulta SQL y devuelve un DataTable de forma asíncrona.</summary>
    public async Task<DataTable> ExecuteQueryAsTableAsync(string query,
        Action<IDataParameterCollection> parametros = null)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 0;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync().ConfigureAwait(false);
                using (IDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false))
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }
    }

    /// <summary>Ejecuta un procedimiento almacenado y devuelve un DataTable de forma asíncrona.</summary>
    public async Task<DataTable> ExecuteProcedureAsTableAsync(string procedimientoAlmacenado,
        Action<IDataParameterCollection> parametros = null)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync().ConfigureAwait(false);

                using (DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    DataTable tabla = new DataTable();
                    tabla.Load(reader);
                    return tabla;
                }
            }
        }
    }

    /// <summary>Ejecuta una consulta y devuelve una colección de diccionarios de forma asíncrona.</summary>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteQueryAsDictionaryAsync(string query,
        Action<IDataParameterCollection> parametros = null)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 0;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync().ConfigureAwait(false);

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false))
                {
                    List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                    while (await reader.ReadAsync().ConfigureAwait(false))
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
    }

    /// <summary>Ejecuta un procedimiento almacenado y devuelve una colección de diccionarios de forma asíncrona.</summary>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteProcedureAsDictionaryAsync(
        string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync().ConfigureAwait(false);
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
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
    }

    /// <summary>Ejecuta una consulta y mapea el primer registro a una entidad de forma asíncrona.</summary>
    public async Task<T> ExecuteQueryAsSingleAsync<T>(string query, Func<IDataReader, T> expression,
        Action<IDataParameterCollection> parametros = null)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.Connection = connection;
                command.CommandText = query;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync().ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        return expression(reader);
                    }
                }
            }
        }

        return default;
    }

    /// <summary>Ejecuta una consulta y mapea el primer registro a una entidad de forma asíncrona.</summary>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado) where T : new()
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.CommandTimeout = 0;

                await connection.OpenAsync().ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        return reader.GetItem<T>();
                    }
                }
            }
        }

        return new T();
    }

    /// <summary>Ejecuta una consulta y mapea el primer registro a una entidad de forma asíncrona.</summary>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado,
        Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync().ConfigureAwait(false);
                using (IDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        return expression(reader);
                    }
                }
            }
        }

        return default;
    }

    /// <summary>Ejecuta una consulta y devuelve una lista de entidades de forma asíncrona.</summary>
    public async Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression,
        Action<IDataParameterCollection> parametros = null)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                command.CommandTimeout = 0;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync().ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
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
    }

    /// <summary>Ejecuta una consulta y devuelve una lista de entidades de forma asíncrona.</summary>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string procedimientoAlmacenado) where T : new()
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            await connection.OpenAsync();

            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.CommandTimeout = 0;

                List<T> items = new List<T>();

                using (IDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        items.Add(reader.GetItem<T>());
                    }
                }

                return items;
            }
        }
    }

    /// <summary>Ejecuta una consulta y devuelve una lista de entidades de forma asíncrona.</summary>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string procedimientoAlmacenado,
        Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;

                parametros?.Invoke(command.Parameters);
                await connection.OpenAsync().ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    List<T> list = new List<T>();
                    while (reader.Read())
                    {
                        T item = expression(reader);
                        list.Add(item);
                    }

                    return list;
                }
            }
        }
    }

    /// <summary>Ejecuta un procedimiento almacenado que no devuelve resultados, de forma asíncrona.</summary>
    public async Task<int> ExecuteProcedureCommandAsync(string procedimientoAlmacenado,
        Action<IDataParameterCollection> parametros = null)
    {
        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.Transaction = Transaccion;
                command.CommandTimeout = 0;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync().ConfigureAwait(false);
                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }

    /// <summary>Realiza una copia masiva de un DataTable a la tabla destino de forma asíncrona.</summary>
    public async Task ExecuteBulkInsertToTableAsync(DataTable source, string target)
    {
        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            await connection.OpenAsync();

            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.Transaction = transaction;
                    command.CommandText = DropTableScriptSQL(target);
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                    command.CommandText = CreateTableScriptSQL(source, target);
                    await command.ExecuteNonQueryAsync();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = target;
                        int defaultBatchSize = source.Rows.Count;
                        bulkCopy.BatchSize = defaultBatchSize;
                        if (Options.BulkCopy.BatchSize > 0)
                            bulkCopy.BatchSize = Options.BulkCopy.BatchSize;

                        bulkCopy.BulkCopyTimeout = Options.BulkCopy.BulkCopyTimeout;

                        foreach (DataColumn column in source.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                        }

                        await bulkCopy.WriteToServerAsync(source).ConfigureAwait(false);
                    }

                    transaction.Commit();
                }
            }
        }
    }

    /// <summary>Realiza una copia masiva de un DataTable a la tabla destino de forma asíncrona.</summary>
    public async Task ExecuteBulkInsertAsync(DataTable source, string target)
    {
        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            await connection.OpenAsync();

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, Transaccion))
            {
                bulkCopy.DestinationTableName = target;
                int defaultBatchSize = source.Rows.Count;
                bulkCopy.BatchSize = defaultBatchSize;
                if (Options.BulkCopy.BatchSize > 0)
                    bulkCopy.BatchSize = Options.BulkCopy.BatchSize;

                bulkCopy.BulkCopyTimeout = Options.BulkCopy.BulkCopyTimeout;

                foreach (DataColumn column in source.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                await bulkCopy.WriteToServerAsync(source).ConfigureAwait(false);
            }
        }
    }
    /// <summary>Copia masivamente datos con configuración avanzada.</summary>
    public async Task ExecuteBulkInsertAsync(DataTable source, BulkOperationsConfiguration configuration)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        if (string.IsNullOrWhiteSpace(configuration.DestinationTableName))
            throw new ArgumentException("DestinationTableName no puede estar vacío.");

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            await connection.OpenAsync();

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, Transaccion))
            {
                bulkCopy.DestinationTableName = configuration.DestinationTableName;
                bulkCopy.BatchSize = configuration.BatchSize;
                bulkCopy.BulkCopyTimeout = configuration.BulkCopyTimeout;
                bulkCopy.NotifyAfter = configuration.NotifyAfter;

                // Configurar mapeos de columnas
                if (configuration.ColumnMappings.Any())
                {
                    foreach (ColumnMapping mapping in configuration.ColumnMappings)
                    {
                        bulkCopy.ColumnMappings.Add(mapping.SourceColumn, mapping.DestinationColumn);
                    }
                }
                else
                {
                    // Mapeo automático por nombre de columna
                    foreach (DataColumn column in source.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                }


                await bulkCopy.WriteToServerAsync(source);
            }
        }
    }

    /// <summary>Copia masivamente datos desde un IDataReader con configuración avanzada.</summary>
    public async Task ExecuteBulkInsertAsync(IDataReader source, BulkOperationsConfiguration configuration)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        if (string.IsNullOrWhiteSpace(configuration.DestinationTableName))
            throw new ArgumentException("DestinationTableName no puede estar vacío.");

        if (Connection.State == ConnectionState.Closed)
        {
            Connection.Open();
        }

        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Connection, SqlBulkCopyOptions.Default, Transaccion))
        {
            bulkCopy.DestinationTableName = configuration.DestinationTableName;
            bulkCopy.BatchSize = configuration.BatchSize;
            bulkCopy.BulkCopyTimeout = configuration.BulkCopyTimeout;
            bulkCopy.NotifyAfter = configuration.NotifyAfter;

            // Configurar mapeos de columnas
            if (configuration.ColumnMappings.Any())
            {
                foreach (ColumnMapping mapping in configuration.ColumnMappings)
                {
                    bulkCopy.ColumnMappings.Add(mapping.SourceColumn, mapping.DestinationColumn);
                }
            }

            await bulkCopy.WriteToServerAsync(source);
        }
    }

    /// <summary>Copia masivamente una colección de entidades con configuración avanzada.</summary>
    public async Task ExecuteBulkInsertAsync<T>(IEnumerable<T> entities, BulkOperationsConfiguration configuration) where T : class
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        await ExecuteBulkInsertAsync(entities.ToDataTable(), configuration).ConfigureAwait(false);
    }

    /// <summary>Copia masivamente una colección de entidades con configuración fluida.</summary>
    public async Task ExecuteBulkInsertAsync<T>(IEnumerable<T> entities, Action<BulkOperationsConfigurationBuilder> configure) where T : class
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        BulkOperationsConfigurationBuilder builder = new BulkOperationsConfigurationBuilder();
        configure(builder);
        BulkOperationsConfiguration configuration = builder.Build();

        await ExecuteBulkInsertAsync(entities, configuration);
    }

    /// <summary>Inserta una colección de entidades en la tabla especificada con configuración de lote.</summary>
    public async Task ExecuteInsertAsync<T>(string tableName, ICollection<T> entities, int batchSize = 1000) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty", nameof(tableName));
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        List<T> entitiesList = entities.ToList();
        if (!entitiesList.Any()) return;

        // Procesar en lotes
        for (int i = 0; i < entitiesList.Count; i += batchSize)
        {
            IEnumerable<T> batch = entitiesList.Skip(i).Take(batchSize);
            DataTable dataTable = batch.ToDataTable();

            BulkOperationsConfiguration configuration = new BulkOperationsConfiguration
            {
                DestinationTableName = tableName,
                BatchSize = batchSize,
                BulkCopyTimeout = 300
            };
            if (Options.BulkCopy.BulkCopyTimeout > 0)
                configuration.BulkCopyTimeout = Options.BulkCopy.BulkCopyTimeout;

            await ExecuteBulkInsertAsync(dataTable, configuration);
        }
    }



    /// <summary>Obtiene la fecha y hora actuales del servidor de forma asíncrona.</summary>
    public async Task<DateTime> GetCurrentDateTimeAsync() =>
        await ExecuteQueryAsSingleAsync("SELECT GETDATE()", reader => reader.GetDateTime(0)).ConfigureAwait(false);


    /// <summary>Ejecuta una consulta que devuelve varios conjuntos de resultados y los devuelve como listas de diccionarios de forma asíncrona.</summary>
    public async Task<IList<IList<Dictionary<string, object>>>> ExecuteMultiResultQueryAsync(string query,
    Action<IDataParameterCollection> parametros = null, Action<string> logger = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("La consulta no puede estar vacía.", nameof(query));
        }

        IList<IList<Dictionary<string, object>>> results = new List<IList<Dictionary<string, object>>>();

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                parametros?.Invoke(command.Parameters);

                logger?.Invoke("Opening database connection...");
                await connection.OpenAsync();
                logger?.Invoke("Connection opened successfully.");

                logger?.Invoke("Executing reader for multi-query...");
                using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection).ConfigureAwait(false))
                {
                    logger?.Invoke("DataReader obtained.");

                    int resultIndex = 0;
                    do
                    {
                        logger?.Invoke($"Processing ResultSet #{resultIndex}...");
                        List<Dictionary<string, object>> resultSet = new List<Dictionary<string, object>>();

                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            Dictionary<string, object> record = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                record[columnName] = value;
                            }

                            resultSet.Add(record);
                        }

                        logger?.Invoke($"ResultSet #{resultIndex} contains {resultSet.Count} rows.");
                        results.Add(resultSet);
                        resultIndex++;
                    } while (await reader.NextResultAsync().ConfigureAwait(false));

                    logger?.Invoke("All ResultSets processed successfully.");
                    return results;
                }
            }
        }
    }



    /// <summary>Inserta una entidad en la tabla especificada de forma asíncrona.</summary>
    public async Task ExecuteInsertAsync<T>(string tableName, T entity)
    {
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.PropertyType.IsSimpleType())
            .ToArray();

        if (properties.Length == 0)
            throw new ArgumentException("No hay propiedades válidas para insertar.", nameof(T));

        string columns = string.Join(", ", properties.Select(p => p.Name));
        string parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        string command = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";

        await ExecuteNonQueryAsync(command, param => param.AsSqlParameters(entity)).ConfigureAwait(false);
    }

    /// <summary>Ejecuta un comando que no devuelve resultados de forma asíncrona.</summary>
    public async Task<int> ExecuteNonQueryAsync(string command, Action<IDataParameterCollection> parametros = null)
    {
        using (SqlCommand sqlCommand = Connection.CreateCommand())
        {
            sqlCommand.CommandTimeout = Options?.CommandTimeout ?? 30;
            sqlCommand.Transaction = Transaccion;
            sqlCommand.CommandText = command;
            parametros?.Invoke(sqlCommand.Parameters);

            if (Connection.State == ConnectionState.Closed)
                await Connection.OpenAsync().ConfigureAwait(false);

            return await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}