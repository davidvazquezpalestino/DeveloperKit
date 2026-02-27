namespace DevKit.JWT;

/// <summary>
/// Proporciona métodos de extensión para registrar los servicios de JWT en el contenedor de dependencias.
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Registra el servicio de generación de tokens JWT en el contenedor de servicios.
    /// </summary>
    /// <param name="services">La colección de servicios de <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">Acción para configurar las opciones de <see cref="JwtOptions"/>.</param>
    /// <param name="lifetime">El tiempo de vida del servicio (Scoped por defecto).</param>
    /// <returns>La misma colección de servicios para encadenar llamadas.</returns>
    public static IServiceCollection AddJsonWebToken(this IServiceCollection services, Action<JwtOptions> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        // Registrar IAccessToken según el ciclo de vida
        services.Add(new ServiceDescriptor(typeof(IAccessToken), typeof(AccessToken), lifetime));

        // Registrar IRefreshTokenService según el ciclo de vida
        services.Add(new ServiceDescriptor(typeof(IRefreshTokenService), typeof(RefreshTokenService), lifetime));

        // Configurar JwtOptions manualmente (en .NET Framework no hay IOptions)
        JwtOptions options = new JwtOptions();
        configure(options);

        // Registrar la instancia configurada como singleton (para reutilizar en servicios)
        services.AddSingleton(options);

        return services;
    }
}