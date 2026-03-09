namespace DevKit.ExecutionEngine.SQLServer.Telemetry;

/// <summary>
/// Nombres de eventos de diagnóstico para SQL Server Provider.
/// </summary>
public static class SqlServerDiagnosticNames
{
    /// <summary>Evento de inicio de consulta SQL.</summary>
    public const string QueryStart = "SqlServer.Query.Start";
    
    /// <summary>Evento de fin de consulta SQL.</summary>
    public const string QueryStop = "SqlServer.Query.Stop";
    
    /// <summary>Evento de error en consulta SQL.</summary>
    public const string QueryError = "SqlServer.Query.Error";
    
    /// <summary>Evento de inicio de transacción.</summary>
    public const string TransactionStart = "SqlServer.Transaction.Start";
    
    /// <summary>Evento de confirmación de transacción.</summary>
    public const string TransactionCommit = "SqlServer.Transaction.Commit";
    
    /// <summary>Evento de reversión de transacción.</summary>
    public const string TransactionRollback = "SqlServer.Transaction.Rollback";
    
    /// <summary>Evento de inicio de operación bulk.</summary>
    public const string BulkStart = "SqlServer.Bulk.Start";
    
    /// <summary>Evento de fin de operación bulk.</summary>
    public const string BulkStop = "SqlServer.Bulk.Stop";
    
    /// <summary>Evento de error en operación bulk.</summary>
    public const string BulkError = "SqlServer.Bulk.Error";
}

/// <summary>
/// Claves para propiedades de diagnóstico.
/// </summary>
public static class SqlServerDiagnosticKeys
{
    /// <summary>Tipo de comando (Text, StoredProcedure).</summary>
    public const string CommandType = "commandType";
    
    /// <summary>Texto del comando o nombre del procedimiento.</summary>
    public const string CommandText = "commandText";
    
    /// <summary>Duración de la operación en milisegundos.</summary>
    public const string Duration = "duration";
    
    /// <summary>Número de filas afectadas.</summary>
    public const string RowsAffected = "rowsAffected";
    
    /// <summary>Excepción ocurrida.</summary>
    public const string Exception = "exception";
    
    /// <summary>Nombre del servidor.</summary>
    public const string ServerName = "serverName";
    
    /// <summary>Nombre de la base de datos.</summary>
    public const string DatabaseName = "databaseName";
    
    /// <summary>Tamaño del lote en operaciones bulk.</summary>
    public const string BatchSize = "batchSize";
    
    /// <summary>Número total de filas en operaciones bulk.</summary>
    public const string TotalRows = "totalRows";
}
