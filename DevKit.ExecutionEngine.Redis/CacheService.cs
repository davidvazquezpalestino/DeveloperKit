namespace DevKit.ExecutionEngine.Redis;

/// <summary>
/// Implementación del servicio de caché que utiliza Redis como backend.
/// Maneja la conexión a Redis y las operaciones de caché.
/// </summary>
internal class CacheService : ICacheService
{
    private readonly IOptions<RedisOptions> RedisOptions;
    private readonly IDatabase DataBase;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de caché.
    /// Establece la conexión a Redis utilizando las opciones proporcionadas.
    /// </summary>
    /// <param name="redisOptions">Opciones de configuración para Redis.</param>
    public CacheService(IOptions<RedisOptions> redisOptions)
    {
        RedisOptions = redisOptions;
        string connectionRedis = redisOptions.Value.ConnectionRedis;
        ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(connectionRedis,
            configuration =>
            {
                configuration.ConnectRetry = 3;
                configuration.SyncTimeout = 10000;
                configuration.AbortOnConnectFail = true;
                configuration.KeepAlive = 180;
            });
        DataBase = connection.GetDatabase();
    }

    /// <inheritdoc/>
    public async Task<T> GetOrSetAsync<T>(Expression<Func<Task<T>>> expression)
    {
        // Obtener info del método y parámetros
        string fullKey = BuildKey(ExpressionConditionExtractor.BuildRedisKey(expression));

        string cached = await DataBase.StringGetAsync(fullKey);
        if (string.IsNullOrWhiteSpace(cached) == false)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(cached);
        }

        // Compilar y ejecutar el factory
        Func<Task<T>> factory = expression.Compile();
        T value = await factory();

        // Validar null o vacío
        if (IsNullOrEmpty(value))
        {
            return default;
        }

        TimeSpan effectiveTtl = TimeSpan.FromDays(Math.Max(0, RedisOptions.Value.DiasCache));

        await SetInternalAsync(fullKey, value, effectiveTtl);
        return value;
    }

    /// <inheritdoc/>
    public async Task InvalidateCacheAsync(params Expression[] expressions)
    {
        foreach (var expression in expressions)
        {
            if (expression is LambdaExpression lambda)
            {
                string fullKey = BuildKey(ExpressionConditionExtractor.BuildRedisKey(lambda));
                await DataBase.KeyDeleteAsync(fullKey);
            }
        }
    }

    /// <summary>
    /// Establece un valor en Redis con TTL y etiquetas opcionales.
    /// </summary>
    /// <typeparam name="T">El tipo del valor a almacenar.</typeparam>
    /// <param name="fullKey">La clave completa para almacenar el valor.</param>
    /// <param name="value">El valor a almacenar.</param>
    /// <param name="ttl">El tiempo de vida del valor en caché.</param>
    /// <param name="tags">Etiquetas opcionales para agrupar claves.</param>
    private async Task SetInternalAsync<T>(string fullKey, T value, TimeSpan ttl, params string[] tags)
    {
        await DataBase.StringSetAsync(fullKey, System.Text.Json.JsonSerializer.Serialize(value), ttl);
        if (tags?.Length > 0)
        {
            foreach (string tag in tags)
            {
                string tagKey = BuildTagKey(tag);
                await DataBase.SetAddAsync(tagKey, fullKey);
                await DataBase.KeyExpireAsync(tagKey, ttl);
            }
        }
    }
    /// <summary>
    /// Evalúa si un objeto es null o vacío (colecciones, strings).
    /// </summary>
    private static bool IsNullOrEmpty<T>(T value)
    {
        if (value == null) return true;

        switch (value)
        {
            case string s:
                return string.IsNullOrWhiteSpace(s);

            case System.Collections.IEnumerable enumerable:
                // Verificar si la colección tiene elementos
                foreach (var _ in enumerable)
                {
                    return false; // tiene al menos uno
                }
                return true;

            default:
                return false;
        }
    }
    /// <summary>
    /// Construye la clave completa incluyendo el nombre del entorno.
    /// </summary>
    /// <param name="key">La clave base.</param>
    /// <returns>La clave completa con prefijo de entorno.</returns>
    private string BuildKey(string key) => $"{RedisOptions.Value.Environment}|{key}";

    /// <summary>
    /// Construye la clave para etiquetas.
    /// </summary>
    /// <param name="tag">El nombre de la etiqueta.</param>
    /// <returns>La clave de etiqueta con prefijo de entorno.</returns>
    private string BuildTagKey(string tag) => $"{RedisOptions.Value.Environment}|tag|{tag}";
}
