namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <summary>Ejecuta un comando que no devuelve resultados.</summary>
    public void ExecuteNonQuery(string command, Action<IDataParameterCollection> parametros = null)
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
                parametros?.Invoke(sqlCommand.Parameters);

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
}