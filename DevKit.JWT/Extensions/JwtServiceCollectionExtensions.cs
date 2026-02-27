namespace DevKit.JWT.Extensions;

/// <summary>
/// Extensiones para configurar servicios JWT en el contenedor de dependencias.
/// </summary>
public static class JwtServiceCollectionExtensions
{
    /// <summary>
    /// Agrega los servicios JWT al contenedor de dependencias.
    /// </summary>
    /// <param name="services">La colección de servicios de <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">La configuración de la aplicación.</param>
    /// <returns>La misma colección de servicios.</returns>
    public static IServiceCollection AddJwtServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Configurar opciones JWT
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionKey));

        // Registrar servicios
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAccessToken, AccessToken>();

        return services;
    }

    /// <summary>
    /// Agrega los servicios JWT con configuración personalizada.
    /// </summary>
    /// <param name="services">La colección de servicios de <see cref="IServiceCollection"/>.</param>
    /// <param name="configureOptions">Acción para configurar las opciones.</param>
    /// <returns>La misma colección de servicios.</returns>
    public static IServiceCollection AddJwtServices(this IServiceCollection services, Action<JwtOptions> configureOptions)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.Configure(configureOptions);

        // Registrar servicios
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAccessToken, AccessToken>();

        return services;
    }

    /// <summary>
    /// Agrega autenticación JWT al pipeline.
    /// </summary>
    /// <param name="services">La colección de servicios de <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">La configuración de la aplicación.</param>
    /// <returns>La misma colección de servicios.</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        JwtOptions jwtOptions = configuration.GetSection(JwtOptions.SectionKey).Get<JwtOptions>();

        if (jwtOptions == null)
        {
            throw new InvalidOperationException("JwtOptions no está configurado correctamente");
        }

        byte[] key = Encoding.UTF8.GetBytes(jwtOptions.SecurityKey);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.ValidIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.ValidAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(jwtOptions.ClockSkewMinutes),
                    RequireExpirationTime = true
                };

                // Eventos para logging y manejo personalizado
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        ILogger<JwtBearerEvents> logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                        logger?.LogWarning("Autenticación JWT falló: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        ILogger<JwtBearerEvents> logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                        string userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        logger?.LogInformation("Token JWT validado para usuario {UserId}", userId);
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
