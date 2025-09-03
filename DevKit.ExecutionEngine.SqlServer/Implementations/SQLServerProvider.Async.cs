namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Métodos asíncronos de <see cref="SQLServerProvider"/>.</summary>
public partial class SQLServerProvider
{

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

                    T item = new();
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



    /// <summary>Ejecuta un procedimiento almacenado que no devuelve resultados, de forma asíncrona.</summary>
    public async Task<int> ExecuteProcedureCommandAsync(string procedimientoAlmacenado,
        Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default)
    {
        using (SqlConnection connection = new(ConnectionString))
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