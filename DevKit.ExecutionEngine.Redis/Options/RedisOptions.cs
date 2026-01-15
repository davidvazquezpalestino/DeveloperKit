namespace DevKit.ExecutionEngine.Redis.Options;

/// <summary>
/// Clase de opciones para configurar la conexión y comportamiento de Redis.
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// Clave de sección para configuración.
    /// </summary>
    public const string SectionKey = nameof(RedisOptions);

    /// <summary>
    /// Cadena de conexión para Redis.
    /// </summary>
    public string ConnectionRedis { get; set; }

    /// <summary>
    /// Nombre del entorno (usado en claves).
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    /// Número de días para el TTL del caché.
    /// </summary>
    public int DiasCache { get; set; }

}