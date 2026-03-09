namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <summary>Ejecuta un comando que no devuelve resultados.</summary>
    public void ExecuteCommand(string command, Action<IDataParameterCollection> dbParameters = null)
    {
        if (Connection.State == ConnectionState.Closed)
        {
            Connection.Open();
        }

        using (DbCommand sqlCommand = Connection.CreateCommand())
        {
            sqlCommand.CommandTimeout = SqlOptions.CommandTimeout;
            sqlCommand.Transaction = Transaction;
            sqlCommand.CommandText = command;
            sqlCommand.CommandType = CommandType.Text;
            dbParameters?.Invoke(sqlCommand.Parameters);

            sqlCommand.ExecuteNonQuery();
        }
    }


    /// <summary>Ejecuta un procedimiento almacenado sin esperar resultados.</summary>
    public void ExecuteProcedureCommand(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.Connection = Connection;
            command.CommandTimeout = SqlOptions.CommandTimeout;
            command.Transaction = Transaction;
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