namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

/// <summary>
/// Interfaz que extiende IOracleRepository con operaciones asíncronas.
/// Proporciona métodos asíncronos para ejecutar consultas y procedimientos almacenados.
/// </summary>
public partial interface ISQLServerProvider
{
    /// <summary>
    /// Ejecuta una consulta de forma asíncrona y retorna la entidad resultante.
    /// </summary>
    Task<T> ExecuteQueryAsSingleAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado de forma asíncrona.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver</typeparam>
    /// <param name="query">Consulta SQL a ejecutar</param>
    /// <param name="dbParameters">Parámetros de la consulta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Primer elemento que cumple con la condición</returns>
    Task<T> FirstAsync<T>(string query, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado o un valor predeterminado si no se encuentra ningún elemento de forma asíncrona.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver</typeparam>
    /// <param name="query">Consulta SQL a ejecutar</param>
    /// <param name="dbParameters">Parámetros de la consulta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Primer elemento que cumple con la condición o valor predeterminado</returns>
    Task<T> FirstOrDefaultAsync<T>(string query, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Ejecuta un procedimiento almacenado de forma asíncrona y mapea el resultado con la expresión indicada.
    /// </summary>
    Task<T> ExecuteProcedureAsSingleAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un procedimiento almacenado de forma asíncrona sin esperar resultados.
    /// </summary>
    Task<int> ExecuteProcedureCommandAsync(string storedProcedure, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un comando de forma asíncrona sin devolver resultados.
    /// </summary>
    Task<int> ExecuteNonQueryAsync(string command, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);


    /// <summary>
    /// Ejecuta una consulta SQL de forma asíncrona y devuelve un valor escalar.
    /// </summary>
    Task<T> ExecuteScalarAsync<T>(string query, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la fecha y hora actuales del servidor de forma asíncrona.
    /// </summary>
    Task<DateTime> GetCurrentDateTimeAsync(CancellationToken cancellationToken = default);
}