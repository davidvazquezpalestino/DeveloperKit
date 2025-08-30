namespace DevKit.Injection.Extensions.Attributes;

/// <summary>
/// Atributo para marcar servicios que deben ser registrados como Singleton.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SingletonAttribute : ServiceAttribute
{
    public SingletonAttribute() : base(ServiceLifetime.Singleton) { }
    
    public SingletonAttribute(Type serviceType) : base(serviceType, ServiceLifetime.Singleton) { }
}

/// <summary>
/// Atributo para marcar servicios que deben ser registrados como Transient.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TransientAttribute : ServiceAttribute
{
    public TransientAttribute() : base(ServiceLifetime.Transient) { }
    
    public TransientAttribute(Type serviceType) : base(serviceType, ServiceLifetime.Transient) { }
}

/// <summary>
/// Atributo para marcar servicios que deben ser registrados como Scoped.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ScopedAttribute : ServiceAttribute
{
    public ScopedAttribute() : base(ServiceLifetime.Scoped) { }
    
    public ScopedAttribute(Type serviceType) : base(serviceType, ServiceLifetime.Scoped) { }
}
