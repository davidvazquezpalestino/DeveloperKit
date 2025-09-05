namespace DevKit.Injection.Extensions;

/// <summary>
/// Proporciona métodos de extensión avanzados para el registro automático de dependencias.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registra automáticamente las clases de un ensamblado en el contenedor de servicios.
    /// </summary>
    public static IServiceCollection AddFromAssembly(this IServiceCollection services,
        Assembly assembly,
        Func<Type, bool>? filter = null,
        Action<string>? logger = null,
        bool onlyClass = false,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        string assemblyName = assembly.GetName().Name ?? assembly.ManifestModule.Name;

        bool IsCandidate(Type type)
        {
            if (type == null)
            {
                return false;
            }
            if (!type.IsClass || type.IsAbstract)
            {
                return false;
            }
            string fullName = type.FullName;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return false;
            }
            if (fullName.Contains("+<") || fullName.Contains("+>") || fullName.Contains("<") || fullName.Contains(">") || fullName.Contains("<>"))
            {
                return false;
            }
            if (fullName.Contains("Microsoft") || fullName.Contains("System.Runtime"))
            {
                return false;
            }
            if (filter != null && !filter(type))
            {
                return false;
            }
            return true;
        }

        List<Type> types = assembly.GetTypes()
            .Where(IsCandidate)
            .ToList();

        foreach (Type type in types)
        {
            Type[] interfaces = type.GetInterfaces();

            if (onlyClass)
            {
                services.TryAdd(new ServiceDescriptor(type, type, lifetime));
                logger?.Invoke($"Registrado => {type.Name}");
            }
            else
            {
                foreach (Type service in interfaces)
                {
                    services.TryAdd(new ServiceDescriptor(service, type, lifetime));
                    logger?.Invoke($"Registrado => {type.Name}:{service.Name}");
                }
            }
        }

        logger?.Invoke(types.Count == 0
            ? $"No se encontraron tipos en el ensamblado {assemblyName} que coincidan con los criterios."
            : $"Se encontraron {types.Count} tipos en el ensamblado {assemblyName}.");

        return services;
    }

    /// <summary>
    /// Registra servicios usando atributos de configuración.
    /// </summary>
    public static IServiceCollection AddServicesWithAttributes(this IServiceCollection services,
        Assembly assembly,
        Action<string>? logger = null)
    {
        ServiceRegistrar registrar = new ServiceRegistrar(services);
        registrar.RegisterFromAttributes(assembly, logger);
        return services;
    }

    /// <summary>
    /// Registra todas las implementaciones de una interfaz específica.
    /// </summary>
    public static IServiceCollection AddImplementationsOf<TInterface>(this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Action<string>? logger = null)
    {
        ServiceRegistrar registrar = new ServiceRegistrar(services);
        registrar.RegisterImplementationsOf<TInterface>(assembly, lifetime, logger);
        return services;
    }

    /// <summary>
    /// Configura servicios con opciones avanzadas y validaciones.
    /// </summary>
    public static ServiceRegistrar ConfigureServices(this IServiceCollection services)
    {
        return new ServiceRegistrar(services);
    }

    /// <summary>
    /// Registra servicios desde múltiples ensamblados.
    /// </summary>
    public static IServiceCollection AddFromAssemblies(this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        Func<Type, bool>? filter = null,
        Action<string>? logger = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        foreach (Assembly assembly in assemblies)
        {
            services.AddFromAssembly(assembly, filter, logger, false, lifetime);
        }
        return services;
    }

    /// <summary>
    /// Registra servicios del ensamblado actual automáticamente.
    /// </summary>
    public static IServiceCollection AddCurrentAssembly(this IServiceCollection services,
        Action<string>? logger = null)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return services.AddFromAssembly(callingAssembly, logger: logger);
    }
}