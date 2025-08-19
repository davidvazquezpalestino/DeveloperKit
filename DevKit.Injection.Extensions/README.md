# DotNet.DependencyInjection

Biblioteca que facilita la configuración de la inyección de dependencias en aplicaciones ASP.NET Core, permitiendo el registro automático de servicios desde un ensamblado.

## Características

- Registro automático de servicios
- Soporte para múltiples ciclos de vida
- Filtros personalizados
- Logging configurable
- Manejo de tipos genéricos
- Integración con ASP.NET Core
- Registro de servicios individuales o por interfaz

## Instalación

El componente se puede instalar como un paquete NuGet:

```bash
dotnet add package DeveloperKit.DependencyInjection
```

## Requisitos

- .NET Core 3.1 o superior
- ASP.NET Core
- Visual Studio 2019 o superior

## Uso Básico

### Configuración Simple

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Registrar servicios desde el ensamblado actual
    services.BindFromAssembly(Assembly.GetExecutingAssembly());
}
```

### Configuración Avanzada

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configuración con todas las opciones
    services.BindFromAssembly(
        assembly: Assembly.GetExecutingAssembly(),
        filter: type => !type.Name.Contains("Test"), // Filtrar tipos
        logger: message => Console.WriteLine(message), // Logger personalizado
        onlyClass: false, // Registrar solo clases o también interfaces
        lifetime: ServiceLifetime.Scoped // Ciclo de vida
    );
}
```

## Parámetros de Configuración

### `assembly`
- Ensamblado del cual se registrarán los servicios
- Ejemplo: `Assembly.GetExecutingAssembly()`

### `filter`
- Función que filtra qué tipos se registran
- Por defecto: `null` (registra todos los tipos)
- Ejemplo: `type => type.Name.Contains("Service")`

### `logger`
- Función para registrar mensajes
- Por defecto: `null`
- Ejemplo: `message => Console.WriteLine(message)`

### `onlyClass`
- Si es `true`, registra solo las clases
- Si es `false`, registra clases e interfaces
- Por defecto: `false`

### `lifetime`
- Ciclo de vida del servicio
- Opciones: `Scoped` (por defecto), `Singleton`, `Transient`

## Ejemplos de Uso

### Registro Básico

```csharp
// Servicio concreto
public class MyService : IMyService
{
    public void DoWork() {}
}

// Configuración
services.BindFromAssembly(
    assembly: Assembly.GetExecutingAssembly(),
    filter: type => type.Name.EndsWith("Service")
);
```

### Registro con Logging

```csharp
// Configuración con logging
services.BindFromAssembly(
    assembly: Assembly.GetExecutingAssembly(),
    logger: message =>
    {
        Console.WriteLine($"[DI] {message}");
    }
);
```

### Registro con Ciclo de Vida Específico

```csharp
// Configuración con ciclo de vida Singleton
services.BindFromAssembly(
    assembly: Assembly.GetExecutingAssembly(),
    lifetime: ServiceLifetime.Singleton
);
```

## Mejores Prácticas

1. **Nomenclatura**
   - Usar sufijos consistentes para los servicios (ej: `Service`, `Repository`)
   - Nombrar interfaces con prefijo `I` (ej: `IMyService`)

2. **Filtros**
   - Usar filtros para evitar registros innecesarios
   - Filtrar tipos de prueba o de desarrollo

3. **Logging**
   - Implementar logging para debugging
   - Usar diferentes niveles de logging

4. **Ciclos de Vida**
   - Usar `Scoped` para servicios por solicitud
   - Usar `Singleton` para servicios únicos
   - Usar `Transient` para servicios por instancia

## Soporte

Para reportar errores o solicitar características, por favor abre un issue en el repositorio de GitHub.

## Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.
