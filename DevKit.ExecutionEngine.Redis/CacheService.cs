namespace DevKit.ExecutionEngine.Redis;

/// <summary>
/// Implementación del servicio de caché que utiliza Redis como backend.
/// Maneja la conexión a Redis y las operaciones de caché.
/// </summary>
internal class CacheService : ICacheService
{
    /// <summary>Separador utilizado para construir las claves de Redis.</summary>
    private const string Separator = "|";

    /// <summary>Prefijo utilizado para las claves de etiquetas.</summary>
    private const string TagPrefix = "tag";

    /// <summary>Opciones de configuración para Redis.</summary>
    private readonly IOptions<RedisOptions> RedisOptions;

    /// <summary>Instancia de la base de datos de Redis.</summary>
    private readonly IDatabase DataBase;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CacheService"/>.
    /// </summary>
    /// <param name="redisOptions">Opciones de configuración.</param>
    public CacheService(IOptions<RedisOptions> redisOptions)
    {
        RedisOptions = redisOptions;
        DataBase = InitializeDatabase(redisOptions.Value.ConnectionRedis);
    }

    /// <summary>
    /// Inicializa la conexión a la base de datos de Redis.
    /// </summary>
    /// <param name="connectionString">Cadena de conexión.</param>
    /// <returns>La instancia de la base de datos.</returns>
    private static IDatabase InitializeDatabase(string connectionString)
    {
        ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(connectionString,
            configuration =>
            {
                configuration.ConnectRetry = 3;
                configuration.SyncTimeout = 10000;
                configuration.AbortOnConnectFail = true;
                configuration.KeepAlive = 180;
            });

        return connection.GetDatabase();
    }

    /// <inheritdoc/>
    public async Task<T> GetOrSetAsync<T>(Expression<Func<Task<T>>> expression)
    {
        string fullKey = BuildKey(ExpressionConditionExtractor.BuildRedisKey(expression));

        T cachedValue = await GetCachedValueAsync<T>(fullKey);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        return await GetAndCacheValueAsync(fullKey, expression);
    }

    /// <summary>
    /// Obtiene un valor del caché si existe.
    /// </summary>
    /// <typeparam name="T">El tipo del valor.</typeparam>
    /// <param name="key">La clave de caché.</param>
    /// <returns>El valor deserializado o el valor por defecto.</returns>
    private async Task<T> GetCachedValueAsync<T>(string key)
    {
        string cached = await DataBase.StringGetAsync(key);
        return string.IsNullOrWhiteSpace(cached)
            ? default
            : JsonSerializer.Deserialize<T>(cached);
    }

    /// <summary>
    /// Obtiene el valor ejecutando la expresión y lo almacena en caché.
    /// </summary>
    /// <typeparam name="T">El tipo del valor.</typeparam>
    /// <param name="key">La clave de caché.</param>
    /// <param name="expression">La expresión a ejecutar.</param>
    /// <returns>El valor producido.</returns>
    private async Task<T> GetAndCacheValueAsync<T>(string key, Expression<Func<Task<T>>> expression)
    {
        Func<Task<T>> compileFunc = expression.Compile();
        T value = await compileFunc();

        if (IsNullOrEmpty(value))
        {
            return default;
        }

        TimeSpan effectiveTtl = TimeSpan.FromDays(Math.Max(0, RedisOptions.Value.DiasCache));
        await SetInternalAsync(key, value, effectiveTtl);

        return value;
    }

    /// <inheritdoc/>
    public async Task InvalidateAsync(params Expression[] expressions)
    {
        if (expressions != null && expressions.Length != 0)
        {
            List<Task> tasks = new(expressions.Length);

            foreach (Expression expression in expressions)
            {
                if (expression is LambdaExpression lambda)
                {
                    string fullKey = BuildKey(ExpressionConditionExtractor.BuildRedisKey(lambda));
                    tasks.Add(DataBase.KeyDeleteAsync(fullKey));
                }
            }
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Establece un valor en la base de datos de Redis de forma interna.
    /// </summary>
    /// <typeparam name="T">El tipo del valor.</typeparam>
    /// <param name="fullKey">La clave completa.</param>
    /// <param name="value">El valor a almacenar.</param>
    /// <param name="ttl">El tiempo de vida.</param>
    /// <param name="tags">Etiquetas opcionales.</param>
    private async Task SetInternalAsync<T>(string fullKey, T value, TimeSpan ttl, params string[] tags)
    {
        await DataBase.StringSetAsync(fullKey, JsonSerializer.Serialize(value), ttl);

        if (tags?.Length > 0)
        {
            await AddTagsToKeyAsync(fullKey, ttl, tags);
        }
    }

    /// <summary>
    /// Asocia etiquetas a una clave específica.
    /// </summary>
    /// <param name="fullKey">La clave original.</param>
    /// <param name="ttl">El tiempo de vida.</param>
    /// <param name="tags">Las etiquetas a agregar.</param>
    private async Task AddTagsToKeyAsync(string fullKey, TimeSpan ttl, string[] tags)
    {
        foreach (string tag in tags)
        {
            string tagKey = BuildTagKey(tag);
            await DataBase.SetAddAsync(tagKey, fullKey);
            await DataBase.KeyExpireAsync(tagKey, ttl);
        }
    }

    /// <summary>
    /// Determina si un valor es nulo o está vacío.
    /// </summary>
    /// <typeparam name="T">El tipo del valor.</typeparam>
    /// <param name="value">El valor a evaluar.</param>
    /// <returns>Verdadero si está vacío; de lo contrario, falso.</returns>
    private static bool IsNullOrEmpty<T>(T value) => value switch
    {
        null => true,
        string s => string.IsNullOrWhiteSpace(s),
        System.Collections.IEnumerable enumerable => enumerable.Cast<object>().Any() == false,
        _ => false
    };

    /// <summary>
    /// Construye una clave completa incluyendo el prefijo del entorno.
    /// </summary>
    /// <param name="key">La clave base.</param>
    /// <returns>La clave completa.</returns>
    private string BuildKey(string key) => $"{RedisOptions.Value.Environment}{Separator}{key}";

    /// <summary>
    /// Construye una clave para una etiqueta específica.
    /// </summary>
    /// <param name="tag">El nombre de la etiqueta.</param>
    /// <returns>La clave de etiqueta completa.</returns>
    private string BuildTagKey(string tag) => $"{RedisOptions.Value.Environment}{Separator}{TagPrefix}{Separator}{tag}";
}
