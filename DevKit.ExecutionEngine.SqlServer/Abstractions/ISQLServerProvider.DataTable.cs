namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

public partial interface ISQLServerProvider
{
    /// <summary>Ejecuta una consulta SQL y devuelve los resultados en un DataTable.</summary>
    /// <param name="query">Consulta SQL a ejecutar.</param>
    /// <param name="parameter">Acción para configurar los parámetros de la consulta.</param>
    /// <returns>DataTable con los resultados de la consulta.</returns>
    DataTable ExecuteQueryAsTable(string query, Action<IDataParameterCollection> parameter = null);

    /// <summary>Ejecuta un procedimiento almacenado y devuelve los resultados en un DataTable.</summary>
    /// <param name="storedProcedure">Nombre del procedimiento almacenado a ejecutar.</param>
    /// <param name="parameter">Acción para configurar los parámetros del procedimiento.</param>
    /// <returns>DataTable con los resultados del procedimiento.</returns>
    DataTable ExecuteProcedureAsTable(string storedProcedure, Action<IDataParameterCollection> parameter = null);

    /// <summary>
    /// Ejecuta una consulta de forma asíncrona y devuelve un DataTable.
    /// </summary>
    Task<DataTable> ExecuteQueryAsTableAsync(string query, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un procedimiento almacenado de forma asíncrona y devuelve un DataTable.
    /// </summary>
    Task<DataTable> ExecuteProcedureAsTableAsync(string storedProcedure, Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default);

}