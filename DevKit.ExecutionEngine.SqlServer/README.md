# SQL Server Database Provider for DeveloperKit

[![NuGet](https://img.shields.io/nuget/v/DevKit.ExecutionEngine.SqlServer.svg?style=flat-square)](https://www.nuget.org/packages/DevKit.ExecutionEngine.SqlServer/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-blue)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2012%2B-red)](https://www.microsoft.com/sql-server/)

High-performance SQL Server database provider for .NET applications, built on top of ADO.NET with a clean, consistent API for both synchronous and asynchronous operations.

## ✨ Features

- **Unified API**: Consistent method naming across all database operations
- **Full Async Support**: Optimized async/await patterns with proper cancellation support
- **Bulk Operations**: High-performance bulk operations with configurable timeouts
- **Transaction Support**: Comprehensive transaction management with async support
- **Stored Procedures**: Full support for executing stored procedures with parameters
- **Type Safety**: Strongly-typed result mapping and parameter handling
- **Dependency Injection**: First-class support for .NET Core DI
- **Configurable**: Fine-grained control over timeouts and connection behavior
- **Multiple Result Sets**: Support for complex queries with multiple result sets
- **Modern .NET Standards**: Built with .NET 6.0+ and .NET Standard 2.1+ in mind

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
    options.CommandTimeout = 30; // seconds
    options.ApplicationName = "MyApplication";
    
    // Connection pooling
    options.ConnectionPooling = new ConnectionPoolingOptions 
    { 
        MaxPoolSize = 200,
        MinPoolSize = 10,
        ConnectionLifetime = 300 // seconds
    };
    
    // Bulk copy options with timeout configuration
    options.BulkCopy = new BulkCopyOptions 
    { 
        BatchSize = 1000,
        BulkCopyTimeout = 600, // 10 minutes
        EnableStreaming = true,
        UseInternalTransaction = false,
        NotifyAfter = 1000 // Raise event after every 1000 rows
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
    private readonly ISQLServerDatabaseProvider _dbProvider;

    public CustomerService(ISQLServerDatabaseProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    // Basic query with parameters
    public async Task<Customer> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT * FROM Customers WHERE Id = @Id";
        
        var customers = await _dbProvider.ExecuteQueryAsListAsync(query,
            reader => new Customer
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            },
            parameters =>
            {
                parameters.AddWithValue("@Id", id);
            },
            cancellationToken);

        return customers.FirstOrDefault();
    }
}
```

### Using Query Builder

```csharp
// Fluent query building
public async Task<List<Customer>> SearchCustomersAsync(string searchTerm, int page = 1, int pageSize = 20)
{
    return await _dbProvider
        .From<Customer>()
        .Where(c => c.Name.Contains(searchTerm) || c.Email.Contains(searchTerm))
        .OrderBy(c => c.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}
```

### Batch Insert

```csharp
public async Task<int> ImportCustomersAsync(IEnumerable<Customer> customers, CancellationToken cancellationToken = default)
{
    // Using batch insert with configurable batch size
    await _dbProvider.ExecuteInsertAsync("Customers", customers, batchSize: 1000, cancellationToken);
    return customers.Count();
}
```

### Transactions

```csharp
public async Task<bool> TransferFundsAsync(int fromAccountId, int toAccountId, decimal amount)
{
    using (var transaction = _dbProvider.BeginTransaction())
    {
        try
        {
            // Withdraw from source account
            await _dbProvider.ExecuteNonQueryAsync(
                "UPDATE Accounts SET Balance = Balance - @Amount WHERE Id = @Id AND Balance >= @Amount",
                parameters =>
                {
                    parameters.AddWithValue("@Id", fromAccountId);
                    parameters.AddWithValue("@Amount", amount);
                });

            // Deposit to target account
            await _dbProvider.ExecuteNonQueryAsync(
                "UPDATE Accounts SET Balance = Balance + @Amount WHERE Id = @Id",
                parameters =>
                {
                    parameters.AddWithValue("@Id", toAccountId);
                    parameters.AddWithValue("@Amount", amount);
                });

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

### Stored Procedures

```csharp
public async Task<List<OrderSummary>> GetCustomerOrderSummaryAsync(int customerId, DateTime startDate, DateTime endDate)
{
    return await _dbProvider.ExecuteStoredProcedureAsListAsync("sp_GetCustomerOrderSummary",
        reader => new OrderSummary
        {
            OrderId = reader.GetInt32(0),
            OrderDate = reader.GetDateTime(1),
            TotalAmount = reader.GetDecimal(2),
            ItemCount = reader.GetInt32(3)
        },
        parameters =>
        {
            parameters.AddWithValue("@CustomerId", customerId);
            parameters.AddWithValue("@StartDate", startDate);
            parameters.AddWithValue("@EndDate", endDate);
        });
}
```

### Bulk Operations

```csharp
// Bulk insert with configuration and cancellation
public async Task BulkInsertProductsAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
{
    try 
    {
        await _dbProvider.ExecuteBulkInsertAsync(products, config =>
        {
            config.BatchSize = 5000;
            config.DestinationTableName = "Products";
            config.ColumnMappings.Add("Id", "ProductId");
            config.ColumnMappings.Add("Name", "ProductName");
            config.ColumnMappings.Add("Price", "UnitPrice");
            config.ColumnMappings.Add("Stock", "UnitsInStock");
        }, cancellationToken);
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Bulk insert operation was cancelled");
        throw;
    }
}

// Bulk insert from DataTable
public async Task BulkInsertFromDataTableAsync(DataTable data, string tableName, CancellationToken cancellationToken = default)
{
    try 
    {
        await _dbProvider.ExecuteBulkInsertToTableAsync(data, tableName, cancellationToken);
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Bulk insert operation was cancelled");
        throw;
    }
}
```

## Advanced Query Builder Examples

### Complex Queries with Joins

```csharp
public async Task<List<OrderWithCustomer>> GetRecentOrdersWithCustomersAsync(DateTime fromDate)
{
    return await _dbProvider
        .From<Order>("o")
        .Join<Customer>("c", "c.Id = o.CustomerId")
        .Where<Order>(o => o.OrderDate >= fromDate)
        .Select((o, c) => new OrderWithCustomer
        {
            OrderId = o.Id,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            CustomerName = c.Name,
            CustomerEmail = c.Email
        })
        .OrderByDescending(x => x.OrderDate)
        .Take(50)
        .ToListAsync();
}
```

### Grouping and Aggregation

```csharp
public async Task<List<SalesByCategory>> GetMonthlySalesByCategoryAsync(int year)
{
    return await _dbProvider
        .From<Order>("o")
        .Join<OrderDetail>("od", "o.Id = od.OrderId")
        .Join<Product>("p", "p.Id = od.ProductId")
        .Join<Category>("c", "c.Id = p.CategoryId")
        .Where(o => o.OrderDate.Year == year)
        .GroupBy(o => new { o.OrderDate.Month, c.Name })
        .Select(g => new SalesByCategory
        {
            Month = g.Key.Month,
            Category = g.Key.Name,
            TotalSales = g.Sum(x => x.od.Quantity * x.od.UnitPrice),
            OrderCount = g.Count()
        })
        .OrderBy(x => x.Month)
        .ThenByDescending(x => x.TotalSales)
        .ToListAsync();
}
```

## Performance Tips

1. **Connection Management**:
   - Let the provider manage connections (they're pooled by default)
   - Avoid opening connections manually unless necessary
   - Use `ConfigureAwait(false)` in library code to prevent deadlocks

2. **Parameterized Queries**:
   - Always use parameters to prevent SQL injection
   - Reuse parameterized commands when possible
   - Use `AddWithValue` for simple parameter mapping

3. **Async/Await Best Practices**:
   - Always pass `CancellationToken` to async methods
   - Handle `OperationCanceledException` for proper cancellation
   - Use `ValueTask<T>` for hot paths with synchronous completion

4. **Bulk Operations**:
   - Use `ExecuteBulkInsertToTableAsync` for large datasets
   - Configure appropriate `BatchSize` and `BulkCopyTimeout`
   - Consider `TableLock` option for faster bulk inserts in exclusive scenarios

5. **Query Optimization**:
   - Use `Select()` to retrieve only needed columns
   - Apply filters early with `Where()`
   - Use `Take()` to limit result sets
   - Consider using `AsNoTracking()` for read-only queries

## Troubleshooting

### Common Issues

1. **Connection Timeouts**:
   - Increase `CommandTimeout` for long-running queries
   - Check network latency and server load

2. **Deadlocks**:
   - Use appropriate transaction isolation levels
   - Keep transactions short and focused

3. **Performance Problems**:
   - Check query execution plans
   - Ensure proper indexing
   - Consider query hints for complex queries

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Async Query with Parameters and Cancellation

```csharp
public async Task<Customer> GetCustomerByEmailAsync(string email, CancellationToken cancellationToken = default)
{
    var query = "SELECT * FROM Customers WHERE Email = @Email";
    
    try
    {
        var customer = await _dbProvider.ExecuteQueryAsSingleAsync<Customer>(query, 
            parameters: p => p.AddWithValue("@Email", email),
            cancellationToken: cancellationToken);
            
        return customer;
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        _logger.LogInformation("Customer query was cancelled");
        throw;
    }
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
public async Task<bool> ProcessOrderAsync(Order order, List<OrderItem> items, CancellationToken cancellationToken = default)
{
    using (var transaction = await _provider.BeginTransactionAsync(cancellationToken))
    {
        try
        {
            // Insertar orden
            var orderId = await _provider.ExecuteScalarAsync<int>(
                "INSERT INTO Orders (CustomerId, OrderDate, Total) OUTPUT INSERTED.Id VALUES (@CustomerId, @OrderDate, @Total);",
                p =>
                {
                    p.AddWithValue("@CustomerId", order.CustomerId);
                    p.AddWithValue("@OrderDate", DateTime.UtcNow);
                    p.AddWithValue("@Total", order.Total);
                },
                cancellationToken);

            // Insertar items
            foreach (var item in items)
            {
                await _provider.ExecuteNonQueryAsync(
                    "INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)",
                    p =>
                    {
                        p.AddWithValue("@OrderId", orderId);
                        p.AddWithValue("@ProductId", item.ProductId);
                        p.AddWithValue("@Quantity", item.Quantity);
                        p.AddWithValue("@UnitPrice", item.UnitPrice);
                    },
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error al procesar la orden");
            return false;
        }
    }
}
```

### Inserción Masiva (Bulk Insert)

```csharp
// Inserción masiva desde DataTable con cancelación
public async Task BulkInsertCustomersAsync(DataTable customersData, CancellationToken cancellationToken = default)
{
    try
    {
        await _provider.ExecuteBulkInsertToTableAsync(
            customersData, 
            "Customers",
            cancellationToken);
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Inserción masiva cancelada");
        throw;
    }
}

// Inserción masiva con configuración avanzada
public async Task BulkInsertWithAdvancedOptionsAsync(DataTable data, CancellationToken cancellationToken = default)
{
    try
    {
        await _provider.ExecuteBulkInsertToTableAsync(
            data, 
            "Customers", 
            options =>
            {
                options.BatchSize = 10000;
                options.BulkCopyTimeout = 600; // 10 minutos
                options.SqlBulkCopyOptions = SqlBulkCopyOptions.TableLock;
                options.NotifyAfter = 1000; // Notificar cada 1000 filas
            },
            cancellationToken);
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Inserción masiva con opciones avanzadas cancelada");
        throw;
    }
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

- `DataTable GetTableFromQuery(string query, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta una consulta SQL y devuelve un `DataTable` con los resultados.

- `DataTable GetTableFromStoredProcedure(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta un procedimiento almacenado y devuelve un `DataTable`.

- `T GetItemFromQuery<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta una consulta y transforma la primera fila a un objeto del tipo `T`.

- `T GetItemFromStoredProcedure<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)`  
  Igual que el anterior, pero con procedimiento almacenado.

- `ICollection<Dictionary<string, object>> GetDictionaryFromQuery(string query, Action<IDataParameterCollection> dbParameters = null)`  
  Obtiene resultados de consulta como una colección de diccionarios con nombre/valor.

- `ICollection<Dictionary<string, object>> GetDictionaryFromStoredProcedure(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)`  
  Igual que el anterior, pero para procedimientos almacenados.

- `ICollection<T> GetItemsFromStoredProcedure<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)`  
  Obtiene una colección de objetos tipo `T` a partir de un procedimiento almacenado.

- `ICollection<T> GetItemsFromQuery<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)`  
  Obtiene una colección de objetos tipo `T` a partir de una consulta SQL.

### Ejecución de comandos

- `void ExecuteNonQuery(string command, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta comandos SQL que no retornan datos (INSERT, UPDATE, DELETE, etc.).

- `void ExecuteInsert<T>(string tableName, T entity) where T : class, new()`  
  Inserta un único objeto en la tabla especificada usando reflexión.

- `void ExecuteInsert<T>(string tableName, ICollection<T> collection) where T : class, new()`  
  Inserta una colección de objetos en la tabla especificada.

- `void ExecuteStoredProcedureCommand(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)`  
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

- `Task<T> GetItemFromQueryAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta una consulta SQL asíncrona y transforma la primera fila en un objeto `T`.

- `Task<T> GetItemFromStoredProcedureAsync<T>(string storedProcedure) where T : new()`  
  Ejecuta un procedimiento almacenado de forma asíncrona y devuelve un objeto `T` por defecto.

- `Task<T> GetItemFromStoredProcedureAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta un procedimiento almacenado de forma asíncrona y mapea la primera fila a un objeto `T`.

- `Task<DataTable> GetTableFromQueryAsync(string query, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta una consulta asíncrona y devuelve un `DataTable` con los resultados.

- `Task<DataTable> GetTableFromStoredProcedureAsync(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta un procedimiento almacenado de forma asíncrona y devuelve un `DataTable`.

- `Task<ICollection<Dictionary<string, object>>> GetDictionaryFromQueryAsync(string query, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta una consulta asíncrona y devuelve una colección de diccionarios.

- `Task<ICollection<Dictionary<string, object>>> GetDictionaryFromStoredProcedureAsync(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)`  
  Igual que el anterior, pero para procedimientos almacenados.

- `Task<ICollection<T>> GetItemsFromQueryAsync<T>(string query, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)`  
  Obtiene una colección de objetos `T` de una consulta asíncrona.

- `Task<ICollection<T>> GetItemsFromStoredProcedureAsync<T>(string storedProcedure) where T : new()`  
  Obtiene una colección de objetos `T` de un procedimiento almacenado asíncrono.

- `Task<ICollection<T>> GetItemsFromStoredProcedureAsync<T>(string storedProcedure, Func<IDataReader, T> expression, Action<IDataParameterCollection> dbParameters = null)`  
  Igual que el anterior, pero con mapeo personalizado.

### Comandos asíncronos

- `Task<int> ExecuteStoredProcedureCommandAsync(string storedProcedure, Action<IDataParameterCollection> dbParameters = null)`  
  Ejecuta un procedimiento almacenado asíncrono que devuelve el número de filas afectadas.

- `Task ExecuteNonQueryAsync(string command, Action<IDataParameterCollection> dbParameters = null)`  
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
