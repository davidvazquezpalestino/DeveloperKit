namespace DevKit.ExecutionEngine.Redis;

/// <summary>
/// Proporciona métodos de extensión para registrar los servicios de RedisCache en el contenedor de dependencias.
/// </summary>
public static class DependencyContainer
{

    extension(IServiceCollection services)
    {
        /// <summary>
        /// registra el servicio de RedisCache en el contenedor de dependencias.
        /// </summary>
        /// <param name="lifetime"></param>
        public IServiceCollection AddRedisCache(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAdd(new ServiceDescriptor(
                typeof(ICacheService),
                typeof(CacheService),
                lifetime));

            return services;
        }
    }
}