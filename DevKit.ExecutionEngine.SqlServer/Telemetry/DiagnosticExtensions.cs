namespace DevKit.ExecutionEngine.SQLServer.Telemetry;

/// <summary>
/// Proporciona métodos de extensión para telemetría de operaciones de base de datos.
/// </summary>
public static class DiagnosticExtensions
{
    private static readonly DiagnosticSource DiagnosticSource = new DiagnosticListener("DevKit.SqlServer");
    
    /// <summary>
    /// Escribe un evento de inicio de consulta.
    /// </summary>
    /// <param name="commandText">Texto del comando.</param>
    /// <param name="commandType">Tipo de comando.</param>
    /// <param name="serverName">Nombre del servidor.</param>
    /// <param name="databaseName">Nombre de la base de datos.</param>
    /// <returns>ID de actividad para correlación.</returns>
    public static Guid WriteQueryStart(string commandText, CommandType commandType, string serverName = null, string databaseName = null)
    {
        if (!DiagnosticSource.IsEnabled(SqlServerDiagnosticNames.QueryStart))
            return Guid.Empty;
            
        var activityId = Guid.NewGuid();
        var properties = new Dictionary<string, object>
        {
            [SqlServerDiagnosticKeys.CommandText] = commandText,
            [SqlServerDiagnosticKeys.CommandType] = commandType.ToString()
        };
        
        if (!string.IsNullOrEmpty(serverName))
            properties[SqlServerDiagnosticKeys.ServerName] = serverName;
            
        if (!string.IsNullOrEmpty(databaseName))
            properties[SqlServerDiagnosticKeys.DatabaseName] = databaseName;
            
        DiagnosticSource.Write(SqlServerDiagnosticNames.QueryStart, properties);
        return activityId;
    }
    
    /// <summary>
    /// Escribe un evento de fin de consulta exitosa.
    /// </summary>
    /// <param name="activityId">ID de actividad.</param>
    /// <param name="duration">Duración en milisegundos.</param>
    /// <param name="rowsAffected">Filas afectadas.</param>
    public static void WriteQueryStop(Guid activityId, long duration, int rowsAffected = -1)
    {
        if (!DiagnosticSource.IsEnabled(SqlServerDiagnosticNames.QueryStop) || activityId == Guid.Empty)
            return;
            
        var properties = new Dictionary<string, object>
        {
            [SqlServerDiagnosticKeys.Duration] = duration
        };
        
        if (rowsAffected >= 0)
            properties[SqlServerDiagnosticKeys.RowsAffected] = rowsAffected;
            
        DiagnosticSource.Write(SqlServerDiagnosticNames.QueryStop, properties);
    }
    
    /// <summary>
    /// Escribe un evento de error en consulta.
    /// </summary>
    /// <param name="activityId">ID de actividad.</param>
    /// <param name="exception">Excepción ocurrida.</param>
    /// <param name="duration">Duración en milisegundos.</param>
    public static void WriteQueryError(Guid activityId, Exception exception, long duration)
    {
        if (!DiagnosticSource.IsEnabled(SqlServerDiagnosticNames.QueryError) || activityId == Guid.Empty)
            return;
            
        var properties = new Dictionary<string, object>
        {
            [SqlServerDiagnosticKeys.Exception] = exception,
            [SqlServerDiagnosticKeys.Duration] = duration
        };
        
        DiagnosticSource.Write(SqlServerDiagnosticNames.QueryError, properties);
    }
    
    /// <summary>
    /// Escribe un evento de inicio de operación bulk.
    /// </summary>
    /// <param name="tableName">Nombre de la tabla.</param>
    /// <param name="batchSize">Tamaño del lote.</param>
    /// <param name="totalRows">Número total de filas.</param>
    /// <returns>ID de actividad para correlación.</returns>
    public static Guid WriteBulkStart(string tableName, int batchSize, int totalRows)
    {
        if (!DiagnosticSource.IsEnabled(SqlServerDiagnosticNames.BulkStart))
            return Guid.Empty;
            
        var activityId = Guid.NewGuid();
        var properties = new Dictionary<string, object>
        {
            [SqlServerDiagnosticKeys.CommandText] = tableName,
            [SqlServerDiagnosticKeys.BatchSize] = batchSize,
            [SqlServerDiagnosticKeys.TotalRows] = totalRows
        };
        
        DiagnosticSource.Write(SqlServerDiagnosticNames.BulkStart, properties);
        return activityId;
    }
    
    /// <summary>
    /// Escribe un evento de fin de operación bulk exitosa.
    /// </summary>
    /// <param name="activityId">ID de actividad.</param>
    /// <param name="duration">Duración en milisegundos.</param>
    /// <param name="rowsAffected">Filas afectadas.</param>
    public static void WriteBulkStop(Guid activityId, long duration, int rowsAffected)
    {
        if (!DiagnosticSource.IsEnabled(SqlServerDiagnosticNames.BulkStop) || activityId == Guid.Empty)
            return;
            
        var properties = new Dictionary<string, object>
        {
            [SqlServerDiagnosticKeys.Duration] = duration,
            [SqlServerDiagnosticKeys.RowsAffected] = rowsAffected
        };
        
        DiagnosticSource.Write(SqlServerDiagnosticNames.BulkStop, properties);
    }
    
    /// <summary>
    /// Escribe un evento de error en operación bulk.
    /// </summary>
    /// <param name="activityId">ID de actividad.</param>
    /// <param name="exception">Excepción ocurrida.</param>
    /// <param name="duration">Duración en milisegundos.</param>
    public static void WriteBulkError(Guid activityId, Exception exception, long duration)
    {
        if (!DiagnosticSource.IsEnabled(SqlServerDiagnosticNames.BulkError) || activityId == Guid.Empty)
            return;
            
        var properties = new Dictionary<string, object>
        {
            [SqlServerDiagnosticKeys.Exception] = exception,
            [SqlServerDiagnosticKeys.Duration] = duration
        };
        
        DiagnosticSource.Write(SqlServerDiagnosticNames.BulkError, properties);
    }
}
