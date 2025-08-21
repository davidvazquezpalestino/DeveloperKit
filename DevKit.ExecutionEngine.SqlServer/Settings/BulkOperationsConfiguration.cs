namespace DevKit.ExecutionEngine.SQLServer.Settings;

///<summary> Configuración avanzada para operaciones Bulk con soporte para mapeo de columnas y callbacks. </summary>
public class BulkOperationsConfiguration
{
    ///<summary> Nombre de la tabla destino. </summary>
    public string DestinationTableName { get; set; } = string.Empty;

    ///<summary> Tamaño del lote para operaciones bulk. </summary>
    public int BatchSize { get; set; } = 1000;

    ///<summary> Tiempo de espera para operaciones bulk en segundos. </summary>
    public int BulkCopyTimeout { get; set; } = 300;

    ///<summary> Opciones de SqlBulkCopy. </summary>


    ///<summary> Número de filas después del cual se dispara el evento de notificación. </summary>
    public int NotifyAfter { get; set; }

    ///<summary> Mapeos de columnas personalizados. </summary>
    public List<ColumnMapping> ColumnMappings { get; set; } = new List<ColumnMapping>();

    ///<summary> Callback para notificaciones de progreso. </summary>

    ///<summary> Habilitar logging detallado. </summary>
    public bool EnableDetailedLogging { get; set; }
}

///<summary> Mapeo de columna para operaciones bulk. </summary>
public class ColumnMapping
{
    /// <summary> Nombre de la columna origen. </summary>
    public string SourceColumn { get; set; }

    /// <summary> Nombre de la columna destino. </summary>
    public string DestinationColumn { get; set; }

    /// <summary> Constructor para mapeo de columnas. </summary>
    public ColumnMapping() { }

    /// <summary> Constructor para mapeo de columnas con nombres. </summary>
    public ColumnMapping(string sourceColumn, string destinationColumn)
    {
        SourceColumn = sourceColumn;
        DestinationColumn = destinationColumn;
    }
}

///<summary> Builder para configurar operaciones bulk de manera fluida. </summary>
public class BulkOperationsConfigurationBuilder
{
    private readonly BulkOperationsConfiguration Configuration = new BulkOperationsConfiguration();

    /// <summary> Establece la tabla destino. </summary>
    public BulkOperationsConfigurationBuilder ToTable(string tableName)
    {
        Configuration.DestinationTableName = tableName;
        return this;
    }

    /// <summary> Establece el tamaño del lote. </summary>
    public BulkOperationsConfigurationBuilder WithBatchSize(int batchSize)
    {
        Configuration.BatchSize = batchSize;
        return this;
    }

    /// <summary> Establece el timeout para operaciones bulk. </summary>
    public BulkOperationsConfigurationBuilder WithTimeout(int timeoutInSeconds)
    {
        Configuration.BulkCopyTimeout = timeoutInSeconds;
        return this;
    }

    /// <summary> Agrega un mapeo de columna. </summary>
    public BulkOperationsConfigurationBuilder MapColumn(string sourceColumn, string destinationColumn)
    {
        Configuration.ColumnMappings.Add(new ColumnMapping(sourceColumn, destinationColumn));
        return this;
    }

    /// <summary>
    /// Habilita logging detallado.
    /// </summary>
    public BulkOperationsConfigurationBuilder EnableDetailedLogging()
    {
        Configuration.EnableDetailedLogging = true;
        return this;
    }

    /// <summary>
    /// Construye la configuración.
    /// </summary>
    public BulkOperationsConfiguration Build() => Configuration;
}
