# SQL Server Database Provider for DeveloperKit

[![NuGet](https://img.shields.io/nuget/v/DevKit.ExecutionEngine.SqlServer.svg?style=flat-square)](https://www.nuget.org/packages/DevKit.ExecutionEngine.SqlServer/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-blue)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2012%2B-red)](https://www.microsoft.com/sql-server/)

High-performance SQL Server database provider for .NET applications, built on top of ADO.NET with a clean, consistent API for both synchronous and asynchronous operations.

## ✨ Features

- **Unified API**: Consistent method naming across all database operations
- **Full Async Support**: All operations have async/await variants
- **Bulk Operations**: High-performance bulk inserts with `SqlBulkCopy`
- **Transaction Support**: Comprehensive transaction management
- **Stored Procedures**: Full support for executing stored procedures
- **Type Safety**: Strongly-typed result mapping
- **Dependency Injection**: First-class support for .NET Core DI
- **Configurable**: Fine-grained control over connection and command behavior
- **Multiple Result Sets**: Support for queries returning multiple result sets
- **Dynamic Table Operations**: Runtime table creation and management

## 🚀 Getting Started

### Prerequisites

- .NET 6.0+ or .NET Standard 2.1+
- SQL Server 2012 or later
- Microsoft.Data.SqlClient NuGet package (automatically referenced)

### Installation

```bash
dotnet add package DevKit.ExecutionEngine.SqlServer
```

## 🛠 Configuration

### Basic Setup

```csharp
using DevKit.ExecutionEngine.SqlServer;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Basic configuration
services.AddSQLServerProvider(options =>
{
    options.ConnectionString = "Server=localhost;Database=myapp;User ID=user;Password=password;";
    options.CommandTimeout = 30; // seconds
});
```

### Advanced Configuration

```csharp
services.AddSQLServerProvider((provider, options) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    
    options.ConnectionString = configuration.GetConnectionString("SqlServer");
    options.CommandTimeout = 30;
    options.ApplicationName = "MyApplication";
    
    // Connection pooling
    options.ConnectionPooling = new ConnectionPoolingOptions 
    { 
        MaxPoolSize = 200,
        MinPoolSize = 10,
        ConnectionLifetime = 300 // seconds
    };
    
    // Bulk copy options
    options.BulkCopy = new BulkCopyOptions 
    { 
        BatchSize = 1000,
        BulkCopyTimeout = 0, // no timeout
        EnableStreaming = true,
        UseInternalTransaction = false
    };
});
```

## 💻 Usage Examples

### Basic Query

```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Using dependency injection
public class CustomerService
{
    private readonly ISQLServerDatabaseProvider _db;
    
    public CustomerService(ISQLServerDatabaseProvider db)
    {
        _db = db;
    }
    
    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
    {
        return await _db.ExecuteQueryAsListAsync<Customer>(
            "SELECT * FROM customers WHERE is_active = @isActive",
            parameters: new { isActive = true });
    }
}
```

### Parameterized Queries

```csharp
// Using anonymous types for parameters
var customers = await _db.ExecuteQueryAsListAsync<Customer>(
    "SELECT * FROM customers WHERE created_at > @minDate AND status = @status",
    parameters: new { minDate = DateTime.UtcNow.AddDays(-30), status = "active" });

// Using dictionary for parameters
var parameters = new Dictionary<string, object>
{
    ["minDate"] = DateTime.UtcNow.AddDays(-30),
    ["status"] = "active"
};

var customers = await _db.ExecuteQueryAsListAsync<Customer>(
    "SELECT * FROM customers WHERE created_at > @minDate AND status = @status",
    parameters: parameters);
```

### Stored Procedures

```csharp
// Executing a stored procedure
var result = await _db.ExecuteStoredProcedureAsListAsync<CustomerReport>(
    "sp_GetCustomerReport",
    parameters: new 
    {
        startDate = new DateTime(2023, 1, 1),
        endDate = DateTime.Now,
        status = "active"
    });
```

### Bulk Operations

```csharp
// Bulk insert from DataTable
DataTable customerData = GetCustomerData();
await _db.ExecuteBulkInsertAsync(customerData, "customers");

// Bulk insert from list of objects
List<Customer> customers = GetCustomers();
await _db.ExecuteBulkInsertToTableAsync(customers, "customers");
```

## 📚 API Reference

### Key Methods

| Method | Description |
|--------|-------------|
| `ExecuteQueryAsList<T>` | Executes a query and maps results to a list of strongly-typed objects |
| `ExecuteQueryAsTable` | Executes a query and returns results as a DataTable |
| `ExecuteNonQuery` | Executes a command and returns the number of rows affected |
| `ExecuteScalar<T>` | Executes a command and returns the first column of the first row |
| `ExecuteStoredProcedure*` | Methods for executing stored procedures |
| `ExecuteBulkInsert*` | Methods for bulk data operations |
| `CreateTable` | Creates a table at runtime |
| `TableExists` | Checks if a table exists |

### Async Variants

All methods have async counterparts with the `Async` suffix (e.g., `ExecuteQueryAsListAsync`).

## ⚙️ Advanced Topics

### Transaction Management

```csharp
using (var transaction = await _db.BeginTransactionAsync())
try
{
    // Perform multiple operations within the same transaction
    await _db.ExecuteNonQueryAsync(
        "UPDATE accounts SET balance = balance - @amount WHERE id = @id", 
        parameters: new { id = 1, amount = 100 });
        
    await _db.ExecuteNonQueryAsync(
        "UPDATE accounts SET balance = balance + @amount WHERE id = @id", 
        parameters: new { id = 2, amount = 100 });
        
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Dynamic Table Operations

```csharp
// Create a table dynamically
await _db.CreateTableAsync("DynamicTable", new[]
{
    new ColumnDefinition("Id", "INT", isPrimaryKey: true, isIdentity: true),
    new ColumnDefinition("Name", "NVARCHAR(100)", isNullable: false),
    new ColumnDefinition("CreatedAt", "DATETIME", defaultValue: "GETDATE()")
});

// Check if table exists
bool exists = await _db.TableExistsAsync("DynamicTable");
```

## 📊 Performance Tips

1. **Use Connection Pooling**: Enabled by default with sensible defaults
2. **Batch Operations**: Use bulk operations for large data imports/exports
3. **Parameterized Queries**: Always use parameters to prevent SQL injection and improve query plan caching
4. **Async/Await**: Use async methods for I/O-bound operations to improve scalability
5. **Connection Management**: Let the provider manage connections unless you have specific requirements

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please read our [contributing guidelines](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## 📫 Support

For support, please open an issue in our [issue tracker](https://github.com/davidvazquezpalestino/DeveloperKit/issues).

---

<div align="center">
  Made with ❤️ by the DeveloperKit Team
</div>
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
        parameters.AddWithValue("@active", true);
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
        parameters.AddWithValue("@CustomerId", customerId);
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
        parameters.AddWithValue("@CustomerId", customer.CustomerId);
        parameters.AddWithValue("@Name", customer.Name);
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

## 🔍 Constructor de Consultas SQL Server (SqlQueryBuilderSimple)

El módulo `SqlQueryBuilderSimple` proporciona una forma fluida y fuertemente tipada para construir y ejecutar consultas SQL Server de manera segura y eficiente.

### Características Principales

- **Consulta Fuertemente Tipada**: Usa expresiones lambda para filtros seguros en tiempo de compilación
- **Fluent API**: Permite encadenar métodos para construir consultas complejas de manera legible
- **Soporte para Operaciones CRUD**: Incluye métodos para SELECT, INSERT, UPDATE, DELETE
- **Paginación**: Métodos integrados para paginación de resultados
- **Mapeo Automático**: Convierte automáticamente los resultados a objetos fuertemente tipados
- **Async/Await**: Todos los métodos tienen versiones asíncronas

### Uso Básico

#### Configuración Inicial

```csharp
using DevKit.ExecutionEngine.SqlServer.Extensions;

// Obtener el proveedor de base de datos (normalmente inyectado por DI)
ISQLServerDatabaseProvider dbProvider = ...;
```

#### Consultas SELECT

```csharp
// Consulta simple con filtro
var clientes = dbProvider
    .From<Cliente>()
    .Where(c => c.Activo && c.FechaRegistro > DateTime.Now.AddMonths(-1))
    .ToList();

// Con ordenamiento y paginación
var clientesPaginados = await dbProvider
    .From<Cliente>()
    .Where(c => c.Pais == "México")
    .OrderBy(c => c.Nombre)
    .Skip(10)
    .Take(5)
    .ToListAsync();

// Consulta con proyección
var nombresClientes = await dbProvider
    .From<Cliente>()
    .Select(c => new { c.Id, c.Nombre })
    .Where(c => c.Nombre.StartsWith("A"))
    .ToListAsync();
```

#### Inserción de Datos

```csharp
// Insertar un solo registro
var nuevoCliente = new Cliente 
{ 
    Nombre = "Juan Pérez", 
    Email = "juan@example.com",
    FechaRegistro = DateTime.Now,
    Activo = true
};

int id = await dbProvider
    .From<Cliente>()
    .InsertAsync(nuevoCliente);

// Insertar múltiples registros
var nuevosClientes = new List<Cliente> { /* ... */ };
int registrosAfectados = await dbProvider
    .From<Cliente>()
    .InsertRangeAsync(nuevosClientes);
```

#### Actualización de Datos

```csharp
// Actualizar con filtro
int actualizados = await dbProvider
    .From<Cliente>()
    .Where(c => c.Pais == "España")
    .UpdateAsync(new { Descuento = 15 });

// Actualizar con expresión
int actualizados = await dbProvider
    .From<Cliente>()
    .Where(c => c.UltimaCompra < DateTime.Now.AddYears(-1))
    .UpdateAsync(c => new Cliente { Activo = false });
```

#### Eliminación de Datos

```csharp
// Eliminar con filtro
int eliminados = await dbProvider
    .From<Cliente>()
    .Where(c => !c.Activo && c.FechaRegistro < DateTime.Now.AddYears(-5))
    .DeleteAsync();
```

#### Consultas Avanzadas

```csharp
// Consulta con joins implícitos
var pedidos = await dbProvider
    .From<Pedido>()
    .Join<Cliente>((p, c) => p.ClienteId == c.Id)
    .Where((p, c) => c.Pais == "México" && p.Fecha.Year == 2023)
    .Select((p, c) => new { 
        p.Id, 
        Cliente = c.Nombre,
        p.Fecha,
        p.Total 
    })
    .OrderByDescending(x => x.Total)
    .ToListAsync();

// Agregaciones
var resumen = await dbProvider
    .From<Pedido>()
    .GroupBy(p => p.ClienteId)
    .Select(g => new {
        ClienteId = g.Key,
        TotalPedidos = g.Count(),
        MontoTotal = g.Sum(p => p.Total),
        Promedio = g.Average(p => p.Total)
    })
    .ToListAsync();
```

### Métodos Disponibles

#### Métodos de Configuración
- `From<T>()`: Inicia una nueva consulta para la entidad T
- `Select<TResult>()`: Especifica las columnas a seleccionar
- `Where(Expression<Func<T, bool>>)`: Filtra los resultados
- `OrderBy/OrderByDescending`: Ordena los resultados
- `ThenBy/ThenByDescending`: Ordenación adicional
- `Skip/Take`: Paginación de resultados
- `GroupBy`: Agrupa los resultados
- `Having`: Filtra grupos
- `Distinct`: Elimina duplicados

#### Métodos de Ejecución
- `ToList()`: Ejecuta la consulta y devuelve una lista
- `FirstOrDefault()`: Devuelve el primer elemento o valor por defecto
- `Count()`: Cuenta los registros
- `Any()`: Verifica si hay algún registro
- `ExecuteNonQuery()`: Ejecuta la consulta y devuelve el número de filas afectadas
- `ExecuteScalar<T>()`: Ejecuta la consulta y devuelve el primer valor
- `ToDataTable()`: Devuelve los resultados como DataTable

#### Métodos de Modificación
- `Insert(T)`: Inserta un nuevo registro
- `InsertRange(IEnumerable<T>)`: Inserta múltiples registros
- `Update(object)`: Actualiza registros
- `Update(Expression<Func<T, T>>)`: Actualiza con expresión
- `Delete()`: Elimina registros

### Buenas Prácticas

1. **Usar parámetros**: Siempre usa expresiones lambda en lugar de cadenas SQL para evitar inyección SQL
2. **Selectivo con las columnas**: Usa `Select()` para obtener solo las columnas necesarias
3. **Paginación**: Usa `Skip()` y `Take()` para consultas que podrían devolver muchos registros
4. **Transacciones**: Envuelve operaciones relacionadas en transacciones
5. **Async/Await**: Usa métodos asíncronos para operaciones de E/S
6. **Manejo de errores**: Implementa try-catch para manejar excepciones de base de datos

### Ejemplo Completo

```csharp
try
{
    using (var transaction = await dbProvider.BeginTransactionAsync())
    {
        try
        {
            // Insertar nuevo cliente
            var nuevoCliente = new Cliente
            {
                Nombre = "Empresa Ejemplo",
                Email = "contacto@ejemplo.com",
                FechaRegistro = DateTime.Now,
                Activo = true
            };

            int clienteId = await dbProvider
                .From<Cliente>()
                .InsertAsync(nuevoCliente);

            // Crear pedido para el cliente
            var nuevoPedido = new Pedido
            {
                ClienteId = clienteId,
                Fecha = DateTime.Now,
                Total = 1500.50m,
                Estado = "Pendiente"
            };

            await dbProvider
                .From<Pedido>()
                .InsertAsync(nuevoPedido);

            // Confirmar transacción
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
catch (Exception ex)
{
    // Manejar error
    Console.WriteLine($"Error al procesar la transacción: {ex.Message}");
}
```

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
