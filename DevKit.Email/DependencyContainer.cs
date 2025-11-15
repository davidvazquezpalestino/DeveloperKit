namespace CoreMailKit;

/// <summary>Proporciona métodos de extensión para registrar los servicios de correo electrónico en el contenedor de dependencias.</summary>
public static class DependencyContainer
{
    /// <summary>Proporciona métodos de extensión para registrar los servicios de correo electrónico en el contenedor de dependencias.</summary>
    extension(IServiceCollection services)
    {
        /// <summary>Registra el servicio de envío de correo electrónico en el contenedor de dependencias.</summary>
        public IServiceCollection AddEmailServices(ServiceLifetime lifetime = ServiceLifetime.Singleton)
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
                default:
                    services.AddSingleton<IEmailSender, SmtpEmailSender>();
                    break;
            }

            return services;
        }
    }
}