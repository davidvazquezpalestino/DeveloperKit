namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

public partial interface ISQLServerProvider
{
    /// <summary>
    /// Ejecuta un comando que no devuelve resultados.
    /// </summary>
    void ExecuteNonQuery(string command, Action<IDataParameterCollection> parametros = null);
}