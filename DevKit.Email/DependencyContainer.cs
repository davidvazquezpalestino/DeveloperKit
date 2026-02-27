namespace CoreMailKit;

/// <summary>Proporciona métodos de extensión para registrar los servicios de correo electrónico en el contenedor de dependencias.</summary>
public static class DependencyContainer
{
    /// <summary>Registra el servicio de envío de correo electrónico en el contenedor de dependencias.</summary>
    /// <param name="services">La colección de servicios de <see cref="IServiceCollection"/>.</param>
    /// <param name="lifetime">El tiempo de vida del servicio (Singleton por defecto).</param>
    /// <returns>La misma colección de servicios.</returns>
    public static IServiceCollection AddEmailServices(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                services.AddTransient<IEmailSender, SmtpEmailSender>();
                break;

            case ServiceLifetime.Scoped:
                services.AddScoped<IEmailSender, SmtpEmailSender>();
                break;

            case ServiceLifetime.Singleton:
                services.AddSingleton<IEmailSender, SmtpEmailSender>();
                break;
            default:
                services.AddSingleton<IEmailSender, SmtpEmailSender>();
                break;
        }

        return services;
    }
}