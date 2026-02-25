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

### **📋 Registro Automático (Único método disponible)**

```csharp
// 1. Marcar servicios con atributos (opcional)
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

// 2. Registrar automáticamente
public void ConfigureServices(IServiceCollection services)
{
    services.AddFromAssembly(Assembly.GetExecutingAssembly());
}
```

### **📋 Registro Automático Básico**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Registro simple desde ensamblado
    services.AddFromAssembly(Assembly.GetExecutingAssembly());
    
    // Con filtros personalizados
    services.AddFromAssembly(
        assembly: Assembly.GetExecutingAssembly(),
        filter: type => type.Name.EndsWith("Service"),
        logTo: message => Console.WriteLine($"[DI] {message}"),
        lifetime: ServiceLifetime.Scoped
    );
    
    // Registro del ensamblado actual
    services.AddCurrentAssembly(message => Console.WriteLine($"[DI] {message}"));
    
    // Múltiples ensamblados
    var assemblies = new[] { 
        Assembly.GetExecutingAssembly(),
        typeof(ExternalService).Assembly 
    };
    services.AddFromAssemblies(assemblies);
}
```

## 🎯 Funcionalidades Avanzadas

### **🔧 Configuración Fluida**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Registro desde múltiples ensamblados
    var assemblies = new[] { 
        Assembly.GetExecutingAssembly(),
        typeof(ExternalService).Assembly 
    };
    services.AddFromAssemblies(assemblies);
    
    // Registro con filtros personalizados
    services.AddFromAssembly(
        Assembly.GetExecutingAssembly(),
        filter: type => type.Name.EndsWith("Service"),
        LogTo: message => Console.WriteLine($"[DI] {message}"),
        lifetime: ServiceLifetime.Scoped
    );
}
```

### **🎨 Registro Múltiples Ensamblados**

```csharp
// Registra desde múltiples ensamblados
var assemblies = new[] { 
    Assembly.GetExecutingAssembly(),
    typeof(ExternalService).Assembly 
};
services.AddFromAssemblies(assemblies);

// Registro del ensamblado actual
services.AddCurrentAssembly(message => Console.WriteLine($"[DI] {message}"));
```

### **🔍 Inspección de Ensamblados**

```csharp
// Listar tipos en un ensamblado sin registrarlos
var types = DependencyInjectionExtensions.ListTypesInAssembly(
    Assembly.GetExecutingAssembly(),
    filter: type => type.Name.EndsWith("Service"),
    logTo: message => Console.WriteLine($"[DEBUG] {message}")
);

// Usar la lista de tipos
foreach (var typeName in types)
{
    Console.WriteLine($"Tipo encontrado: {typeName}");
}
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
| `AddCurrentAssembly` | Registro automático del ensamblado actual |
| `ListTypesInAssembly` | Lista los nombres de las clases en un ensamblado sin registrarlas |

## 🔍 Inspección de Ensamblados

### **Listado de Tipos sin Registro**

```csharp
// Obtener lista de tipos en un ensamblado
var types = DependencyInjectionExtensions.ListTypesInAssembly(
    assembly: Assembly.GetExecutingAssembly(),
    filter: type => type.Name.EndsWith("Service"),
    logTo: message => Console.WriteLine($"[DEBUG] {message}")
);

// Usar la lista de tipos
foreach (var typeName in types)
{
    Console.WriteLine($"Tipo encontrado: {typeName}");
}
```

##  Mejores Prácticas

### **🎯 Uso de Filtros**
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
| Registro Automático | ✅ | ✅ | ✅ |
| Múltiples Ensamblados | ✅ | ✅ | ✅ |
| Filtros Personalizados | ✅ | ✅ | ✅ |
| .NET Framework | ✅ | ❌ | ✅ |
| Inspección de Tipos | ✅ | ❌ | ❌ |
| Logging Integrado | ✅ | ❌ | ✅ |

## 📄 Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.
