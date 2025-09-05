namespace DevKit.ExecutionEngine.Oracle.Settings;

/// <summary>Opciones de configuración para Oracle Database.</summary>
public class OracleOptions
{
    /// <summary>Cadena de conexión a la base de datos Oracle.</summary>
    public string ConnectionString { get; set; }

    /// <summary>Tiempo de espera para comandos en segundos. Por defecto: 30.</summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>Tiempo de espera para conexiones en segundos. Por defecto: 30.</summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>Configuración de pooling de conexiones.</summary>
    public OracleConnectionPoolingOptions ConnectionPooling { get; set; } = new OracleConnectionPoolingOptions();

    /// <summary>Tamaño de prefetch (FetchSize) en bytes para mejorar rendimiento de lecturas. 0 usa el valor por defecto del proveedor.</summary>
    public int FetchSize { get; set; } = 0;

    /// <summary>Forzar BindByName en comandos Oracle. Por defecto: true.</summary>
    public bool BindByName { get; set; } = true;

    /// <summary>Permite establecer el nombre de la aplicación en la conexión.</summary>
    public Func<string> ConfigureApplication { get; set; }

    /// <summary>Tiempo de espera para operaciones de copia masiva en segundos. Por defecto: 300.</summary>
    public int BulkCopyTimeout { get; set; } = 300;
}

/// <summary>Configuración para el Pooling de conexiones Oracle.</summary>
public class OracleConnectionPoolingOptions
{
    /// <summary>Habilitar pooling de conexiones. Por defecto: true.</summary>
    public bool Pooling { get; set; } = true;

    /// <summary>Tamaño mínimo del pool. Por defecto: 1.</summary>
    public int MinPoolSize { get; set; } = 1;

    /// <summary>Tamaño máximo del pool. Por defecto: 100.</summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>Tiempo de vida (en segundos) de una conexión en el pool. 0 para ilimitado.</summary>
    public int ConnectionLifetime { get; set; } = 0;
}
