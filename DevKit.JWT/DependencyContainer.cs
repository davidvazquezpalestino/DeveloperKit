namespace DevKit.JWT;

/// <summary>
/// Proporciona métodos de extensión para registrar los servicios de JWT en el contenedor de dependencias.
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Registra el servicio de generación de tokens JWT en el contenedor de servicios.
    /// </summary>
    public static IServiceCollection AddJsonWebToken(this IServiceCollection services,
        Action<JwtOptions> configure, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<IAccessToken, AccessToken>();
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped<IAccessToken, AccessToken>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<IAccessToken, AccessToken>();
                break;
        }

        services.Configure(configure);

        return services;
    }
}