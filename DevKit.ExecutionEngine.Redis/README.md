# DevKit.ExecutionEngine.Redis

Servicio de caché distribuida utilizando Redis como backend, con soporte para generación automática de claves mediante expresiones lambda.

## Instalación

1. Configura la sección `RedisOptions` en tu `appsettings.json`:

```json
{
  "RedisOptions": {
    "ConnectionRedis": "localhost:6379",
    "Environment": "Development",
    "DiasCache": 7
  }
}
```

2. Registra el servicio en el contenedor de dependencias:

```csharp
services.AddRedisCache(ServiceLifetime.Scoped);
```

## Uso

### Inyección y Consumo

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
        // Obtiene del caché o ejecuta la función si no existe
        return await _cacheService.GetOrSetAsync(() => _repositorio.ObtenerProductosConsutlaAsync());
    }
}
```

### Invalidación de Caché

Invalida entradas específicas basadas en la misma expresión utilizada para el registro:

```csharp
await _cacheService.InvalidateCacheAsync(() => _repositorio.ObtenerProductosConsutlaAsync());
```

## Configuración (RedisOptions)

- `ConnectionRedis`: Cadena de conexión (ej. "localhost:6379").
- `Environment`: Prefijo para las claves (evita conflictos entre entornos).
- `DiasCache`: TTL por defecto para las nuevas entradas.