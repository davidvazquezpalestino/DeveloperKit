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
        Action<JwtOptions> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        // Registrar IAccessToken según el ciclo de vida
        services.Add(new ServiceDescriptor(typeof(IAccessToken), typeof(AccessToken), lifetime));

        // Registrar IRefreshTokenService según el ciclo de vida
        services.Add(new ServiceDescriptor(typeof(IRefreshTokenService), typeof(RefreshTokenService), lifetime));

        // Configurar JwtOptions manualmente (en .NET Framework no hay IOptions)
        var options = new JwtOptions();
        configure(options);

        // Registrar la instancia configurada como singleton (para reutilizar en servicios)
        services.AddSingleton(options);

        return services;
    }
}