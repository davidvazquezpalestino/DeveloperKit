namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Proporciona métodos de extensión para streaming de resultados usando IAsyncEnumerable.
/// </summary>
public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Ejecuta una consulta y devuelve los resultados como un stream asíncrono.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver.</typeparam>
    /// <param name="connection">Conexión a la base de datos.</param>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="dbParameters">Parámetros de la consulta.</param>
    /// <param name="commandTimeout">Tiempo de espera del comando.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Stream asíncrono de entidades.</returns>
    public static async IAsyncEnumerable<T> StreamAsync<T>(
        this DbConnection connection,
        string query,
        Action<IDataParameterCollection> dbParameters = null,
        int? commandTimeout = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class, new()
    {
        using DbCommand command = connection.CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;
        
        if (commandTimeout.HasValue)
            command.CommandTimeout = commandTimeout.Value;
            
        dbParameters?.Invoke(command.Parameters);

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return await reader.GetEntityAsync<T>(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve los resultados como un stream asíncrono.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver.</typeparam>
    /// <param name="connection">Conexión a la base de datos.</param>
    /// <param name="storedProcedure">Nombre del procedimiento almacenado.</param>
    /// <param name="dbParameters">Parámetros del procedimiento.</param>
    /// <param name="commandTimeout">Tiempo de espera del comando.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Stream asíncrono de entidades.</returns>
    public static async IAsyncEnumerable<T> StreamProcedureAsync<T>(
        this DbConnection connection,
        string storedProcedure,
        Action<IDataParameterCollection> dbParameters = null,
        int? commandTimeout = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class, new()
    {
        using DbCommand command = connection.CreateCommand();
        command.CommandText = storedProcedure;
        command.CommandType = CommandType.StoredProcedure;
        
        if (commandTimeout.HasValue)
            command.CommandTimeout = commandTimeout.Value;
            
        dbParameters?.Invoke(command.Parameters);

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return await reader.GetEntityAsync<T>(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Ejecuta una consulta con mapeo personalizado y devuelve los resultados como un stream asíncrono.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver.</typeparam>
    /// <param name="connection">Conexión a la base de datos.</param>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="mapper">Función de mapeo personalizada.</param>
    /// <param name="dbParameters">Parámetros de la consulta.</param>
    /// <param name="commandTimeout">Tiempo de espera del comando.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Stream asíncrono de entidades.</returns>
    public static async IAsyncEnumerable<T> StreamAsync<T>(
        this DbConnection connection,
        string query,
        Func<IDataReader, T> mapper,
        Action<IDataParameterCollection> dbParameters = null,
        int? commandTimeout = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using DbCommand command = connection.CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;
        
        if (commandTimeout.HasValue)
            command.CommandTimeout = commandTimeout.Value;
            
        dbParameters?.Invoke(command.Parameters);

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return mapper(reader);
        }
    }
}
