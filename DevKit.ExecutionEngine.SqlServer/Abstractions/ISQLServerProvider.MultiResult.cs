namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

public partial interface ISQLServerProvider
{
    /// <summary>
    /// Ejecuta una consulta y devuelve los resultados como lista de listas de diccionarios.
    /// </summary>
    public Task<DataSet> ExecuteQueryMultiResultAsync(
        string query,
        Action<IDataParameterCollection> dbParameters = null,
        Action<string> logger = null,
        CancellationToken cancellationToken = default);
}