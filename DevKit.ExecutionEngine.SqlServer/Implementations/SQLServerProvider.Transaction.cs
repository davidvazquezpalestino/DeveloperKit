namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <summary>Inicia una transacción y abre la conexión si es necesario.</summary>
    public void BeginTransaction()
    {
        Connection.Open();
        Transaction = Connection.BeginTransaction();
    }
    /// <summary>Confirma la transacción y cierra la conexión.</summary>
    /// <exception cref="InvalidOperationException">Se lanza cuando no hay una transacción activa.</exception>
    public void CommitTransaction()
    {
        if (Transaction == null)
        {
            throw new InvalidOperationException("No hay una transacción activa para confirmar.");
        }

        using (Transaction)
        {
            Transaction.Commit();
        }

        Transaction = null;

        if (Connection.State != ConnectionState.Closed)
        {
            Connection.Close();
        }
    }
    /// <summary>Revierte la transacción y cierra la conexión.</summary>
    /// <exception cref="InvalidOperationException">Se lanza cuando no hay una transacción activa.</exception>
    public void RollbackTransaction()
    {
        if (Transaction == null)
        {
            throw new InvalidOperationException("No hay una transacción activa para revertir.");
        }

        try
        {
            Transaction.Rollback();
        }
        finally
        {
            Transaction?.Dispose();
            Transaction = null;
            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }
    }
}