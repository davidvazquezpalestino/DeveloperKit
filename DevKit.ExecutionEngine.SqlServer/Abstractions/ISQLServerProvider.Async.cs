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
    Task<T> ExecuteQueryAsSingleAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado de forma asíncrona.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver</typeparam>
    /// <param name="query">Consulta SQL a ejecutar</param>
    /// <param name="parametros">Parámetros de la consulta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Primer elemento que cumple con la condición</returns>
    Task<T> FirstAsync<T>(string query, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado o un valor predeterminado si no se encuentra ningún elemento de forma asíncrona.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver</typeparam>
    /// <param name="query">Consulta SQL a ejecutar</param>
    /// <param name="parametros">Parámetros de la consulta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Primer elemento que cumple con la condición o valor predeterminado</returns>
    Task<T> FirstOrDefaultAsync<T>(string query, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Ejecuta un procedimiento almacenado de forma asíncrona y retorna la entidad mapeada.
    /// </summary>
    Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado, CancellationToken cancellationToken = default) where T : new();

    /// <summary>
    /// Ejecuta un procedimiento almacenado de forma asíncrona y mapea el resultado con la expresión indicada.
    /// </summary>
    Task<T> ExecuteProcedureAsSingleAsync<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una consulta de forma asíncrona y devuelve un DataTable.
    /// </summary>
    Task<DataTable> ExecuteQueryAsTableAsync(string query, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un procedimiento almacenado de forma asíncrona y devuelve un DataTable.
    /// </summary>
    Task<DataTable> ExecuteProcedureAsTableAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una consulta y devuelve los resultados como colección de diccionarios.
    /// </summary>
    Task<ICollection<Dictionary<string, object>>> ExecuteQueryAsDictionaryAsync(string query, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve los resultados como colección de diccionarios.
    /// </summary>
    Task<ICollection<Dictionary<string, object>>> ExecuteProcedureAsDictionaryAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una consulta y devuelve una lista de entidades.
    /// </summary>
    Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve una lista de entidades.
    /// </summary>
    Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, CancellationToken cancellationToken = default) where T : new();

    /// <summary>
    /// Ejecuta un procedimiento almacenado y mapea cada registro a la entidad indicada.
    /// </summary>
    Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un procedimiento almacenado de forma asíncrona sin esperar resultados.
    /// </summary>
    Task<int> ExecuteProcedureCommandAsync(string storedProcedure, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una copia masiva de un DataTable a la tabla destino de forma asíncrona.
    /// </summary>
    /// <param name="source">DataTable que contiene los datos a insertar.</param>
    /// <param name="target">Nombre de la tabla de destino.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>Tarea asíncrona que representa la operación.</returns>
    Task ExecuteBulkInsertToTableAsync(DataTable source, string target, CancellationToken cancellationToken = default);


    /// <summary>
    /// Ejecuta un comando de forma asíncrona sin devolver resultados.
    /// </summary>
    Task<int> ExecuteNonQueryAsync(string command, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Ejecuta una consulta y devuelve los resultados como lista de listas de diccionarios.
    /// </summary>
    Task<IList<IList<Dictionary<string, object>>>> ExecuteMultiResultQueryAsync(
        string query,
        Action<IDataParameterCollection> parametros = null,
        Action<string> logger = null,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Inserta una entidad en la tabla especificada.
    /// </summary>
    Task ExecuteInsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Inserta una colección de entidades en la tabla especificada con configuración de lote.
    /// </summary>
    Task ExecuteInsertAsync<T>(string tableName, ICollection<T> entities, int batchSize = 1000, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// Ejecuta una consulta SQL de forma asíncrona y devuelve un valor escalar.
    /// </summary>
    Task<T> ExecuteScalarAsync<T>(string query, Action<IDataParameterCollection> parameter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la fecha y hora actuales del servidor de forma asíncrona.
    /// </summary>
    Task<DateTime> GetCurrentDateTimeAsync(CancellationToken cancellationToken = default);
}