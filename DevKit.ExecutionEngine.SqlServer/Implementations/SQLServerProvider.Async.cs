namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Métodos asíncronos de <see cref="SQLServerProvider"/>.</summary>
public partial class SQLServerProvider
{
    /// <summary>Ejecuta una consulta SQL y devuelve un DataTable de forma asíncrona.</summary>
    public async Task<DataTable> ExecuteQueryAsTableAsync(string query,
        Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("La consulta no puede estar vacía.", nameof(query));
        }

        bool isConnectionOwner = false;
        SqlConnection connection = Connection;

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                isConnectionOwner = true;
            }

            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = SqlOptions?.CommandTimeout ?? 30; // Valor por defecto de 30 segundos

                if (Transaccion != null)
                {
                    command.Transaction = Transaccion;
                }

                parametros?.Invoke(command.Parameters);

                using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken)
                           .ConfigureAwait(false))
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }
        finally
        {
            if (isConnectionOwner && connection?.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }

    /// <summary>Ejecuta un procedimiento almacenado y devuelve un DataTable de forma asíncrona.</summary>
    public async Task<DataTable> ExecuteProcedureAsTableAsync(string procedimientoAlmacenado,
        Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
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
        Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
                {
                    List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
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
        string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
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
        Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.Connection = connection;
                command.CommandText = query;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
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
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado, CancellationToken cancellationToken = default) where T : new()
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.CommandTimeout = SqlOptions.CommandTimeout;

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false))
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

    /// <summary>Ejecuta un procedimiento almacenado y mapea el primer registro a la entidad indicada de forma asíncrona.</summary>
    public async Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return expression(reader);
                    }

                    return default;
                }
            }
        }
    }

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado de forma asíncrona.
    /// </summary>
    public async Task<T> FirstAsync<T>(string query, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default) where T : class, new()
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        throw new InvalidOperationException("La secuencia no contiene elementos");
                    }

                    T item = new T();
                    PropertyInfo[] properties = typeof(T).GetProperties();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PropertyInfo property = properties.FirstOrDefault(p => string.Equals(p.Name, reader.GetName(i), StringComparison.OrdinalIgnoreCase));
                        if (property != null && !await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(false))
                        {
                            property.SetValue(item, Convert.ChangeType(reader[i], property.PropertyType));
                        }
                    }

                    return item;
                }
            }
        }
    }

    /// <summary>Ejecuta una consulta y devuelve una lista de entidades de forma asíncrona.</summary>
    public async Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression,
        Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                parametros?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
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
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string procedimientoAlmacenado, CancellationToken cancellationToken = default) where T : new()
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedimientoAlmacenado;
                command.CommandTimeout = SqlOptions.CommandTimeout;

                List<T> items = new List<T>();

                using (IDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
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
        Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = procedimientoAlmacenado;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlOptions.CommandTimeout;

                parametros?.Invoke(command.Parameters);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
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
        Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (SqlConnection connection = new SqlConnection(ConnectionString))
        using (SqlCommand command = connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedimientoAlmacenado;
            command.Transaction = Transaccion;
            command.CommandTimeout = SqlOptions.CommandTimeout;
            parametros?.Invoke(command.Parameters);

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado o un valor predeterminado si no se encuentra ningún elemento de forma asíncrona.
    /// </summary>
    public async Task<T> FirstOrDefaultAsync<T>(string query, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default) where T : class, new()
    {
        try
        {
            return await FirstAsync<T>(query, parametros, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException) when (typeof(T).IsClass)
        {
            return null;
        }
    }

    /// <summary>Obtiene la fecha y hora actuales del servidor de forma asíncrona.</summary>
    public async Task<DateTime> GetCurrentDateTimeAsync(CancellationToken cancellationToken = default) =>
        await ExecuteQueryAsSingleAsync("SELECT GETDATE()", reader => reader.GetDateTime(0), null, cancellationToken).ConfigureAwait(false);

    /// <summary>Ejecuta una consulta SQL de forma asíncrona y devuelve un valor escalar.</summary>
    public async Task<T> ExecuteScalarAsync<T>(string query, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        using (DbCommand command = connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            parameter?.Invoke(command.Parameters);
            command.CommandTimeout = SqlOptions.CommandTimeout;

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            object result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            if (result == null || result == DBNull.Value)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }
    }

    /// <summary>Ejecuta una consulta que devuelve varios conjuntos de resultados y los devuelve como listas de diccionarios de forma asíncrona.</summary>
    public async Task<IList<IList<Dictionary<string, object>>>> ExecuteMultiResultQueryAsync(string query,
        Action<IDataParameterCollection> parametros = null, Action<string> logger = null, CancellationToken cancellationToken = default)
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

                await connection.OpenAsync(cancellationToken)
                    .ConfigureAwait(false);

                using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
                {
                    logger?.Invoke("DataReader obtained.");

                    int resultIndex = 0;
                    do
                    {
                        logger?.Invoke($"Processing ResultSet #{resultIndex}...");
                        List<Dictionary<string, object>> resultSet = new List<Dictionary<string, object>>();

                        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
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
                    } while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));

                    logger?.Invoke("All ResultSets processed successfully.");
                    return results;
                }
            }
        }
    }



    /// <summary>Ejecuta un comando de forma asíncrona sin devolver resultados.</summary>
    public async Task<int> ExecuteNonQueryAsync(string command, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (SqlCommand sqlCommand = Connection.CreateCommand())
        {
            sqlCommand.CommandTimeout = SqlOptions.CommandTimeout;
            sqlCommand.Transaction = Transaccion;
            sqlCommand.CommandText = command;
            parametros?.Invoke(sqlCommand.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            return await sqlCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}