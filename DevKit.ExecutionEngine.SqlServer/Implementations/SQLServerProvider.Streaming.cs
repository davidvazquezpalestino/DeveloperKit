namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Métodos de streaming para <see cref="SQLServerProvider"/>.</summary>
public partial class SQLServerProvider
{
    /// <summary>
    /// Ejecuta una consulta y devuelve los resultados como un stream asíncrono.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver.</typeparam>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="dbParameters">Parámetros de la consulta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Stream asíncrono de entidades.</returns>
    public async IAsyncEnumerable<T> StreamAsync<T>(string query, Action<IDataParameterCollection> dbParameters = null, [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class, new()
    {
        ThrowIfDisposed();
        
        using var connection = new SqlConnection(ConnectionString);
        await foreach (T item in connection.StreamAsync<T>(query, dbParameters, SqlOptions.CommandTimeout, cancellationToken))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve los resultados como un stream asíncrono.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver.</typeparam>
    /// <param name="storedProcedure">Nombre del procedimiento almacenado.</param>
    /// <param name="dbParameters">Parámetros del procedimiento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Stream asíncrono de entidades.</returns>
    public async IAsyncEnumerable<T> StreamProcedureAsync<T>(string storedProcedure, Action<IDataParameterCollection> dbParameters = null, [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class, new()
    {
        ThrowIfDisposed();
        
        using var connection = new SqlConnection(ConnectionString);
        await foreach (T item in connection.StreamProcedureAsync<T>(storedProcedure, dbParameters, SqlOptions.CommandTimeout, cancellationToken))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Ejecuta una consulta con mapeo personalizado y devuelve los resultados como un stream asíncrono.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver.</typeparam>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="mapper">Función de mapeo personalizada.</param>
    /// <param name="dbParameters">Parámetros de la consulta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Stream asíncrono de entidades.</returns>
    public async IAsyncEnumerable<T> StreamAsync<T>(string query, Func<IDataReader, T> mapper, Action<IDataParameterCollection> dbParameters = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        using var connection = new SqlConnection(ConnectionString);
        await foreach (T item in connection.StreamAsync(query, mapper, dbParameters, SqlOptions.CommandTimeout, cancellationToken))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Verifica si la instancia ha sido dispuesta y lanza una excepción si es así.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Lanzada si la instancia ha sido dispuesta.</exception>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SQLServerProvider));
        }
    }
}
