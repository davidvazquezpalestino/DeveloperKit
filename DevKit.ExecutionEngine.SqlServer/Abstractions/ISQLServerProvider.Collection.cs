namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

public partial interface ISQLServerProvider
{
    /// <summary>Ejecuta un procedimiento almacenado y devuelve los resultados como una colección de objetos.</summary>
    /// <typeparam name="T">Tipo de los objetos en la colección de retorno.</typeparam>
    /// <param name="storedProcedure">Nombre del procedimiento almacenado.</param>
    /// <param name="expression">Función para mapear cada fila a un objeto.</param>
    /// <param name="parameter">Acción para configurar los parámetros del procedimiento.</param>
    /// <returns>Colección de objetos mapeados.</returns>
    ICollection<T> ExecuteProcedureAsList<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null);

    /// <summary>Ejecuta una consulta SQL y devuelve los resultados como una colección de objetos.</summary>
    /// <typeparam name="T">Tipo de los objetos en la colección de retorno.</typeparam>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="expression">Función para mapear cada fila a un objeto.</param>
    /// <param name="parameter">Acción para configurar los parámetros de la consulta.</param>
    /// <returns>Colección de objetos mapeados.</returns>
    ICollection<T> ExecuteQueryAsList<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null);


    /// <summary>Ejecuta una consulta SQL y devuelve los resultados como una colección de diccionarios.</summary>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="parameter">Acción para configurar los parámetros de la consulta.</param>
    /// <returns>Colección de diccionarios donde cada diccionario representa una fila.</returns>
    ICollection<Dictionary<string, object>> ExecuteQueryAsDictionary(string query, Action<IDataParameterCollection> parameter = null);

    /// <summary>Ejecuta un procedimiento almacenado y devuelve los resultados como una colección de diccionarios.</summary>
    /// <param name="storedProcedure">Nombre del procedimiento almacenado.</param>
    /// <param name="parameter">Acción para configurar los parámetros del procedimiento.</param>
    /// <returns>Colección de diccionarios donde cada diccionario representa una fila.</returns>
    ICollection<Dictionary<string, object>> ExecuteProcedureAsDictionary(string storedProcedure, Action<IDataParameterCollection> parameter = null);


    /// <summary>
    /// Ejecuta una consulta y devuelve una lista de entidades.
    /// </summary>
    Task<ICollection<T>> ExecuteQueryAsListAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un procedimiento almacenado y mapea cada registro a la entidad indicada.
    /// </summary>
    Task<ICollection<T>> ExecuteProcedureAsListAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una consulta y devuelve los resultados como colección de diccionarios.
    /// </summary>
    Task<ICollection<Dictionary<string, object>>> ExecuteQueryAsDictionaryAsync(string query, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve los resultados como colección de diccionarios.
    /// </summary>
    Task<ICollection<Dictionary<string, object>>> ExecuteProcedureAsDictionaryAsync(string storedProcedure, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);

}