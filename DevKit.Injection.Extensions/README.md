# DevKit.Injection.Extensions

Biblioteca avanzada para la configuración automática de inyección de dependencias en aplicaciones .NET, con soporte para atributos declarativos, validaciones y patrones empresariales.

## 🚀 Características Principales

### **🎯 Registro Automático Avanzado**
- Registro automático de servicios desde ensamblados
- Soporte para múltiples ciclos de vida (Scoped, Singleton, Transient)
- Filtros personalizados y configurables
- Logging detallado y configurable

### **📋 Sistema de Atributos Declarativo**
- Atributos específicos por ciclo de vida (`[Singleton]`, `[Scoped]`, `[Transient]`)
- Configuración declarativa con `[Service]`
- Registro automático basado en atributos
- Soporte para reemplazo de servicios existentes

### **🛡️ Validaciones y Seguridad**
- Detección automática de dependencias circulares
- Validación de tipos y configuraciones
- Manejo robusto de errores
- Logging de advertencias y problemas

### **⚙️ Funcionalidades Empresariales**
- Patrón Decorator para servicios
- Factory methods personalizados
- Registro desde múltiples ensamblados
- Configuración fluida avanzada

## 📦 Instalación

```bash
dotnet add package DevKit.Injection.Extensions
```

## 🔧 Requisitos

- .NET Framework 4.8+ o .NET 8.0+
- Microsoft.Extensions.DependencyInjection.Abstractions
- Compatible con ASP.NET Core, Blazor, MAUI y aplicaciones de consola

## 🚀 Uso Rápido

### **📋 Registro con Atributos (Recomendado)**

```csharp
// 1. Marcar servicios con atributos
[Singleton]
public class CacheService : ICacheService 
{
    public void ClearCache() { }
}

[Scoped(typeof(IUserService))]
public class UserService : IUserService 
{
    public Task<User> GetUserAsync(int id) => Task.FromResult(new User());
}

[Transient]
public class EmailNotificationService 
{
    public void SendEmail(string to, string subject) { }
}

// 2. Registrar automáticamente
public void ConfigureServices(IServiceCollection services)
{
    services.AddServicesWithAttributes(Assembly.GetExecutingAssembly());
}
```

### **⚡ Registro Automático Básico**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Registro simple desde ensamblado
    services.AddFromAssembly(Assembly.GetExecutingAssembly());
    
    // Con filtros personalizados
    services.AddFromAssembly(
        assembly: Assembly.GetExecutingAssembly(),
        filter: type => type.Name.EndsWith("Service"),
        logger: message => Console.WriteLine($"[DI] {message}"),
        lifetime: ServiceLifetime.Scoped
    );
}
```

## 🎯 Funcionalidades Avanzadas

### **🔧 Configuración Fluida**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureServices()
        .RegisterFromAttributes(Assembly.GetExecutingAssembly())
        .RegisterImplementationsOf<IRepository>(Assembly.GetExecutingAssembly())
        .RegisterDecorator<IUserService, CachedUserService>()
        .ValidateCircularDependencies();
}
```

### **🎨 Registro por Interfaz**

```csharp
// Registra todas las implementaciones de IRepository
services.AddImplementationsOf<IRepository>(
    Assembly.GetExecutingAssembly(),
    ServiceLifetime.Scoped
);

// Registra desde múltiples ensamblados
var assemblies = new[] { 
    Assembly.GetExecutingAssembly(),
    typeof(ExternalService).Assembly 
};
services.AddFromAssemblies(assemblies);
```

### **🏭 Factory Methods**

```csharp
services.ConfigureServices()
    .RegisterFactory<IComplexService>(provider => 
    {
        var dependency = provider.GetRequiredService<IDependency>();
        return new ComplexService(dependency, "custom-config");
    }, ServiceLifetime.Singleton);
```

### **🎭 Patrón Decorator**

```csharp
// Servicio base
[Scoped]
public class UserService : IUserService 
{
    public Task<User> GetUserAsync(int id) => /* implementación */;
}

// Decorator con cache
public class CachedUserService : IUserService 
{
    private readonly IUserService _inner;
    private readonly IMemoryCache _cache;
    
    public CachedUserService(IUserService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }
    
    public Task<User> GetUserAsync(int id)
    {
        return _cache.GetOrCreateAsync($"user_{id}", 
            _ => _inner.GetUserAsync(id));
    }
}

// Configuración
services.ConfigureServices()
    .RegisterFromAttributes(Assembly.GetExecutingAssembly())
    .RegisterDecorator<IUserService, CachedUserService>();
```

## 📋 Atributos Disponibles

| Atributo | Descripción | Ejemplo |
|----------|-------------|---------|
| `[Service]` | Configuración general | `[Service(ServiceLifetime.Scoped)]` |
| `[Singleton]` | Registro como Singleton | `[Singleton(typeof(ICache))]` |
| `[Scoped]` | Registro como Scoped | `[Scoped]` |
| `[Transient]` | Registro como Transient | `[Transient(typeof(INotification))]` |

## 🚀 Métodos de Extensión

| Método | Descripción |
|--------|-------------|
| `AddFromAssembly` | Registro desde un ensamblado |
| `AddFromAssemblies` | Registro desde múltiples ensamblados |
| `AddServicesWithAttributes` | Registro basado en atributos |
| `AddImplementationsOf<T>` | Todas las implementaciones de una interfaz |
| `AddCurrentAssembly` | Registro automático del ensamblado actual |
| `ConfigureServices` | Configuración fluida avanzada |

## 🛡️ Validaciones y Debugging

### **Detección de Dependencias Circulares**

```csharp
services.ConfigureServices()
    .RegisterFromAttributes(Assembly.GetExecutingAssembly())
    .ValidateCircularDependencies(); // Lanza excepción si hay ciclos
```

### **Logging Detallado**

```csharp
services.AddFromAssembly(
    Assembly.GetExecutingAssembly(),
    logger: message => _logger.LogInformation("[DI] {Message}", message)
);

// Ver log de registros
var registrar = services.ConfigureServices();
registrar.RegisterFromAttributes(Assembly.GetExecutingAssembly());
var logs = registrar.GetRegistrationLog();
```

## 💡 Mejores Prácticas

### **🎯 Uso de Atributos**
- Usar `[Singleton]` para servicios sin estado (caches, configuraciones)
- Usar `[Scoped]` para servicios de negocio (repositories, services)
- Usar `[Transient]` para servicios ligeros y sin estado compartido

### **🔍 Filtros Inteligentes**
```csharp
services.AddFromAssembly(
    Assembly.GetExecutingAssembly(),
    filter: type => 
        type.Name.EndsWith("Service") || 
        type.Name.EndsWith("Repository") ||
        type.GetInterfaces().Any(i => i.Name.StartsWith("I"))
);
```

### **📊 Organización por Módulos**
```csharp
// Program.cs o Startup.cs
services.AddFromAssembly(typeof(BusinessLogic.IUserService).Assembly);
services.AddFromAssembly(typeof(DataAccess.IUserRepository).Assembly);
services.AddFromAssembly(typeof(Infrastructure.IEmailService).Assembly);
```

## 🔧 Casos de Uso Avanzados

### **Registro Condicional**
```csharp
services.AddFromAssembly(
    Assembly.GetExecutingAssembly(),
    filter: type => 
    {
        // Solo en desarrollo
        if (Environment.IsDevelopment())
            return !type.Name.Contains("Production");
        
        // Solo servicios de producción
        return !type.Name.Contains("Mock") && !type.Name.Contains("Test");
    }
);
```

### **Múltiples Implementaciones**
```csharp
// Registrar todas las implementaciones de INotificationProvider
services.AddImplementationsOf<INotificationProvider>(Assembly.GetExecutingAssembly());

// Usar en runtime
public class NotificationService
{
    public NotificationService(IEnumerable<INotificationProvider> providers) 
    {
        // Usar todas las implementaciones
    }
}
```

## 📚 Compatibilidad

- ✅ **ASP.NET Core** 3.1+
- ✅ **Blazor Server/WASM**
- ✅ **MAUI**
- ✅ **Worker Services**
- ✅ **Console Applications**
- ✅ **.NET Framework** 4.8+

## 🆚 Comparación con Otras Bibliotecas

| Característica | DevKit.Injection | Scrutor | Autofac |
|----------------|------------------|---------|---------|
| Atributos Declarativos | ✅ | ❌ | ✅ |
| Validación Circular | ✅ | ❌ | ✅ |
| Patrón Decorator | ✅ | ✅ | ✅ |
| Múltiples Ensamblados | ✅ | ✅ | ✅ |
| .NET Framework | ✅ | ❌ | ✅ |
| Configuración Fluida | ✅ | ✅ | ✅ |

## 📄 Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.
