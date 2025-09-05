# CoreOracleDatabase

Biblioteca que proporciona una implementación robusta para la conexión y operaciones con bases de datos Oracle en aplicaciones .NET.

## Características

- Conexión segura a Oracle Database
- Manejo de transacciones
- Ejecución de consultas y procedimientos almacenados
- Soporte para parámetros
- Manejo de errores robusto
- Soporte completo para operaciones asíncronas con CancellationToken
- Configuración avanzada de timeouts y bulk operations
- Manejo de conexiones temporales
- Integración con inyección de dependencias
- Optimizaciones de rendimiento con ConfigureAwait(false)

## Instalación

El componente se puede instalar como un paquete NuGet:

```bash
dotnet add package DeveloperKit.CoreOracleDatabase
```

## Requisitos

- Oracle Database 11g o superior
- Oracle Data Provider for .NET (ODP.NET)
- .NET Framework 4.8 o superior
- Visual Studio 2019 o superior

## Uso Básico

### Configuración Inicial

```csharp
// Configuración básica
services.AddScoped<IOracleRepository, OracleRepository>(provider =>
{
    var connectionString = Configuration.GetConnectionString("OracleConnection");
    return new OracleRepository(connectionString);
});

// Configuración avanzada con opciones personalizadas
services.Configure<OracleOptions>(options =>
{
    options.CommandTimeout = 60; // Timeout en segundos para comandos
    options.BulkCopyTimeout = 300; // Timeout para operaciones bulk
    options.ConnectionPooling = true;
    options.MaxPoolSize = 100;
    options.MinPoolSize = 5;
});
```

### Ejemplo de Uso

```csharp
// Inyección del repositorio
private readonly IOracleRepository _oracleRepository;

public MyService(IOracleRepository oracleRepository)
{
    _oracleRepository = oracleRepository;
}

// Ejecutar consulta con soporte de cancelación
public async Task<DataTable> GetCustomers(CancellationToken cancellationToken = default)
{
    var query = "SELECT * FROM CUSTOMERS WHERE ACTIVE = :active";
    
    return await _oracleRepository.ExecuteQueryAsTableAsync(query, parameters =>
    {
        parameters.Add("active", OracleDbType.Int32, 1, ParameterDirection.Input);
    }, cancellationToken);
}

// Ejecutar procedimiento almacenado con soporte de cancelación
public async Task ExecuteStoredProcedure(CancellationToken cancellationToken = default)
{
    var spName = "PKG_CUSTOMERS.GET_CUSTOMER";
    
    await _oracleRepository.ExecuteStoredProcedureAsync(spName, parameters =>
    {
        parameters.Add("p_customer_id", OracleDbType.Int32, 123, ParameterDirection.Input);
        parameters.Add("p_result", OracleDbType.Int32, ParameterDirection.Output);
    }, cancellationToken);
}
```

### Manejo de Transacciones

```csharp
public async Task ExecuteWithTransaction()
{
    try
    {
        // Iniciar transacción
        _oracleRepository.BeginTransaction();
        
        // Ejecutar operaciones
        await _oracleRepository.ExecuteQueryAsync("INSERT INTO CUSTOMERS (...) VALUES (...)"));
        await _oracleRepository.ExecuteQueryAsync("UPDATE CUSTOMERS SET (...) WHERE (...)"));
        
        // Confirmar transacción
        _oracleRepository.CommitTransaction();
    }
    catch (Exception ex)
    {
        // Revertir transacción en caso de error
        _oracleRepository.RollbackTransaction();
        throw;
    }
}
```

### Manejo de Parámetros

```csharp
// Parámetros de entrada
parameters.Add("p_input", OracleDbType.Varchar2, "valor", ParameterDirection.Input);

// Parámetros de salida
parameters.Add("p_output", OracleDbType.Varchar2, ParameterDirection.Output);

// Parámetros de entrada/salida
parameters.Add("p_inout", OracleDbType.Int32, 1, ParameterDirection.InputOutput);
```

## Mejores Prácticas

1. Siempre usar parámetros en las consultas para evitar inyección SQL
2. Manejar transacciones para operaciones que afectan múltiples tablas
3. Usar conexiones asíncronas para operaciones largas
4. Implementar manejo de errores robusto
5. Usar procedimientos almacenados cuando sea posible
6. Implementar timeout adecuado para consultas
7. Usar pool de conexiones para mejor rendimiento

## Soporte

Para reportar errores o solicitar características, por favor abre un issue en el repositorio de GitHub.

## Licencia

Este proyecto está bajo la licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.

## Autor

**David Vázquez Palestino**

- Desarrollador de software con experiencia en .NET y Oracle Database
- Creador y mantenedor de CoreOracleDatabase

## Agradecimientos

- A la comunidad de desarrolladores .NET por su apoyo y contribuciones
- A todos los contribuyentes que han ayudado a mejorar este proyecto
