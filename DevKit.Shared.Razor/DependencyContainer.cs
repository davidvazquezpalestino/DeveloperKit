namespace DevKit.Shared.Razor;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Proporciona métodos de extensión para registrar servicios de componentes de UI en el contenedor de dependencias.
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Registra los servicios de componentes de UI personalizados en el contenedor de dependencias.
    /// </summary>
    public static IServiceCollection AddComponentsCustom(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAdd(new ServiceDescriptor(
            typeof(ISweetAlert), 
            typeof(SweetAlert), 
            lifetime));
            
        return services;
    }
}

