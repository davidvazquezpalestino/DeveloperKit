namespace DevKit.ExecutionEngine.SQLServer.Settings;

/// <summary>
/// Opciones de configuración para SQL Server Repository.
/// </summary>
public class SqlOptions
{
    /// <summary>
    /// Constructor por defecto que inicializa las opciones anidadas para evitar nulls.
    /// </summary>
    public SqlOptions()
    {
        BulkCopy = new BulkCopyOptions();
        ConnectionPooling = new ConnectionPoolingOptions();
        SqlAuth = new SqlAuthOptions();
    }

    /// <summary>
    /// Cadena de conexión a la base de datos SQL Server.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Tiempo de espera para comandos SQL en segundos. Por defecto: 30 segundos.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Tiempo de espera para conexiones en segundos. Por defecto: 30 segundos.
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Configuración para operaciones Bulk Copy.
    /// </summary>
    public BulkCopyOptions BulkCopy { get; set; }

    /// <summary>
    /// Configuración de pooling de conexiones. Por defecto: habilitado.
    /// </summary>
    public ConnectionPoolingOptions ConnectionPooling { get; set; }

    /// <summary>
    /// Configuración de pooling de nombre de aplicación.
    /// </summary>
    public Func<string> ConfigureApplication { get; set; }

    /// <summary>
    /// Opción alternativa para construir la cadena de conexión indicando servidor, base de datos, usuario y contraseña.
    /// Si <see cref="ConnectionString"/> está vacío, se construirá con estos valores.
    /// </summary>
    public SqlAuthOptions SqlAuth { get; set; }

    /// <summary>
    /// Obtiene la cadena de conexión efectiva usando <see cref="ConnectionString"/>,
    /// o la construye desde <see cref="SqlAuth"/> si es válida.
    /// </summary>
    public string GetConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            return ConnectionString;
        }

        if (SqlAuth.IsConfigured())
        {
            // Construcción básica; se agregan opciones comunes si están configuradas
            List<string> parts = new()
            {
                $"Server={SqlAuth.Server}",
                $"Database={SqlAuth.Database}",
                $"User Id={SqlAuth.UserId}",
                $"Password={SqlAuth.Password}"
            };

            // Solo agregar estas claves si están habilitadas para evitar problemas con versiones antiguas
            if (SqlAuth.TrustServerCertificate)
            {
                parts.Add("TrustServerCertificate=True");
            }

            if (SqlAuth.MultipleActiveResultSets)
            {
                parts.Add("MultipleActiveResultSets=True");
            }

            if (ConnectionTimeout > 0)
            {
                parts.Add($"Connect Timeout={ConnectionTimeout}");
            }

            if (ConnectionPooling.Pooling)
            {
                parts.Add("Pooling=True");
            }
            else
            {
                parts.Add("Pooling=False");
            }

            if (ConnectionPooling.MinPoolSize > 0)
            {
                parts.Add($"Min Pool Size={ConnectionPooling.MinPoolSize}");
            }

            if (ConnectionPooling.MaxPoolSize > 0)
            {
                parts.Add($"Max Pool Size={ConnectionPooling.MaxPoolSize}");
            }

            string appName = ConfigureApplication?.Invoke();
            if (!string.IsNullOrWhiteSpace(appName))
            {
                parts.Add($"Application Name={appName}");
            }

            ConnectionString = string.Join(";", parts);
            return ConnectionString;
        }

        return string.Empty;
    }

}

/// <summary>
/// Opciones específicas para operaciones Bulk Copy.
/// </summary>
public class BulkCopyOptions
{
    /// <summary>
    /// Tamaño del lote para operaciones bulk. Por defecto: 1000.
    /// </summary>
    public int BatchSize { get; set; }

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

/// <summary>
/// Datos mínimos para construir una cadena de conexión con autenticación SQL (usuario/contraseña).
/// </summary>
public class SqlAuthOptions
{
    /// <summary>
    /// Nombre o dirección del servidor SQL Server.
    /// </summary>
    public string Server { get; set; }

    /// <summary>
    /// Nombre de la base de datos.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// Usuario de SQL Server.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Contraseña del usuario de SQL Server.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Indica si se debe confiar en el certificado del servidor.
    /// Por defecto: true.
    /// </summary>
    public bool TrustServerCertificate { get; set; } = true;

    /// <summary>
    /// Habilita Multiple Active Result Sets (MARS).
    /// Por defecto: true.
    /// </summary>
    public bool MultipleActiveResultSets { get; set; } = true;

    /// <summary>
    /// Indica si los campos necesarios están completos para construir la cadena.
    /// </summary>
    public bool IsConfigured()
        => !string.IsNullOrWhiteSpace(Server)
           && !string.IsNullOrWhiteSpace(Database)
           && !string.IsNullOrWhiteSpace(UserId)
           && !string.IsNullOrWhiteSpace(Password);
}

