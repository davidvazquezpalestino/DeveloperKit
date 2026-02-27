namespace DevKit.ExecutionEngine.Redis;

/// <summary>
/// Proporciona métodos de extensión para registrar los servicios de RedisCache en el contenedor de dependencias.
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Registra el servicio de RedisCache en el contenedor de dependencias.
    /// </summary>
    /// <param name="services">La colección de servicios donde se registrará el caché.</param>
    /// <param name="lifetime">El tiempo de vida del servicio (por defecto <see cref="ServiceLifetime.Scoped"/>).</param>
    /// <returns>La misma colección de servicios para encadenar llamadas.</returns>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAdd(new ServiceDescriptor(
            typeof(ICacheService),
            typeof(CacheService),
            lifetime));

        return services;
    }
}