namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve una lista de entidades.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver</typeparam>
    /// <param name="storedProcedure">Nombre del procedimiento almacenado</param>
    /// <param name="expression">Función para mapear cada registro a una entidad</param>
    /// <param name="parametros">Parámetros del procedimiento</param>
    /// <returns>Lista de entidades mapeadas</returns>
    public ICollection<T> ExecuteProcedureAsList<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandText = storedProcedure;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = SqlOptions.CommandTimeout;
            dbParameters?.Invoke(command.Parameters);

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
    /// <summary>Ejecuta una consulta y devuelve una lista de entidades.</summary>
    public ICollection<T> ExecuteQueryAsList<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)
    {
        DbCommand command = Connection.CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = SqlOptions.CommandTimeout;
        dbParameters?.Invoke(command.Parameters);

        if (Connection.State == ConnectionState.Closed)
        {
            Connection.Open();
        }

        IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);

        ICollection<T> collection = new List<T>();
        while (reader.Read())
        {
            collection.Add(expression(reader));
        }
        return collection;

    }
    /// <summary>Ejecuta una consulta y devuelve los registros como colección de diccionarios.</summary>
    public ICollection<Dictionary<string, object>> ExecuteQueryAsDictionary(string query, Action<IDataParameterCollection> dbParameters = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            dbParameters?.Invoke(command.Parameters);
            command.CommandTimeout = SqlOptions.CommandTimeout;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (DbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<Dictionary<string, object>> result = new();
                while (reader.Read())
                {
                    Dictionary<string, object> row = new();
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
    /// <summary>Ejecuta un procedimiento almacenado y devuelve los registros como colección de diccionarios.</summary>
    public ICollection<Dictionary<string, object>> ExecuteProcedureAsDictionary(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            dbParameters?.Invoke(command.Parameters);
            command.CommandTimeout = SqlOptions.CommandTimeout;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (DbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                List<Dictionary<string, object>> result = new();
                while (reader.Read())
                {
                    Dictionary<string, object> row = new();
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


    /// <summary>Ejecuta una consulta y devuelve una lista de entidades de forma asíncrona.</summary>
    public async Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression,
        Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                dbParameters?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    List<T> result = new();

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
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, CancellationToken cancellationToken = default) where T : new()
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = storedProcedure;
                command.CommandTimeout = SqlOptions.CommandTimeout;

                List<T> items = new();

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

    /// <inheritdoc />>
    public async Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure,
        Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = storedProcedure;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlOptions.CommandTimeout;

                dbParameters?.Invoke(command.Parameters);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (IDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    List<T> list = new();
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
    /// <summary>Ejecuta una consulta y devuelve una colección de diccionarios de forma asíncrona.</summary>
    public async Task<ICollection<Dictionary<string, object>>> ExecuteQueryAsDictionaryAsync(string query,
        Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                dbParameters?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
                {
                    List<Dictionary<string, object>> result = new();
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        Dictionary<string, object> row = new();
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
        string storedProcedure, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = storedProcedure;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                dbParameters?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                List<Dictionary<string, object>> result = new();
                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        Dictionary<string, object> row = new();
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
}