namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

/// <summary>
/// Interfaz para operaciones de streaming con IAsyncEnumerable.
/// </summary>
public partial interface ISQLServerProvider
{
    /// <summary>
    /// Ejecuta una consulta y devuelve los resultados como un stream asíncrono.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver.</typeparam>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="dbParameters">Parámetros de la consulta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Stream asíncrono de entidades.</returns>
    IAsyncEnumerable<T> StreamAsync<T>(string query, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve los resultados como un stream asíncrono.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver.</typeparam>
    /// <param name="storedProcedure">Nombre del procedimiento almacenado.</param>
    /// <param name="dbParameters">Parámetros del procedimiento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Stream asíncrono de entidades.</returns>
    IAsyncEnumerable<T> StreamProcedureAsync<T>(string storedProcedure, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Ejecuta una consulta con mapeo personalizado y devuelve los resultados como un stream asíncrono.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver.</typeparam>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="mapper">Función de mapeo personalizada.</param>
    /// <param name="dbParameters">Parámetros de la consulta.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Stream asíncrono de entidades.</returns>
    IAsyncEnumerable<T> StreamAsync<T>(string query, Func<IDataReader, T> mapper, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);
}
