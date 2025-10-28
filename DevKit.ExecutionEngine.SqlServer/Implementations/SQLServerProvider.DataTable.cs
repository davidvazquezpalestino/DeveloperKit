namespace DevKit.ExecutionEngine.SQLServer.Implementations;

public partial class SQLServerProvider
{
    /// <summary>Ejecuta una consulta y devuelve el resultado en un <see cref="DataTable"/>.</summary>
    public DataTable ExecuteQueryAsTable(string query, Action<IDataParameterCollection> dbParameters = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            dbParameters?.Invoke(command.Parameters);
            command.CommandTimeout = SqlOptions.CommandTimeout;

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            using (DbDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                DataTable table = new();
                table.Load(reader);
                return table;
            }
        }
    }

    /// <summary>Ejecuta un procedimiento almacenado y devuelve el resultado en un <see cref="DataTable"/>.</summary>
    public DataTable ExecuteProcedureAsTable(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)
    {
        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure;
            command.CommandTimeout = SqlOptions.CommandTimeout;
            dbParameters?.Invoke(command.Parameters);

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            DataTable tabla = new();
            IDataReader reader = command.ExecuteReader();
            tabla.Load(reader);
            return tabla;
        }
    }

    /// <summary>Ejecuta una consulta SQL y devuelve un DataTable de forma asíncrona.</summary>
    public async Task<DataTable> ExecuteQueryAsTableAsync(string query,
        Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("La consulta no puede estar vacía.", nameof(query));
        }

        bool isConnectionOwner = false;
        SqlConnection connection = Connection;

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                isConnectionOwner = true;
            }

            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = SqlOptions.CommandTimeout; // Valor por defecto de 30 segundos

                if (Transaccion != null)
                {
                    command.Transaction = Transaccion;
                }

                dbParameters?.Invoke(command.Parameters);

                using (IDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken)
                           .ConfigureAwait(false))
                {
                    DataTable table = new();
                    table.Load(reader);
                    return table;
                }
            }
        }
        finally
        {
            if (isConnectionOwner && connection?.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }

    /// <summary>Ejecuta un procedimiento almacenado y devuelve un DataTable de forma asíncrona.</summary>
    public async Task<DataTable> ExecuteProcedureAsTableAsync(string storedProcedure,
        Action<IDataParameterCollection> dbParameters = null, CancellationToken cancellationToken = default)
    {
        using (DbConnection connection = new SqlConnection(ConnectionString))
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = storedProcedure;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlOptions.CommandTimeout;
                dbParameters?.Invoke(command.Parameters);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    DataTable tabla = new();
                    tabla.Load(reader);
                    return tabla;
                }
            }
        }
    }
}