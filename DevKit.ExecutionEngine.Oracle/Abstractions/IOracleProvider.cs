namespace DevKit.ExecutionEngine.Oracle.Abstractions;

/// <summary>
/// Interfaz principal para el repositorio de Oracle que define operaciones básicas de base de datos.
/// Proporciona métodos para ejecutar consultas, procedimientos almacenados y operaciones de transacción.
/// </summary>
public partial interface IOracleProvider : IAsyncDisposable
{
    /// <summary>
    /// Estado actual de la conexión.
    /// </summary>
    public ConnectionState ConnectionState { get; }

    /// <summary>
    /// Cadena de conexión utilizada por el repositorio.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Inicia una transacción.
    /// </summary>
    void BeginTransaction();

    /// <summary>
    /// Confirma la transacción en curso.
    /// </summary>
    void CommitTransaction();

    /// <summary>
    /// Revierte la transacción en curso.
    /// </summary>
    void RollbackTransaction();

    /// <summary>Ejecuta una consulta SQL y devuelve los resultados en un DataTable.</summary>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="parameter">Acción para configurar los parámetros de la consulta.</param>
    /// <returns>DataTable con los resultados de la consulta.</returns>
    DataTable ExecuteQueryAsTable(string query, Action<IDataParameterCollection> parameter = null);

    /// <summary>Ejecuta un procedimiento almacenado y devuelve los resultados en un DataTable.</summary>
    /// <param name="procedimientoAlmacenado">Nombre del procedimiento almacenado a ejecutar.</param>
    /// <param name="parameter">Acción para configurar los parámetros del procedimiento.</param>
    /// <returns>DataTable con los resultados del procedimiento.</returns>
    DataTable ExecuteProcedureAsTable(string procedimientoAlmacenado, Action<IDataParameterCollection> parameter = null);

    /// <summary>
    /// Ejecuta una consulta y mapea el primer registro a la entidad indicada.
    /// </summary>
    T ExecuteQueryAsSingle<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null);

    /// <summary>
    /// Ejecuta un procedimiento almacenado y mapea el primer registro a la entidad indicada.
    /// </summary>
    T ExecuteProcedureAsSingle<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null);

    /// <summary>Ejecuta una consulta SQL y devuelve los resultados como una colección de diccionarios.</summary>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="parameter">Acción para configurar los parámetros de la consulta.</param>
    /// <returns>Colección de diccionarios donde cada diccionario representa una fila.</returns>
    ICollection<Dictionary<string, object>> ExecuteQueryAsDictionary(string query, Action<IDataParameterCollection> parameter = null);

    /// <summary>Ejecuta un procedimiento almacenado y devuelve los resultados como una colección de diccionarios.</summary>
    /// <param name="procedimientoAlmacenado">Nombre del procedimiento almacenado.</param>
    /// <param name="parameter">Acción para configurar los parámetros del procedimiento.</param>
    /// <returns>Colección de diccionarios donde cada diccionario representa una fila.</returns>
    ICollection<Dictionary<string, object>> ExecuteProcedureAsDictionary(string procedimientoAlmacenado, Action<IDataParameterCollection> parameter = null);

    /// <summary>Ejecuta un procedimiento almacenado y devuelve los resultados como una colección de objetos.</summary>
    /// <typeparam name="T">Tipo de los objetos en la colección de retorno.</typeparam>
    /// <param name="procedimientoAlmacenado">Nombre del procedimiento almacenado.</param>
    /// <param name="expression">Función para mapear cada fila a un objeto.</param>
    /// <param name="parameter">Acción para configurar los parámetros del procedimiento.</param>
    /// <returns>Colección de objetos mapeados.</returns>
    ICollection<T> ExecuteProcedureAsList<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null);

    /// <summary>Ejecuta una consulta SQL y devuelve los resultados como una colección de objetos.</summary>
    /// <typeparam name="T">Tipo de los objetos en la colección de retorno.</typeparam>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="expression">Función para mapear cada fila a un objeto.</param>
    /// <param name="parameter">Acción para configurar los parámetros de la consulta.</param>
    /// <returns>Colección de objetos mapeados.</returns>
    ICollection<T> ExecuteQueryAsList<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parameter = null);

    /// <summary>
    /// Ejecuta un comando que no devuelve resultados.
    /// </summary>
    void ExecuteNonQuery(string command, Action<IDataParameterCollection> parametros = null);

    /// <summary>
    /// Ejecuta un procedimiento almacenado sin esperar resultados.
    /// </summary>
    void ExecuteProcedureCommand(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null);

    /// <summary>
    /// Copia masivamente datos de un DataTable a la tabla destino.
    /// </summary>
    void ExecuteBulkInsertToTable(DataTable source, string target);
    /// <summary>
    /// Copia masivamente datos de un DataTable a la tabla destino.
    /// </summary>
    void ExecuteBulkInsert(DataTable source, string target);

}