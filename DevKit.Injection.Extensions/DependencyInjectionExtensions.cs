namespace DevKit.Injection.Extensions;

/// <summary>
/// Proporciona métodos de extensión avanzados para el registro automático de dependencias.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registra automáticamente las clases de un ensamblado en el contenedor de servicios.
        /// </summary>
        public IServiceCollection AddFromAssembly(Assembly assembly,
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
        public IServiceCollection AddFromAssemblies(IEnumerable<Assembly> assemblies,
            Func<Type, bool> filter = null,
            Action<string> logTo = null,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            foreach (Assembly assembly in assemblies)
            {
                services.AddFromAssembly(assembly, filter, logTo, false, lifetime);
            }
            return services;
        }

        /// <summary>
        /// Registra servicios del ensamblado actual automáticamente.
        /// </summary>
        public IServiceCollection AddCurrentAssembly(Action<string> logTo = null)
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            return services.AddFromAssembly(callingAssembly, logTo: logTo);
        }
    }
}