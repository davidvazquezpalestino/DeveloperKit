
namespace DevKit.ExecutionEngine.SQLServer.Implementations;

/// <summary>Implementación de métodos de inserción para SQL Server.</summary>
public partial class SQLServerProvider
{
    /// <summary>Inserta una entidad en la tabla especificada.</summary>
    public void ExecuteInsert<T>(string tableName, T entity) where T : class, new()
    {
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(propertyInfo => propertyInfo.CanRead && !propertyInfo.GetGetMethod().GetParameters().Any())
            .ToArray();

        string columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
        string parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";

        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = SqlOptions.CommandTimeout;

            foreach (PropertyInfo prop in properties)
            {
                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = $"@{prop.Name}";
                parameter.Value = prop.GetValue(entity) ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.ExecuteNonQuery();
        }
    }

    /// <summary>Inserta una colección de entidades en la tabla especificada.</summary>
    public void ExecuteInsert<T>(string tableName, ICollection<T> collection) where T : class, new()
    {
        if (!collection.Any())
        {
            return;
        }

        using (SqlTransaction transaction = Connection.BeginTransaction())
        {
            try
            {
                foreach (T entity in collection)
                {
                    ExecuteInsert(tableName, entity);
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    /// <summary>Inserta una colección de entidades en la tabla especificada con configuración de lote.</summary>
    public async Task ExecuteInsertAsync<T>(
        string tableName,
        ICollection<T> entities,
        int batchSize = 1000,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(tableName));
        }

        if (entities == null || !entities.Any())
        {
            return;
        }

        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !p.GetGetMethod().GetParameters().Any())
            .ToArray();

        string columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
        string parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            await connection.OpenAsync(cancellationToken);

            foreach (IEnumerable<T> batch in entities.Chunk(batchSize))
            {
                // Use synchronous BeginTransaction for .NET Framework compatibility
                using (SqlTransaction transaction = connection.BeginTransaction())
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = (SqlTransaction)transaction;
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = SqlOptions.CommandTimeout;

                    try
                    {
                        foreach (T entity in batch)
                        {
                            command.Parameters.Clear();
                            foreach (PropertyInfo prop in properties)
                            {
                                SqlParameter parameter = command.CreateParameter();
                                parameter.ParameterName = $"@{prop.Name}";
                                parameter.Value = prop.GetValue(entity) ?? DBNull.Value;
                                command.Parameters.Add(parameter);
                            }
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }

    /// <summary>Inserta una entidad en la tabla especificada.</summary>
    public async Task ExecuteInsertAsync<T>(
        string tableName,
        T entity,
        CancellationToken cancellationToken = default)
    {
        PropertyInfo[] properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !p.GetGetMethod().GetParameters().Any())
            .ToArray();

        string columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
        string parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.CommandType = CommandType.Text;
            command.CommandTimeout = SqlOptions.CommandTimeout;

            foreach (PropertyInfo prop in properties)
            {
                command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity) ?? DBNull.Value);
            }

            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
