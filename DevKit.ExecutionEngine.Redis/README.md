# DevKit.ExecutionEngine.Redis

Este proyecto proporciona una implementación de servicio de caché utilizando Redis como backend. Está diseñado para integrar fácilmente el almacenamiento en caché en aplicaciones .NET, permitiendo mejorar el rendimiento al reducir las consultas repetitivas a bases de datos u otros servicios.

## Características Principales

- **Caché Asíncrono**: Soporte completo para operaciones asíncronas.
- **Invalidación Inteligente**: Permite invalidar entradas de caché basadas en expresiones lambda.
- **Configuración Flexible**: Opciones configurables para conexión, TTL y entorno.
- **Integración con LINQ**: Utiliza expresiones lambda para generar claves de caché de manera automática.

## Instalación

1. Agrega el paquete NuGet correspondiente al proyecto.
2. Configura las opciones de Redis en tu archivo `appsettings.json`:

```json
{
  "RedisOptions": {
    "ConnectionRedis": "localhost:6379",
    "Environment": "Development",
    "DiasCache": 7
  }
}
```

3. Registra el servicio en el contenedor de dependencias:

```csharp
services.AddSingleton<ICacheService, CacheService>();
```

## Uso Básico

### Inyección de Dependencias

```csharp
public class MiServicio
{
    private readonly ICacheService _cacheService;

    public MiServicio(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<List<Producto>> ObtenerProductosAsync()
    {
        return await _cacheService.GetOrSetAsync(() => _repositorio.ObtenerProductosAsync());
    }
}
```

### Invalidación de Caché

```csharp
await _cacheService.InvalidateCacheAsync(() => _repositorio.ObtenerProductosAsync());
```

## Clases Principales

- **ICacheService**: Interfaz que define los métodos del servicio de caché.
- **CacheService**: Implementación concreta del servicio de caché.
- **RedisOptions**: Clase de configuración para Redis.
- **ExpressionConditionExtractor**: Utilidad para extraer condiciones de expresiones lambda y construir claves de Redis.

## Configuración

La configuración se realiza a través de la clase `RedisOptions`:

- `ConnectionRedis`: Cadena de conexión a Redis (ej. "localhost:6379").
- `Environment`: Nombre del entorno, usado como prefijo en las claves.
- `DiasCache`: Número de días que dura el caché por defecto.

## Dependencias

- StackExchange.Redis
- Microsoft.Extensions.Options
- System.Text.Json

## Notas

- Asegúrate de que Redis esté ejecutándose y accesible.
- Las claves de caché incluyen el nombre del entorno para evitar conflictos entre entornos.
- El TTL se calcula en días, pero puede ser ajustado según necesidades.

Para más detalles, consulta la documentación completa en `Documentation.md`.