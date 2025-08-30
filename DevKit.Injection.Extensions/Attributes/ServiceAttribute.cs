namespace DevKit.Injection.Extensions.Attributes;

/// <summary>
/// Atributo para marcar clases que deben ser registradas automáticamente en el contenedor DI.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ServiceAttribute : Attribute
{
    /// <summary>
    /// Ciclo de vida del servicio.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Tipo de interfaz a implementar. Si es null, se registra como clase concreta.
    /// </summary>
    public Type? ServiceType { get; set; }

    /// <summary>
    /// Indica si debe reemplazar registros existentes.
    /// </summary>
    public bool Replace { get; set; } = false;

    /// <summary>
    /// Crea una instancia del atributo con configuración por defecto.
    /// </summary>
    public ServiceAttribute() { }

    /// <summary>
    /// Crea una instancia del atributo con ciclo de vida específico.
    /// </summary>
    public ServiceAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }

    /// <summary>
    /// Crea una instancia del atributo con tipo de servicio específico.
    /// </summary>
    public ServiceAttribute(Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
    }
}
