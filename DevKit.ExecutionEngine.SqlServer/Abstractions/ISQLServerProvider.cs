namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

/// <summary>
/// Interfaz principal para el repositorio de Oracle que define operaciones básicas de base de datos.
/// Proporciona métodos para ejecutar consultas, procedimientos almacenados y operaciones de transacción.
/// </summary>
public partial interface ISQLServerProvider
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
    /// Ejecuta una consulta y mapea el primer registro a la entidad indicada.
    /// </summary>
    T ExecuteQueryAsSingle<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null);

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver</typeparam>
    /// <param name="query">Consulta SQL a ejecutar</param>
    /// <param name="parametros">Parámetros de la consulta</param>
    /// <returns>Primer elemento que cumple con la condición</returns>
    T First<T>(string query, Action<IDataParameterCollection> parametros = null) where T : class, new();

    /// <summary>
    /// Ejecuta una consulta y devuelve el primer elemento del tipo especificado o un valor predeterminado si no se encuentra ningún elemento.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a devolver</typeparam>
    /// <param name="query">Consulta SQL a ejecutar</param>
    /// <param name="parametros">Parámetros de la consulta</param>
    /// <returns>Primer elemento que cumple con la condición o valor predeterminado</returns>
    T FirstOrDefault<T>(string query, Action<IDataParameterCollection> parametros = null) where T : class, new();

    /// <summary>
    /// Ejecuta un procedimiento almacenado y mapea el primer registro a la entidad indicada.
    /// </summary>
    T ExecuteProcedureAsSingle<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null);

    /// <summary>
    /// Ejecuta una consulta SQL y devuelve un valor escalar.
    /// </summary>
    T ExecuteScalar<T>(string query, Action<IDataParameterCollection> parameter = null);

    /// <summary>
    /// Obtiene la fecha y hora actuales del servidor.
    /// </summary>
    DateTime GetCurrentDateTime();
}