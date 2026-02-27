namespace DevKit.MessageBoxWin;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contenedor de dependencias para el servicio de mensajes de Windows Forms.
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Registra el servicio de mensajes de Windows Forms en el contenedor de dependencias.
    /// </summary>
    /// <param name="services">La colección de servicios de <see cref="IServiceCollection"/>.</param>
    /// <param name="lifetime">El tiempo de vida del servicio (Scoped por defecto).</param>
    /// <returns>La misma colección de servicios para encadenar llamadas.</returns>
    public static IServiceCollection AddDotNetCoreMessageBox(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
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