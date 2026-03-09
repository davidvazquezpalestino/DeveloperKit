# DevKit.ExecutionEngine.SqlServer

Biblioteca de acceso a datos para SQL Server que proporciona una capa de abstracción robusta, segura y fácil de usar sobre ADO.NET, con soporte completo para operaciones síncronas y asíncronas. Incluye configuración moderna vía Options Pattern e inserciones masivas (Bulk).

## Características

- Conexión segura a SQL Server con soporte para autenticación integrada y SQL.
- Manejo de transacciones (commit/rollback).
- Ejecución de consultas SQL y procedimientos almacenados.
- Soporte completo para parámetros SQL con tipado fuerte.
- Manejo de errores robusto con mensajes descriptivos.
- Operaciones asíncronas para mejor rendimiento.
- Manejo automático de conexiones (abre/cierra según sea necesario).
- Integración con inyección de dependencias (`Microsoft.Extensions.DependencyInjection`).
- Binding automático de objetos a parámetros SQL para inserciones.
- Operaciones bulk para inserción masiva de datos (`SqlBulkCopy`).
- Soporte para múltiples formatos de retorno (DataTable, List<T>, Diccionarios, objetos fuertemente tipados).
- Creación y eliminación dinámica de tablas.
- Soporte para múltiples conjuntos de resultados.

## Instalación

Actualmente este proyecto forma parte del repositorio DeveloperKit. Si se publica como paquete NuGet, reemplaza este bloque con el nombre del paquete correspondiente.

## Requisitos

- SQL Server 2012 o superior.
- .NET 6.0 o superior (compatible con .NET Standard 2.1+).
- .NET Framework 4.8 (soporte heredado).
- Visual Studio 2022 o superior (recomendado).
- `Microsoft.Data.SqlClient`.

## Configuración (Options Pattern)

La configuración se realiza exclusivamente mediante Options Pattern. Usa los métodos de extensión en `DependencyContainer`:

```csharp
using DevKit.ExecutionEngine.SqlServer.Settings;
using DevKit.ExecutionEngine.Abstractions.Interfaces;

// Registro normal (scoped por defecto)
services.AddSQLServerProvider((provider, options) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    options.ConnectionString = configuration.GetConnectionString("SqlServer");
    options.CommandTimeout = 60; // segundos
    options.ConfigureApplication = () => "MiAplicacion";
    options.ConnectionPooling = new ConnectionPoolingOptions
    {
        MaxPoolSize = 200,
        MinPoolSize = 10,
        Pooling = true
    };
    options.BulkCopy = new BulkCopyOptions
    {
        BatchSize = 5_000,
        BulkCopyTimeout = 300,
        SqlBulkCopyOptions = SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.KeepIdentity
    };
});

// Registro keyed (si necesitas múltiples conexiones)
services.AddSQLServerProvider("Reporting", (provider, options) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    options.ConnectionString = configuration.GetConnectionString("ReportingDb");
}, ServiceLifetime.Scoped);
```

Inyecta `ISQLServerDatabaseProvider` donde lo necesites:

```csharp
public class CustomerService
{
    private readonly ISQLServerDatabaseProvider _provider;
    public CustomerService(ISQLServerDatabaseProvider provider) => _provider = provider;
}
```

### Ejemplo de appsettings.json

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=.;Database=MainDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "ReportingDb": "Server=.;Database=Reporting;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "SqlOptions": {
    "CommandTimeout": 60,
    "BulkCopy": {
      "BatchSize": 5000,
      "BulkCopyTimeout": 300,
      "SqlBulkCopyOptions": 17 // TableLock | KeepIdentity
    }
  }
}
```

### Registro leyendo de configuración

```csharp
// Program.cs / Startup.cs
services.AddSQLServerProvider((provider, options) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    options.ConnectionString = configuration.GetConnectionString("SqlServer");
    configuration.GetSection("SqlOptions").Bind(options);
});
```

### Cómo resolver proveedores keyed

```csharp
// .NET 8+ (minimal APIs / controllers)
public class ReportsController : ControllerBase
{
    private readonly ISQLServerDatabaseProvider _reportingProvider;

    public ReportsController([FromKeyedServices("Reporting")] ISQLServerDatabaseProvider reportingProvider)
    {
        _reportingProvider = reportingProvider;
    }
}

// Alternativa genérica (cualquier versión con Microsoft.Extensions.DependencyInjection 8+)
public class ReportService
{
    private readonly ISQLServerDatabaseProvider _reportingProvider;

    public ReportService(IKeyedServiceProvider provider)
    {
        _reportingProvider = provider.GetRequiredKeyedService<ISQLServerDatabaseProvider>("Reporting");
    }
}
```

## Ejemplos de Uso

### Consultas básicas

```csharp
// Inyección del proveedor
private readonly ISQLServerDatabaseProvider _provider;

public CustomerService(ISQLServerDatabaseProvider provider)
{
    _provider = provider;
}

// Ejecutar consulta y obtener DataTable
public async Task<DataTable> GetActiveCustomersAsync()
{
    var query = "SELECT * FROM CUSTOMERS WHERE ACTIVE = @active";
    
    return await _provider.ExecuteQueryAsTableAsync(query, parameters =>
    {
        parameters.AddPosgreParameter("active", true);
    });
}

// Ejecutar consulta y mapear a objetos fuertemente tipados
public async Task<List<Customer>> GetCustomersAsync()
{
    var query = "SELECT CustomerId, Name, Email, CreatedDate FROM CUSTOMERS";
    
    return await _provider.ExecuteQueryAsListAsync(query, reader => new Customer
    {
        CustomerId = reader.GetInt32(0),
        Name = reader.GetString(1),
        Email = reader.IsDBNull(2) ? null : reader.GetString(2),
        CreatedDate = reader.GetDateTime(3)
    });
}
```

### Procedimientos Almacenados

```csharp
// Ejecutar procedimiento almacenado con parámetros
public async Task<Customer> GetCustomerByIdAsync(int customerId)
{
    var spName = "usp_GetCustomerById";
    
    return await _provider.ExecuteProcedureAsSingleAsync(spName, reader => new Customer
    {
        CustomerId = reader.GetInt32("CustomerId"),
        Name = reader.GetString("Name")
    }, parameters =>
    {
        parameters.AddPosgreParameter("CustomerId", customerId);
    });
}
```

### Operaciones CRUD

```csharp
// Insertar una entidad (asíncrono, compacto)
public async Task InsertCustomerAsync(Customer customer)
{
    await _provider.ExecuteInsertAsync("Customers", customer);
}

// Insertar varias entidades (asíncrono, compacto)
public async Task InsertCustomersAsync(ICollection<Customer> customers)
{
    await _provider.ExecuteInsertAsync("Customers", customers);
}

// Actualizar un registro
public async Task<bool> UpdateCustomerAsync(Customer customer)
{
    var query = "UPDATE Customers SET Name = @Name WHERE CustomerId = @CustomerId";
    
    int rowsAffected = await _provider.ExecuteNonQueryAsync(query, parameters =>
    {
        parameters.AddPosgreParameter("CustomerId", customer.CustomerId);
        parameters.AddPosgreParameter("Name", customer.Name);
    });
    
    return rowsAffected > 0;
}
```

### Operaciones con Transacciones

```csharp
public async Task<bool> ProcessOrderAsync(Order order, List<OrderItem> items)
{
    try
    {
        _provider.BeginTransaction();
        
        // ... operaciones de base de datos ...
        
        _provider.CommitTransaction();
        return true;
    }
    catch (Exception ex)
    {
        _provider.RollbackTransaction();
        // Loggear el error
        return false;
    }
}
```

### Inserción Masiva (Bulk Insert)

```csharp
// Inserción masiva desde DataTable
public async Task BulkInsertCustomersAsync(DataTable customersData)
{
    await _provider.ExecuteBulkInsertToTableAsync(customersData, "Customers");
}

// Inserción masiva con configuración avanzada
public async Task BulkInsertWithAdvancedOptionsAsync(DataTable data)
{
    await _provider.ExecuteBulkInsertToTableAsync(data, "Customers", options =>
    {
        options.BatchSize = 10000;
        options.BulkCopyTimeout = 600;
        options.SqlBulkCopyOptions = SqlBulkCopyOptions.TableLock;
    });
}
```    await _sqlRepository.ExecuteBulkCopyAsync(customers, builder => builder
        .ToTable("Customers")
        .WithBatchSize(5000)
        .WithTimeout(300)
        .WithOptions(SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.CheckConstraints)
        .MapColumn("CustomerId", "CustomerId")
        .MapColumn("Name", "Name")
        .MapColumn("Email", "Email")
        .MapColumn("CreatedDate", "CreatedDate")
    );
}
```

### Creación Dinámica de Tablas

```csharp
// Crear tabla desde DataTable
public void CreateTableFromData(DataTable sourceData, string destinationTableName)
{
    _sqlRepository.CreateTable(sourceData, destinationTableName);
}

// Crear tabla desde IDataReader
public void CreateTableFromReader(IDataReader reader, string destinationTableName)
{
    _sqlRepository.CreateTable(reader, destinationTableName);
}

// Eliminar tabla
public void DropCustomerTable()
{
    _sqlRepository.DropTable("Customers");
}
```

### Manejo de Tipos de Datos

La biblioteca soporta todos los tipos de datos de SQL Server, incluyendo tipos especiales como:

- Tipos numéricos: INT, BIGINT, DECIMAL, FLOAT, etc.
- Tipos de texto: VARCHAR, NVARCHAR, CHAR, NCHAR, TEXT, NTEXT
- Tipos de fecha: DATETIME, DATETIME2, DATE, TIME, DATETIMEOFFSET
- Tipos binarios: VARBINARY, IMAGE
- Tipos especiales: UNIQUEIDENTIFIER, XML, JSON, GEOMETRY, GEOGRAPHY

## Manejo de errores

```csharp
try
{
    // Código que interactúa con la base de datos
}
catch (SqlException sqlEx) when (sqlEx.Number == 2627) // Violación de restricción única
{
    throw new DuplicateEntryException("Ya existe un registro con la misma clave", sqlEx);
}
catch (SqlException sqlEx) when (sqlEx.Number == 547) // Violación de clave foránea
{
    throw new InvalidOperationException("No se puede eliminar el registro porque tiene registros relacionados", sqlEx);
}
catch (SqlException sqlEx) when (sqlEx.Number == 1205) // Deadlock
{
    // Reintentar la operación
    await Task.Delay(500);
    await ExecuteOperationWithRetry();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error al acceder a la base de datos");
    throw new DatabaseOperationException("Error al procesar la operación", ex);
}
```

## Mejores prácticas

1. **Seguridad**
   - Siempre usar parámetros en las consultas para evitar inyección SQL
   - No concatenar valores directamente en las consultas SQL
   - Usar los permisos mínimos necesarios para la conexión a la base de datos
   - Validar y limpiar todos los datos de entrada

2. **Rendimiento**
   - Usar operaciones asíncronas para operaciones de E/S
   - Implementar paginación para consultas que devuelven grandes conjuntos de datos
   - Usar operaciones bulk para inserciones/actualizaciones masivas
   - Mantener las transacciones lo más cortas posibles

3. **Mantenibilidad**
   - Usar procedimientos almacenados para lógica de negocio compleja
   - Implementar un patrón de repositorio para centralizar el acceso a datos
   - Usar DTOs para transferir datos entre capas
   - Documentar consultas complejas

4. **Manejo de Errores**
   - Implementar un manejo de errores consistente
   - Registrar errores con suficiente contexto para diagnóstico
   - Proporcionar mensajes de error amigables al usuario final
   - Manejar adecuadamente las transacciones fallidas

5. **Patrones de Diseño**
   - Usar el patrón Unit of Work para operaciones atómicas
   - Implementar el patrón Repository para abstraer el acceso a datos
   - Usar el patrón Specification para construir consultas complejas
   - Aplicar el principio de responsabilidad única

## Manejo de Errores

```csharp
try
{
    await _sqlRepository.ExecuteQueryAsync(query);
}
catch (SqlException ex)
{
    // Manejo específico de errores de SQL
    if (ex.Number == 2627) // Violación de restricción única
    {
        throw new DuplicateEntryException("Ya existe un registro con esta clave");
    }
    throw;
}
```

## Optimización de Consultas

1. Usar parámetros en lugar de concatenación de strings
2. Implementar timeout razonable
3. Usar índices apropiados
4. Evitar SELECT * cuando no es necesario
5. Usar procedimientos almacenados para consultas complejas
6. Implementar paginación para listados grandes
7. Usar transacciones solo cuando sea necesario

## Mapa de métodos públicos y ubicación

Resumen rápido de métodos principales y dónde están implementados.

### Sincrónicos — `Implementations/MSSqlRepository.cs`

- Conexión y transacciones: `ConnectionString`, `ConnectionState`, `ConnectionClose()`, `BeginTransaction()`, `CommitTransaction()`, `RollbackTransaction()`
- Consultas: `ExecuteQueryAsTable(...)`, `ExecuteProcedureAsTable(...)`
- Diccionarios: `ExecuteProcedureAsDictionary(...)`
- Inserts compactos: `ExecuteInsert<T>(string table, T entity)`, `ExecuteInsert<T>(string table, ICollection<T> entities)`

### Asíncronos — `Implementations/MSSqlRepository.Async.cs`

- Consultas: `ExecuteQueryAsTableAsync(...)`, `ExecuteProcedureAsTableAsync(...)`
- Diccionarios: `ExecuteQueryAsDictionaryAsync(...)`, `ExecuteProcedureAsDictionaryAsync(...)`
- Proyección única/listas: `ExecuteQueryAsSingleAsync<T>(...)`, `ExecuteProcedureAsSingleAsync<T>(...)`, `ExecuteQueryAsListAsync<T>(...)`, `ExecuteProcedureAsListAsync<T>(...)`
- Múltiples conjuntos de resultados: `ExecuteMultiResultQueryAsync(...)`
- Comandos: `ExecuteProcedureCommandAsync(...)`, `ExecuteNonQueryAsync(...)`
- Inserts compactos: `ExecuteInsertAsync<T>(string table, T entity)`, `ExecuteInsertAsync<T>(string table, ICollection<T> entities)`
- Bulk Copy: `ExecuteBulkCopyToTableAsync(...)`, `ExecuteBulkCopyAsync(DataTable, ...)`, `ExecuteBulkCopyAsync(IDataReader, ...)`, `ExecuteBulkCopyAsync<T>(IEnumerable<T>, ...)`
- Utilidades: `GetCurrentDateTimeAsync()`

Notas:
- Los métodos async usan `ConfigureAwait(false)` por ser librería.
- Las operaciones Bulk aceptan `BulkOperationsConfiguration` o builder fluido.

## Soporte

Para reportar errores o solicitar características, por favor abre un issue en el repositorio de GitHub.

## Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.

- `void CommitTransaction()`  
  Confirma la transacción activa y cierra la conexión.

- `void RollbackTransaction()`  
  Revierte la transacción activa y cierra la conexión.

### Consultas y ejecución

- `DataTable GetTableFromQuery(string query, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta una consulta SQL y devuelve un `DataTable` con los resultados.

- `DataTable GetTableFromStoredProcedure(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta un procedimiento almacenado y devuelve un `DataTable`.

- `T GetItemFromQuery<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta una consulta y transforma la primera fila a un objeto del tipo `T`.

- `T GetItemFromStoredProcedure<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)`  
  Igual que el anterior, pero con procedimiento almacenado.

- `ICollection<Dictionary<string, object>> GetDictionaryFromQuery(string query, Action<IDataParameterCollection> parametros = null)`  
  Obtiene resultados de consulta como una colección de diccionarios con nombre/valor.

- `ICollection<Dictionary<string, object>> GetDictionaryFromStoredProcedure(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)`  
  Igual que el anterior, pero para procedimientos almacenados.

- `ICollection<T> GetItemsFromStoredProcedure<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)`  
  Obtiene una colección de objetos tipo `T` a partir de un procedimiento almacenado.

- `ICollection<T> GetItemsFromQuery<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)`  
  Obtiene una colección de objetos tipo `T` a partir de una consulta SQL.

### Ejecución de comandos

- `void ExecuteNonQuery(string command, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta comandos SQL que no retornan datos (INSERT, UPDATE, DELETE, etc.).

- `void ExecuteInsert<T>(string tableName, T entity) where T : class, new()`  
  Inserta un único objeto en la tabla especificada usando reflexión.

- `void ExecuteInsert<T>(string tableName, ICollection<T> collection) where T : class, new()`  
  Inserta una colección de objetos en la tabla especificada.

- `void ExecuteStoredProcedureCommand(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta un procedimiento almacenado que no devuelve resultados.

### Operaciones Bulk Copy

- `void ExecuteBulkCopyToTable(DataTable source, string destinationTable)`  
  Borra la tabla destino, la crea según la estructura de la tabla fuente y realiza la inserción masiva.

- `void ExecuteBulkCopy(DataTable source, string destinationTable)`  
  Inserción masiva desde un `DataTable`.

- `void ExecuteBulkCopy(IDataReader source, string destinationTable)`  
  Inserción masiva desde un `IDataReader`.

### Otros métodos útiles

- `DateTime GetCurrentDateTime()`  
  Obtiene la fecha y hora actual del servidor SQL.

- Métodos para crear parámetros SQL para consultas y procedimientos almacenados (`AddParameter`).

- Métodos para configurar la conexión (`SetDatabaseLogon`).

## Métodos Asíncronos de IMSSqlRepository

La interfaz también define métodos asíncronos para mejorar el rendimiento y escalabilidad de las operaciones de base de datos.

### Métodos asíncronos principales

- `Task<T> GetItemFromQueryAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta una consulta SQL asíncrona y transforma la primera fila en un objeto `T`.

- `Task<T> GetItemFromStoredProcedureAsync<T>(string procedimientoAlmacenado) where T : new()`  
  Ejecuta un procedimiento almacenado de forma asíncrona y devuelve un objeto `T` por defecto.

- `Task<T> GetItemFromStoredProcedureAsync<T>(string procedimientoAlmacenado, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta un procedimiento almacenado de forma asíncrona y mapea la primera fila a un objeto `T`.

- `Task<DataTable> GetTableFromQueryAsync(string query, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta una consulta asíncrona y devuelve un `DataTable` con los resultados.

- `Task<DataTable> GetTableFromStoredProcedureAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta un procedimiento almacenado de forma asíncrona y devuelve un `DataTable`.

- `Task<ICollection<Dictionary<string, object>>> GetDictionaryFromQueryAsync(string query, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta una consulta asíncrona y devuelve una colección de diccionarios.

- `Task<ICollection<Dictionary<string, object>>> GetDictionaryFromStoredProcedureAsync(string procedimientoAlmacenado, Action<IDataParameterCollection> parametros = null)`  
  Igual que el anterior, pero para procedimientos almacenados.

- `Task<ICollection<T>> GetItemsFromQueryAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)`  
  Obtiene una colección de objetos `T` de una consulta asíncrona.

- `Task<ICollection<T>> GetItemsFromStoredProcedureAsync<T>(string storedProcedure) where T : new()`  
  Obtiene una colección de objetos `T` de un procedimiento almacenado asíncrono.

- `Task<ICollection<T>> GetItemsFromStoredProcedureAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> parametros = null)`  
  Igual que el anterior, pero con mapeo personalizado.

### Comandos asíncronos

- `Task<int> ExecuteStoredProcedureCommandAsync(string storedProcedure, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta un procedimiento almacenado asíncrono que devuelve el número de filas afectadas.

- `Task ExecuteNonQueryAsync(string command, Action<IDataParameterCollection> parametros = null)`  
  Ejecuta un comando SQL que no retorna datos de forma asíncrona.

- `Task ExecuteBulkCopyToTableAsync(DataTable source, string destinationTable)`  
  Realiza una inserción masiva asíncrona creando la tabla destino.

- `Task ExecuteBulkCopyAsync(DataTable source, string destinationTable)`  
  Inserción masiva asíncrona desde un `DataTable`.

- `Task ExecuteBulkCopyAsync(IDataReader source, string destinationTable)`  
  Inserción masiva asíncrona desde un `IDataReader`.

- `Task ExecuteInsertAsync<T>(string tableName, T entity)`  
  Inserta un objeto de forma asíncrona en la tabla especificada.

- `Task ExecuteInsertAsync<T>(string tableName, ICollection<T> collection)`  
  Inserta una colección de objetos de forma asíncrona.

- `Task<DateTime> GetCurrentDateTimeAsync()`  
  Obtiene la fecha y hora actual del servidor SQL de forma asíncrona.

## Métodos para manejo de tablas en IMSSqlRepository

Además de los métodos para manipulación de datos, la interfaz incluye operaciones para gestionar tablas directamente:

- `void DropTable(string tableName)`  
  Elimina la tabla especificada de la base de datos.

- `void CreateTable(DataTable source, string destinationTable)`  
  Crea una tabla en la base de datos a partir de la estructura de un `DataTable`.

- `void CreateTable(IDataReader reader, string destinationTable)`  
  Crea una tabla en la base de datos basándose en la estructura del `IDataReader`.

## Interfaz IServiceProviderKeyed

Esta interfaz proporciona un mecanismo para obtener servicios basados en una clave específica:

```csharp
public interface IServiceProviderKeyed
{
    TService GetKeyedService<TService, TKeyed>(TKeyed key);
}

public void ConfigureServices(IServiceCollection services)
{
    services.AddMicrosoftSQL(provider =>
    {
        // Aquí configuras la instancia concreta de IMSSqlRepository
        IMSSqlRepository repo = new MSSqlRepository();
        repo.SetDatabaseLogon("Server=myServer;Database=myDB;User Id=myUser;Password=myPassword;");
        return repo;
    }, ServiceLifetime.Scoped);
}
