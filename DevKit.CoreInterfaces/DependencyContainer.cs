namespace CoreInterfaces;

/// <summary>Contenedor para la inyección de dependencias de las interfaces principales.</summary>
public static class DependencyContainer
{
    extension(IServiceCollection services)
    {
        /// <summary>Agrega el servicio de procesamiento de respuestas al contenedor de dependencias.</summary>
        public IServiceCollection AddProcessResponse(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAdd(new ServiceDescriptor(
                typeof(IProcessResponse<>),
                typeof(ProcessResponse<>),
                lifetime));

            return services;
        }

        /// <summary>Agrega el servicio de respuestas para WebApi al contenedor de dependencias.</summary>
        public IServiceCollection AddWebApiResponse(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAdd(new ServiceDescriptor(
                typeof(IWebApiResponse<>),
                typeof(WebApiResponse<>),
                lifetime));

            return services;
        }
    }
}