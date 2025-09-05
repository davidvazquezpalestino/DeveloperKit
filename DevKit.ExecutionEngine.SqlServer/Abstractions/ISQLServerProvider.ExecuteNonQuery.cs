namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

public partial interface ISQLServerProvider
{
    /// <summary>
    /// Ejecuta un comando que no devuelve resultados.
    /// </summary>
    void ExecuteNonQuery(string command, Action<IDataParameterCollection> dbParameters = null);

    /// <summary>
    /// Ejecuta un procedimiento almacenado sin esperar resultados.
    /// </summary>
    void ExecuteProcedureCommand(string storedProcedure, Action<IDataParameterCollection> dbParameters = null);

}