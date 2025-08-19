namespace DevKit.CrystalToolkit;

using Microsoft.Extensions.DependencyInjection;

/// <summary>Proporciona métodos de extensión para registrar servicios de Crystal Reports en el contenedor de dependencias.</summary>
public static class DependencyContainer
{
    /// <summary>Registra el servicio de Crystal Reports en el contenedor de dependencias.</summary>
    public static IServiceCollection AddCrystalReport(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<IReport, Report>();
                break;
                
            case ServiceLifetime.Transient:
                services.AddTransient<IReport, Report>();
                break;
                
            case ServiceLifetime.Scoped:
            default:
                services.AddScoped<IReport, Report>();
                break;
        }
        
        return services;
    }
}

