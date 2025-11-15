namespace DevKit.MessageBoxWin;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contenedor de dependencias para el servicio de mensajes de Windows Forms.
/// </summary>
public static class DependencyContainer
{

    /// <summary>
    /// Contenedor de dependencias para el servicio de mensajes de Windows Forms.
    /// </summary>

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registra el servicio de mensajes de Windows Forms en el contenedor de dependencias.
        /// </summary>
        public IServiceCollection AddDotNetCoreMessageBox(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                    services.AddTransient<IMessageBox<DialogResult>, WindowsMessageBox>();
                    break;

                case ServiceLifetime.Scoped:
                default:
                    services.AddScoped<IMessageBox<DialogResult>, WindowsMessageBox>();
                    break;

                case ServiceLifetime.Singleton:
                    services.AddSingleton<IMessageBox<DialogResult>, WindowsMessageBox>();
                    break;
            }

            return services;
        }
    }
}