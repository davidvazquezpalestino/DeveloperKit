# DotNet.CoreAuthorizationJwt

Biblioteca que facilita la implementación de autenticación JWT en aplicaciones ASP.NET Core.

## Características

- Generación de tokens JWT segura
- Configuración flexible
- Manejo de claves de seguridad
- Validación de tokens
- Soporte para múltiples audiencias
- Configuración de expiración
- Inyección de dependencias

## Instalación

El componente se puede instalar como un paquete NuGet:

```bash
dotnet add package DeveloperKit.CoreAuthorizationJwt
```

## Requisitos

- .NET Core 3.1 o superior
- ASP.NET Core
- Visual Studio 2019 o superior

## Configuración

### Configuración Básica

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonWebToken(options =>
    {
        options.SecurityKey = "your-secret-key-here";
        options.ValidIssuer = "your-domain.com";
        options.ValidAudience = "your-client-id";
        options.ExpireInMinutes = 60;
    });
}
```

### Configuración Avanzada

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonWebToken(options =>
    {
        // Clave de seguridad (mínimo 16 caracteres)
        options.SecurityKey = "your-very-long-secret-key-here";
        
        // Emisor del token
        options.ValidIssuer = "https://api.yourdomain.com";
        
        // Cliente que puede usar el token
        options.ValidAudience = "client-123";
        
        // Duración del token en minutos
        options.ExpireInMinutes = 120;
    }, ServiceLifetime.Scoped);
}
```

## Uso

### Generación de Token

```csharp
public class TokenService
{
    private readonly IAccessToken _accessToken;

    public TokenService(IAccessToken accessToken)
    {
        _accessToken = accessToken;
    }

    public async Task<string> GenerateTokenAsync(string userId, string role)
    {
        return await _accessToken.GetTokenAsync(configurations =>
        {
            configurations.Add("userId", userId);
            configurations.Add("role", role);
        });
    }
}
```

### Validación de Token

```csharp
[Authorize]
public class ProtectedController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProtectedData()
    {
        // El token se valida automáticamente por el middleware de autenticación
        return Ok(new { data = "Protected data" });
    }
}
```

## Configuración de Middleware

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ... otras configuraciones

    app.UseAuthentication();
    app.UseAuthorization();

    // ... otras configuraciones
}
```

## Mejores Prácticas

1. **Seguridad**
   - Usar claves de seguridad largas y complejas
   - No usar la misma clave de seguridad en múltiples entornos
   - Rotar las claves de seguridad periódicamente
   - Usar HTTPS para todas las comunicaciones

2. **Configuración**
   - Almacenar claves de seguridad en variables de entorno
   - Usar diferentes configuraciones por entorno
   - Validar la configuración en el inicio de la aplicación

3. **Manejo de Tokens**
   - Implementar refrescos de token
   - Manejar tokens expirados
   - Validar firmas de token
   - Usar claims apropiados

4. **Rendimiento**
   - Usar caché para tokens frecuentes
   - Implementar limitación de velocidad
   - Manejar eficientemente la memoria

## Soporte

Para reportar errores o solicitar características, por favor abre un issue en el repositorio de GitHub.

## Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.
