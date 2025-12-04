namespace DevKit.ExecutionEngine.SQLServer.Query;

/// <summary>
/// Representa el resultado de la construcción de una consulta SQL
/// </summary>
public class QueryResult
{
    /// <summary>
    /// Obtiene o establece la consulta SQL generada
    /// </summary>
    public string SQL { get; set; }

    /// <summary>
    /// Obtiene o establece los parámetros de la consulta
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="QueryResult"/>
    /// </summary>
    public QueryResult()
    {
        Parameters = new Dictionary<string, object>();
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="QueryResult"/>
    /// </summary>
    /// <param name="sql">Consulta SQL generada</param>
    /// <param name="parameters">Parámetros de la consulta</param>
    public QueryResult(string sql, Dictionary<string, object> parameters = null)
    {
        SQL = sql;
        Parameters = parameters ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Permite la desconstrucción del objeto en una tupla
    /// </summary>
    public void Deconstruct(out string sql, out Dictionary<string, object> parameters)
    {
        sql = SQL;
        parameters = Parameters;
    }
}