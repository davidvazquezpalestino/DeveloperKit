namespace DevKit.ExecutionEngine.Oracle.Abstractions;

public partial interface IOracleProvider
{
    /// <summary>
    /// Ejecuta una consulta de forma asíncrona y retorna la entidad resultante.
    /// </summary>
    Task<T> ExecuteQueryAsSingleAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);

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
    /// Copia masivamente datos de un DataTable a la tabla destino.
    /// </summary>
    Task ExecuteBulkInsertToTableAsync(DataTable source, string target, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copia masivamente datos de un DataTable a la tabla destino.
    /// </summary>
    Task ExecuteBulkInsertAsync(DataTable source, string target, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un comando de forma asíncrona sin devolver resultados.
    /// </summary>
    Task<int> ExecuteNonQueryAsync(string command, Action<IDataParameterCollection> parametros = null, CancellationToken cancellationToken = default);
}