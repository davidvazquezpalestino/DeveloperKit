namespace DevKit.Injection.Extensions;

/// <summary>
/// Proporciona métodos de extensión avanzados para el registro automático de dependencias.
/// </summary>
public static class RegisterServices
{
    /// <summary>
    /// Registra automáticamente las clases de un ensamblado en el contenedor de servicios.
    /// </summary>
    /// <param name="services">La colección de servicios de <see cref="IServiceCollection"/>.</param>
    /// <param name="assembly">El ensamblado de donde se cargarán los tipos.</param>
    /// <param name="filter">Filtro opcional para los tipos.</param>
    /// <param name="logTo">Acción opcional para registrar el proceso.</param>
    /// <param name="onlyClass">Si es verdadero, registra la clase directamente. Si es falso (por defecto), registra por sus interfaces.</param>
    /// <param name="lifetime">El tiempo de vida del servicio (Scoped por defecto).</param>
    /// <returns>La misma colección de servicios para encadenar llamadas.</returns>
    public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services,
        Assembly assembly,
        Func<Type, bool> filter = null,
        Action<string> logTo = null,
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
            if (type.IsClass == false || type.IsAbstract)
            {
                return false;
            }
            // Excluir records (clases con método de instancia <Clone>$ generado por el compilador).
            if (type.GetMethod("<Clone>$", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is not null)
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

            if (filter is not null && filter(type) == false)
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
                logTo?.Invoke($"Registrado => {type.Name}");
            }
            else
            {
                foreach (Type service in interfaces)
                {
                    services.TryAdd(new ServiceDescriptor(service, type, lifetime));
                    logTo?.Invoke($"Registrado => {type.Name}:{service.Name}");
                }
            }
        }

        logTo?.Invoke(types.Count == 0
            ? $"No se encontraron tipos en el ensamblado {assemblyName} que coincidan con los criterios."
            : $"Se encontraron {types.Count} tipos en el ensamblado {assemblyName}.");

        return services;
    }

    /// <summary>
    /// Registra servicios desde múltiples ensamblados.
    /// </summary>
    /// <param name="services">La colección de servicios de <see cref="IServiceCollection"/>.</param>
    /// <param name="assemblies">Lista de ensamblados.</param>
    /// <param name="filter">Filtro opcional.</param>
    /// <param name="logTo">Acción de registro opcional.</param>
    /// <param name="lifetime">Tiempo de vida del servicio.</param>
    /// <returns>La misma colección de servicios.</returns>
    public static IServiceCollection AddServicesFromAssemblies(this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        Func<Type, bool> filter = null,
        Action<string> logTo = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        foreach (Assembly assembly in assemblies)
        {
            services.AddServicesFromAssembly(assembly, filter, logTo, false, lifetime);
        }
        return services;
    }

    /// <summary>
    /// Registra servicios del ensamblado actual automáticamente.
    /// </summary>
    /// <param name="services">La colección de servicios de <see cref="IServiceCollection"/>.</param>
    /// <param name="filter"></param>
    /// <param name="logTo">Acción de registro opcional.</param>
    /// <param name="onlyClass"></param>
    /// <param name="lifetime"></param>
    /// <returns>La misma colección de servicios.</returns>
    public static IServiceCollection AddServicesCurrentAssembly(this IServiceCollection services,
        Func<Type, bool> filter = null,
        Action<string> logTo = null,
        bool onlyClass = false,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return services.AddServicesFromAssembly(callingAssembly, filter, logTo, onlyClass, lifetime);
    }

    /// <summary>
    /// Lista los nombres de las clases en un ensamblado sin registrarlas en el contenedor.
    /// </summary>
    /// <param name="assembly">El ensamblado a analizar.</param>
    /// <param name="filter">Filtro opcional para los tipos a incluir.</param>
    /// <param name="logTo">Acción opcional para registrar los resultados.</param>
    /// <returns>Una lista con los nombres de las clases encontradas.</returns>
    public static IReadOnlyList<string> ListTypesInAssembly(Assembly assembly,
        Func<Type, bool> filter = null,
        Action<string> logTo = null)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        bool IsCandidate(Type type)
        {
            if (type == null || type.IsClass == false || type.IsAbstract)
            {
                return false;
            }

            string fullName = type.FullName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fullName) ||
                fullName.Contains("<>") ||
                fullName.Contains("Microsoft") ||
                fullName.Contains("System.Runtime") ||
                fullName.Contains("System.IEquatable"))
            {
                return false;
            }

            return filter == null || filter(type);
        }

        List<string> types = assembly.GetTypes()
            .Where(IsCandidate)
            .Select(t => t.FullName ?? t.Name)
            .OrderBy(name => name)
            .ToList();

        logTo?.Invoke($"Se encontraron {types.Count} tipos en el ensamblado {assembly.GetName().Name}:");
        foreach (string typeName in types)
        {
            logTo?.Invoke($"- {typeName}");
        }

        return types.AsReadOnly();
    }
}