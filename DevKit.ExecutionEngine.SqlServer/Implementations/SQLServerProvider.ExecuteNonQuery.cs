namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <summary>Ejecuta un comando que no devuelve resultados.</summary>
    public void ExecuteNonQuery(string command, Action<IDataParameterCollection> dbParameters = null)
    {
        bool isConnectionOwner = false;
        try
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
                isConnectionOwner = true;
            }

            using (DbCommand sqlCommand = Connection.CreateCommand())
            {
                sqlCommand.CommandTimeout = SqlOptions.CommandTimeout;
                sqlCommand.Transaction = Transaccion;
                sqlCommand.CommandText = command;
                sqlCommand.CommandType = CommandType.Text;
                dbParameters?.Invoke(sqlCommand.Parameters);

                sqlCommand.ExecuteNonQuery();
            }
        }
        finally
        {
            if (isConnectionOwner && Connection?.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }
    }
    /// <summary>Ejecuta un procedimiento almacenado sin esperar resultados.</summary>
    public void ExecuteProcedureCommand(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.Connection = Connection;
            command.CommandTimeout = SqlOptions.CommandTimeout;
            command.Transaction = Transaccion;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            dbParameters?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.ExecuteNonQuery();
        }
    }
}