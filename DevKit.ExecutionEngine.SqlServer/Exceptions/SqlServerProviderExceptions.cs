namespace DevKit.ExecutionEngine.SQLServer.Exceptions;

/// <summary>
/// Excepción base para todas las excepciones de SQL Server Provider.
/// </summary>
public abstract class SqlServerProviderException(string message,
                                                 string serverName = null,
                                                 string databaseName = null,
                                                 string commandText = null,
                                                 CommandType? commandType = null,
                                                 long executionTimeMs = 0,
                                                 IDictionary<string, object> parameters = null,
                                                 Exception innerException = null) : Exception(message, innerException)
{
    /// <summary>
    /// Nombre del servidor de base de datos.
    /// </summary>
    public string ServerName { get; } = serverName;

    /// <summary>
    /// Nombre de la base de datos.
    /// </summary>
    public string DatabaseName { get; } = databaseName;

    /// <summary>
    /// Comando SQL que causó la excepción.
    /// </summary>
    public string CommandText { get; } = commandText;

    /// <summary>
    /// Tipo de comando (Text, StoredProcedure).
    /// </summary>
    public CommandType CommandType { get; } = commandType ?? CommandType.Text;

    /// <summary>
    /// Tiempo de ejecución del comando en milisegundos.
    /// </summary>
    public long ExecutionTimeMs { get; } = executionTimeMs;

    /// <summary>
    /// Parámetros del comando.
    /// </summary>
    public IReadOnlyDictionary<string, object> Parameters { get; } = parameters?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [];

    /// <summary>
    /// Timestamp de cuando ocurrió la excepción.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// ID de correlación para seguimiento.
    /// </summary>
    public Guid CorrelationId { get; } = Guid.NewGuid();

    /// <summary>
    /// Convierte la excepción a un diccionario para telemetría.
    /// </summary>
    /// <returns>Diccionario con propiedades de telemetría.</returns>
    public virtual Dictionary<string, object> ToTelemetryProperties()
    {
        return new Dictionary<string, object>
        {
            ["ExceptionType"] = GetType().Name,
            ["Message"] = Message,
            ["ServerName"] = ServerName ?? "Unknown",
            ["DatabaseName"] = DatabaseName ?? "Unknown",
            ["CommandType"] = CommandType.ToString(),
            ["ExecutionTimeMs"] = ExecutionTimeMs,
            ["ParameterCount"] = Parameters.Count,
            ["Timestamp"] = Timestamp,
            ["CorrelationId"] = CorrelationId,
            ["InnerExceptionType"] = InnerException?.GetType().Name,
            ["InnerExceptionMessage"] = InnerException?.Message
        };
    }
}

/// <summary>
/// Excepción lanzada cuando falla una consulta SQL.
/// </summary>
public class SqlQueryException(string message,
                               string serverName = null,
                               string databaseName = null,
                               string commandText = null,
                               CommandType? commandType = null,
                               long executionTimeMs = 0,
                               IDictionary<string, object> parameters = null,
                               Exception innerException = null) : SqlServerProviderException(message, serverName, databaseName, commandText, commandType, executionTimeMs, parameters, innerException)
{
}

/// <summary>
/// Excepción lanzada cuando falla una conexión a la base de datos.
/// </summary>
public class SqlConnectionException(string message,
                                    string safeConnectionString = null,
                                    int connectionTimeout = 30,
                                    string serverName = null,
                                    string databaseName = null,
                                    Exception innerException = null) : SqlServerProviderException(message, serverName, databaseName, null, null, 0, null, innerException)
{
    /// <summary>
    /// Cadena de conexión (sin información sensible).
    /// </summary>
    public string SafeConnectionString { get; } = safeConnectionString;

    /// <summary>
    /// Tiempo de espera de conexión en segundos.
    /// </summary>
    public int ConnectionTimeout { get; } = connectionTimeout;

    /// <inheritdoc/>
    public override Dictionary<string, object> ToTelemetryProperties()
    {
        Dictionary<string, object> properties = base.ToTelemetryProperties();
        properties["ConnectionTimeout"] = ConnectionTimeout;
        return properties;
    }
}

/// <summary>
/// Excepción lanzada cuando falla una transacción.
/// </summary>
public class SqlTransactionException(string message,
                                   IsolationLevel isolationLevel = IsolationLevel.Unspecified,
                                   string transactionState = null,
                                   string serverName = null,
                                   string databaseName = null,
                                   Exception innerException = null) : SqlServerProviderException(message, serverName, databaseName, null, null, 0, null, innerException)
{
    /// <summary>
    /// Nivel de aislamiento de la transacción.
    /// </summary>
    public IsolationLevel IsolationLevel { get; } = isolationLevel;

    /// <summary>
    /// Estado de la transacción cuando ocurrió el error.
    /// </summary>
    public string TransactionState { get; } = transactionState;

    /// <inheritdoc/>
    public override Dictionary<string, object> ToTelemetryProperties()
    {
        Dictionary<string, object> properties = base.ToTelemetryProperties();
        properties["IsolationLevel"] = IsolationLevel.ToString();
        properties["TransactionState"] = TransactionState ?? "Unknown";
        return properties;
    }
}

/// <summary>
/// Excepción lanzada cuando falla una operación bulk.
/// </summary>
public class SqlBulkOperationException(string message,
                                     string tableName = null,
                                     int rowsProcessed = 0,
                                     int batchSize = 0,
                                     string serverName = null,
                                     string databaseName = null,
                                     Exception innerException = null) : SqlServerProviderException(message, serverName, databaseName, null, CommandType.Text, 0, null, innerException)
{
    /// <summary>
    /// Nombre de la tabla destino.
    /// </summary>
    public string TableName { get; } = tableName;

    /// <summary>
    /// Número de filas procesadas antes del error.
    /// </summary>
    public int RowsProcessed { get; } = rowsProcessed;

    /// <summary>
    /// Tamaño del lote.
    /// </summary>
    public int BatchSize { get; } = batchSize;

    /// <inheritdoc/>
    public override Dictionary<string, object> ToTelemetryProperties()
    {
        Dictionary<string, object> properties = base.ToTelemetryProperties();
        properties["TableName"] = TableName ?? "Unknown";
        properties["RowsProcessed"] = RowsProcessed;
        properties["BatchSize"] = BatchSize;
        return properties;
    }
}

/// <summary>
/// Excepción lanzada cuando ocurre un timeout en una operación.
/// </summary>
public class SqlTimeoutException(string message,
                                   int timeoutSeconds,
                                   string serverName = null,
                                   string databaseName = null,
                                   string commandText = null,
                                   CommandType? commandType = null,
                                   IDictionary<string, object> parameters = null) : SqlServerProviderException(message, serverName, databaseName, commandText, commandType, timeoutSeconds * 1000, parameters, null)
{
    /// <summary>
    /// Tiempo de espera configurado en segundos.
    /// </summary>
    public int TimeoutSeconds { get; } = timeoutSeconds;

    /// <inheritdoc/>
    public override Dictionary<string, object> ToTelemetryProperties()
    {
        Dictionary<string, object> properties = base.ToTelemetryProperties();
        properties["TimeoutSeconds"] = TimeoutSeconds;
        return properties;
    }
}
