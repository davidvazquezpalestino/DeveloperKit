# CoreOracleDatabase

Biblioteca que proporciona una implementación robusta para la conexión y operaciones con bases de datos Oracle en aplicaciones .NET.

## Características

- Conexión segura a Oracle Database
- Manejo de transacciones
- Ejecución de consultas y procedimientos almacenados
- Soporte para parámetros
- Manejo de errores robusto
- Soporte para operaciones asíncronas
- Manejo de conexiones temporales
- Integración con inyección de dependencias

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
// Configurar la conexión
services.AddScoped<IOracleRepository, OracleRepository>(provider =>
{
    var connectionString = Configuration.GetConnectionString("OracleConnection");
    return new OracleRepository(connectionString);
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

// Ejecutar consulta
public async Task<DataTable> GetCustomers()
{
    var query = "SELECT * FROM CUSTOMERS WHERE ACTIVE = :active";
    
    return await _oracleRepository.ExecuteQueryAsTableAsync(query, parameters =>
    {
        parameters.Add("active", OracleDbType.Int32, 1, ParameterDirection.Input);
    });
}

// Ejecutar procedimiento almacenado
public async Task ExecuteStoredProcedure()
{
    var spName = "PKG_CUSTOMERS.GET_CUSTOMER";
    
    await _oracleRepository.ExecuteStoredProcedureAsync(spName, parameters =>
    {
        parameters.Add("p_customer_id", OracleDbType.Int32, 123, ParameterDirection.Input);
        parameters.Add("p_result", OracleDbType.Int32, ParameterDirection.Output);
    });
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
