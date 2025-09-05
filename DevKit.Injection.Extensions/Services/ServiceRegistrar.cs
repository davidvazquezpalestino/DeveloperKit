namespace DevKit.Injection.Extensions.Services;

/// <summary>
/// Servicio para registro avanzado de dependencias con soporte para atributos y validaciones.
/// </summary>
public class ServiceRegistrar(IServiceCollection services)
{
    private readonly List<string> RegistrationLog = new List<string>();

    /// <summary>
    /// Registra servicios basados en atributos desde un ensamblado.
    /// </summary>
    public ServiceRegistrar RegisterFromAttributes(Assembly assembly, Action<string>? logger = null)
    {
        List<Type> typesWithAttributes = assembly.GetTypes()
                                        .Where(type => type.IsClass && !type.IsAbstract)
                                        .Where(type => type.GetCustomAttribute<ServiceAttribute>() != null)
                                        .ToList();

        foreach (Type type in typesWithAttributes)
        {
            ServiceAttribute attribute = type.GetCustomAttribute<ServiceAttribute>()!;
            RegisterServiceWithAttribute(type, attribute, logger);
        }

        logger?.Invoke($"Registrados {typesWithAttributes.Count} servicios con atributos desde {assembly.GetName().Name}");
        return this;
    }

    /// <summary>
    /// Registra servicios que implementan una interfaz específica.
    /// </summary>
    public ServiceRegistrar RegisterImplementationsOf<TInterface>(
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Action<string>? logger = null)
    {
        Type interfaceType = typeof(TInterface);
        List<Type> implementations = assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => interfaceType.IsAssignableFrom(type))
            .ToList();

        foreach (Type implementation in implementations)
        {
            services.Add(new ServiceDescriptor(interfaceType, implementation, lifetime));
            string message = $"Registrado {implementation.Name} como {interfaceType.Name} ({lifetime})";
            RegistrationLog.Add(message);
            logger?.Invoke(message);
        }

        return this;
    }

    /// <summary>
    /// Registra decoradores para un servicio específico.
    /// </summary>
    public ServiceRegistrar RegisterDecorator<TService, TDecorator>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TDecorator : class, TService
    {
        services.Decorate<TService, TDecorator>();
        string message = $"Registrado decorador {typeof(TDecorator).Name} para {typeof(TService).Name}";
        RegistrationLog.Add(message);
        return this;
    }

    /// <summary>
    /// Registra servicios con factory personalizado.
    /// </summary>
    public ServiceRegistrar RegisterFactory<TService>(
        Func<IServiceProvider, TService> factory,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
    {
        services.Add(new ServiceDescriptor(typeof(TService), factory, lifetime));
        string message = $"Registrado factory para {typeof(TService).Name} ({lifetime})";
        RegistrationLog.Add(message);
        return this;
    }

    /// <summary>
    /// Valida que no haya dependencias circulares.
    /// </summary>
    public ServiceRegistrar ValidateCircularDependencies()
    {
        // Implementación básica de validación
        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        try
        {
            // Intentar resolver todos los servicios registrados
            foreach (ServiceDescriptor service in services.Where(s => !s.ServiceType.IsAbstract && !s.ServiceType.IsInterface))
            {
                serviceProvider.GetService(service.ServiceType);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("circular") || ex.Message.Contains("cycle"))
        {
            throw new InvalidOperationException("Se detectaron dependencias circulares en el contenedor DI", ex);
        }
        catch (Exception ex)
        {
            // Log pero no fallar por otros errores de resolución
            string message = $"Advertencia durante validación de dependencias: {ex.Message}";
            RegistrationLog.Add(message);
        }

        return this;
    }

    /// <summary>
    /// Obtiene el log de registros realizados.
    /// </summary>
    public IReadOnlyList<string> GetRegistrationLog() => RegistrationLog.AsReadOnly();

    private void RegisterServiceWithAttribute(Type implementationType, ServiceAttribute attribute, Action<string>? logger)
    {
        Type serviceType = attribute.ServiceType ?? implementationType;
        ServiceDescriptor descriptor = new ServiceDescriptor(serviceType, implementationType, attribute.Lifetime);

        if (attribute.Replace)
        {
            services.Replace(descriptor);
        }
        else
        {
            services.TryAdd(descriptor);
        }

        string message = $"Registrado {implementationType.Name} como {serviceType.Name} ({attribute.Lifetime})";
        RegistrationLog.Add(message);
        logger?.Invoke(message);
    }
}

/// <summary>
/// Extensiones para decoradores de servicios.
/// </summary>
public static class ServiceCollectionDecoratorExtensions
{
    public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services)
        where TDecorator : class, TService
    {
        ServiceDescriptor originalDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));
        if (originalDescriptor == null)
        {
            throw new InvalidOperationException($"Servicio {typeof(TService).Name} no está registrado");
        }

        services.Remove(originalDescriptor);

        services.Add(new ServiceDescriptor(
            typeof(TService),
            provider =>
            {
                object originalService = ActivatorUtilities.CreateInstance(provider, originalDescriptor.ImplementationType!);
                return ActivatorUtilities.CreateInstance<TDecorator>(provider, originalService);
            },
            originalDescriptor.Lifetime));

        return services;
    }
}
