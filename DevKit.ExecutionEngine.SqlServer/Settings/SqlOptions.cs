namespace DevKit.ExecutionEngine.SqlServer.Settings;

/// <summary>
/// Opciones de configuración para SQL Server Repository.
/// </summary>
public class SqlOptions
{
    /// <summary>
    /// Cadena de conexión a la base de datos SQL Server.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Tiempo de espera para comandos SQL en segundos. Por defecto: 30 segundos.
    /// </summary>
    public int CommandTimeout { get; set; }

    /// <summary>
    /// Tiempo de espera para conexiones en segundos. Por defecto: 30 segundos.
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Configuración para operaciones Bulk Copy.
    /// </summary>
    public BulkCopyOptions BulkCopy { get; set; } = new BulkCopyOptions();

    /// <summary>
    /// Configuración de pooling de conexiones. Por defecto: habilitado.
    /// </summary>
    public ConnectionPoolingOptions ConnectionPooling { get; set; } = new ConnectionPoolingOptions();

    /// <summary>
    /// Configuración de pooling de nombre de aplicación.
    /// </summary>
    public Func<string> ConfigureApplication { get; set; }

}

/// <summary>
/// Opciones específicas para operaciones Bulk Copy.
/// </summary>
public class BulkCopyOptions
{
    /// <summary>
    /// Tamaño del lote para operaciones bulk. Por defecto: 1000.
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Tiempo de espera para operaciones bulk en segundos. Por defecto: 300 (5 minutos).
    /// </summary>
    public int BulkCopyTimeout { get; set; } = 300;

    /// <summary>
    /// Opciones de SqlBulkCopy. Por defecto: Default.
    /// </summary>
    public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; } = SqlBulkCopyOptions.Default;

    /// <summary>
    /// Número de filas después del cual se dispara el evento de notificación. Por defecto: 0 (deshabilitado).
    /// </summary>
    public int NotifyAfter { get; set; } = 0;

    /// <summary>
    /// Habilitar logging de progreso para operaciones bulk. Por defecto: false.
    /// </summary>
    public bool EnableProgressLogging { get; set; } = false;
}

/// <summary>
/// Opciones de configuración para pooling de conexiones.
/// </summary>
public class ConnectionPoolingOptions
{
    /// <summary>
    /// Habilitar pooling de conexiones. Por defecto: true.
    /// </summary>
    public bool Pooling { get; set; } = true;

    /// <summary>
    /// Tamaño mínimo del pool de conexiones. Por defecto: 5.
    /// </summary>
    public int MinPoolSize { get; set; } = 5;

    /// <summary>
    /// Tamaño máximo del pool de conexiones. Por defecto: 100.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

}
