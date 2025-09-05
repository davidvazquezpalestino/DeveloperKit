namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

public partial interface ISQLServerProvider
{
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
}