# Logging de Consultas SQL

Este módulo proporciona capacidades de registro (logging) para las consultas SQL ejecutadas a través del proveedor de SQL Server en DeveloperKit.

## Características

- Registro detallado de consultas SQL generadas
- Captura de parámetros de consulta
- Niveles de log configurables
- Soporte para operaciones síncronas y asíncronas
- Manejo de errores con registro de excepciones

## Configuración

### Configuración Básica

```csharp
// Configurar el logger por defecto (escribe en la salida de depuración)
QueryLogger.SetMinimumLevel(LogLevel.Debug);

// O configurar un logger personalizado
QueryLogger.SetLogger(new MiLoggerPersonalizado());
```

### Niveles de Log

- `Debug`: Información detallada para depuración
- `Information`: Información general sobre el funcionamiento de la aplicación
- `Warning`: Condiciones inesperadas que no detienen la ejecución
- `Error`: Errores que requieren atención inmediata

## Uso

### Métodos Principales

#### `QueryLogger.LogQuery`

```csharp
// Registrar una consulta manualmente
QueryLogger.LogQuery(
    sql: "SELECT * FROM Clientes WHERE Id = @Id",
    parameters: new Dictionary<string, object> { ["@Id"] = 42 },
    level: LogLevel.Debug,
    message: "Obteniendo cliente por ID"
);
```

#### `SetLogger`

```csharp
// Implementar un logger personalizado
public class MiLoggerPersonalizado : IQueryLogger
{
    public void LogQuery(string sql, IDictionary<string, object> parameters = null, 
        LogLevel level = LogLevel.Debug, string message = null)
    {
        // Implementación personalizada de logging
        Console.WriteLine($"[{level}] {message}\nSQL: {sql}");
    }
}

// Configurar el logger personalizado
QueryLogger.SetLogger(new MiLoggerPersonalizado());
```

## Integración con Query Builder

El logging está integrado automáticamente en los siguientes métodos:

- `ToList()` / `ToListAsync()`
- `FirstOrDefault()` / `FirstOrDefaultAsync()`
- `ExecuteNonQuery()` / `ExecuteNonQueryAsync()`
- `ExecuteScalar<T>()` / `ExecuteScalarAsync<T>()`

### Ejemplo de Uso con Query Builder

```csharp
// Ejemplo con logging automático
var clientes = await db.From<Cliente>()
    .Where(c => c.Activo && c.FechaRegistro > DateTime.Now.AddMonths(-1))
    .OrderBy(c => c.Nombre)
    .Take(5)
    .ToListAsync();
```

El ejemplo anterior generará automáticamente logs como:

```
[Debug] Iniciando consulta asíncrona ToList para Cliente
[Debug] Consulta asíncrona ToList completada. Se encontraron 3 registros de Cliente
```

## Personalización

### Implementación de Logger Personalizado

Puedes implementar tu propio logger creando una clase que implemente la interfaz `IQueryLogger`:

```csharp
public class MiLoggerPersonalizado : IQueryLogger
{
    public void LogQuery(string sql, IDictionary<string, object> parameters, 
        LogLevel level, string message)
    {
        // Implementa tu lógica de logging personalizada aquí
        // Por ejemplo, escribir en un archivo, enviar a un servicio externo, etc.
        
        var logMessage = new StringBuilder();
        logMessage.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}");
        logMessage.AppendLine("SQL:");
        logMessage.AppendLine(sql);
        
        if (parameters?.Count > 0)
        {
            logMessage.AppendLine("\nParámetros:");
            foreach (var param in parameters)
            {
                logMessage.AppendLine($"  {param.Key} = {param.Value} (Tipo: {param.Value?.GetType().Name ?? "null"})");
            }
        }
        
        // Escribir en un archivo de log
        File.AppendAllText("sql_queries.log", logMessage.ToString() + "\n\n");
    }
}

// Configurar el logger personalizado al inicio de la aplicación
QueryLogger.SetLogger(new MiLoggerPersonalizado());
```

## Mejores Prácticas

1. **Niveles de Log Apropiados**:
   - Usa `Debug` para información detallada de desarrollo
   - Usa `Information` para eventos importantes del flujo de la aplicación
   - Usa `Warning` para condiciones inesperadas que no impiden la ejecución
   - Usa `Error` solo para errores que requieren atención

2. **Rendimiento**:
   - Evita realizar operaciones costosas en los métodos de logging
   - Considera usar un sistema de cola para escribir logs de forma asíncrona

3. **Seguridad**:
   - No registres información sensible como contraseñas o datos personales
   - Considera ofuscar o enmascarar datos sensibles en los logs

## Solución de Problemas

### Los logs no aparecen

1. Verifica que el nivel de log no esté configurado demasiado alto:
   ```csharp
   QueryLogger.SetMinimumLevel(LogLevel.Debug);
   ```

2. Si estás en un entorno de producción, asegúrate de que el logger esté correctamente configurado y tenga permisos para escribir en el destino de log.

### Los parámetros no se muestran correctamente

Los parámetros se registran como pares clave-valor. Si no ves los valores esperados, verifica que:

1. Los parámetros se estén pasando correctamente al método de consulta
2. Los nombres de los parámetros coincidan exactamente con los usados en la consulta SQL

## Ejemplo Completo

```csharp
// Configuración al inicio de la aplicación
public void ConfigureServices(IServiceCollection services)
{
    // Configurar el logger personalizado
    QueryLogger.SetLogger(new MiLoggerPersonalizado());
    
    // O usar el logger por defecto con nivel Debug
    // QueryLogger.SetMinimumLevel(IQueryLogger.LogLevel.Debug);
    
    // Configurar el proveedor de SQL Server
    services.AddSQLServerProvider(options => 
    {
        options.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
    });
}

// Uso en una clase de servicio
public class ClienteService
{
    private readonly ISQLServerProvider _db;
    
    public ClienteService(ISQLServerProvider db)
    {
        _db = db;
    }
    
    public async Task<Cliente> ObtenerClienteActivoAsync(int id)
    {
        try
        {
            return await _db.From<Cliente>()
                .Where(c => c.Id == id && c.Activo)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            // El error ya se registró automáticamente
            throw new ApplicationException("Error al obtener el cliente", ex);
        }
    }
}
```
