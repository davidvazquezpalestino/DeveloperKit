# DevKit.ExecutionEngine.Redis

Servicio de caché distribuida utilizando Redis como backend, con soporte para generación automática de claves mediante expresiones lambda.

## Características

- Generación automática de claves a partir de expresiones lambda
- Detección automática de la clase origen para evitar colisiones
- Soporte para tipos complejos, genéricos y colecciones
- Invalidación selectiva de caché
- Configuración flexible de TTL y entorno
- Manejo robusto de valores nulos y tipos primitivos

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
public class RolUseCase(IRolRepository repository, ICacheService cache) : IRolPort
{
    public async Task<Rol> GetRolAsync(int rolID)
    {
        return await cache.GetOrSetAsync(() => repository.GetRolAsync(rolID));
    }

    public async Task<ICollection<RolPaginaConsulta>> GetRolPaginasAsync(int rolID)
    {
        return await cache.GetOrSetAsync(() => repository.GetRolPaginasAsync(rolID));
    }
}
```

### Invalidación de Caché

Invalida entradas específicas basadas en la misma expresión utilizada para el registro:

```csharp
await _cacheService.InvalidateAsync(() => _repositorio.ObtenerProductosConsutlaAsync());
```

## Generación de Claves

El sistema genera claves únicas automáticamente usando el formato:

```
{Environment}|{ClassName}:{MethodName}:{ReturnType}:{Parameters}
```

### Ejemplos de Claves Generadas

```csharp
// Para: RolUseCase.GetRolAsync(123)
"Development|RolUseCase:GetRolAsync:Rol:123"

// Para: RolUseCase.GetRolPaginasAsync(456)  
"Development|RolUseCase:GetRolPaginasAsync:ICollection|RolPaginaConsulta|:456"

// Para: UserService.GetUsersByStatus("active", 1)
"Development|UserService:GetUsersByStatus:ICollection|User|:active:1"
```

### Características de Generación de Claves

- **Detección automática de clase**: Identifica la clase que origina la llamada (ej: `RolUseCase`)
- **Manejo de genéricos**: Formatea tipos genéricos como `ICollection|RolPaginaConsulta|`
- **Tipos complejos**: Soporta DateTime, booleanos, diccionarios y colecciones
- **Valores nulos**: Maneja valores nulos de forma segura
- **Sin colisiones**: El diseño previene casi en su totalidad las colisiones de claves

### Formateo de Valores

| Tipo | Formato | Ejemplo |
|------|---------|---------|
| `string` | Directo | `"hello"` |
| `int` | Directo | `123` |
| `DateTime` | ISO 8601 | `"2024-03-06T10:30:00"` |
| `bool` | Numérico | `1` o `0` |
| `null` | Texto | `"NULL"` |
| `List<T>` | Corchetes | `"[1,2,3]"` |
| `Dictionary<K,V>` | Pares clave-valor | `"key1|value1,key2|value2"` |

## Configuración (RedisOptions)

- `ConnectionRedis`: Cadena de conexión (ej. "localhost:6379").
- `Environment`: Prefijo para las claves (evita conflictos entre entornos).
- `DiasCache`: TTL por defecto para las nuevas entradas.

## Mejoras Recientes

### Mejoras en Nomenclatura
- Renombrado `FetchAndStoreValueAsync` → `GetAndCacheValueAsync` para mayor claridad
- Mejora en documentación XML en español

### Mejoras en Generación de Claves
- **Detección de clase origen**: Las claves ahora comienzan con el nombre de la clase que realiza la llamada
- **Orden optimizado**: `ClassName:MethodName:ReturnType:Parameters`
- **Identificación de contexto**: Distingue entre clases concretas e interfaces de repositorio

### Mejoras en Rendimiento
- Patrones async/await optimizados con `ConfigureAwait(false)`
- Configuración flexible de timeouts
- Manejo mejorado de cancelación

## Arquitectura

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   RolUseCase    │───▶│   CacheService   │───▶│     Redis       │
│                 │    │                  │    │                 │
│ GetRolAsync()   │    │ GetOrSetAsync()  │    │ Key-Value Store│
│                 │    │                  │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
        │                       │
        ▼                       ▼
┌─────────────────┐    ┌──────────────────┐
│ IRolRepository  │    │ExpressionCondition│
│                 │    │Extractor         │
│ GetRolAsync()   │    │                  │
│                 │    │BuildRedisKey()   │
└─────────────────┘    └──────────────────┘