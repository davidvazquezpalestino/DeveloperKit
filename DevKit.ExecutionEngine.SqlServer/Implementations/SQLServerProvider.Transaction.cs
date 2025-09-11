namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <summary>Inicia una transacción y abre la conexión si es necesario.</summary>
    public void BeginTransaction()
    {
        Connection.Open();
        Transaccion = Connection.BeginTransaction();
    }
    /// <summary>Confirma la transacción y cierra la conexión.</summary>
    /// <exception cref="InvalidOperationException">Se lanza cuando no hay una transacción activa.</exception>
    public void CommitTransaction()
    {
        if (Transaccion == null)
        {
            throw new InvalidOperationException("No hay una transacción activa para confirmar.");
        }

        using (Transaccion)
        {
            Transaccion.Commit();
        }

        Transaccion = null;

        if (Connection.State != ConnectionState.Closed)
        {
            Connection.Close();
        }
    }
    /// <summary>Revierte la transacción y cierra la conexión.</summary>
    /// <exception cref="InvalidOperationException">Se lanza cuando no hay una transacción activa.</exception>
    public void RollbackTransaction()
    {
        if (Transaccion == null)
        {
            throw new InvalidOperationException("No hay una transacción activa para revertir.");
        }

        try
        {
            Transaccion.Rollback();
        }
        finally
        {
            Transaccion?.Dispose();
            Transaccion = null;
            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }
    }
}