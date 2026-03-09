# MySQL Database Provider for DeveloperKit

[![NuGet](https://img.shields.io/nuget/v/DevKit.ExecutionEngine.MySql.svg?style=flat-square)](https://www.nuget.org/packages/DevKit.ExecutionEngine.MySql/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0%2B-orange)](https://www.mysql.com/)

High-performance MySQL database provider for .NET applications, built on top of MySqlConnector with a clean, consistent API for both synchronous and asynchronous operations.

## ✨ Features

- **Unified API**: Consistent method naming across all database operations
- **Full Async Support**: All operations have async/await variants
- **Bulk Operations**: High-performance bulk inserts with `MySqlBulkCopy`
- **Temporary Tables**: Runtime creation and management of temporary tables
- **Stored Procedures**: Full support for executing stored procedures
- **Type Safety**: Strongly-typed result mapping
- **Dependency Injection**: First-class support for .NET Core DI
- **Configurable**: Fine-grained control over connection and command behavior

## 🚀 Getting Started

### Prerequisites

- .NET Standard 2.0+ or .NET 6.0+
- MySQL Server 8.0 or higher
- MySqlConnector NuGet package (automatically referenced)

### Installation

```bash
dotnet add package DevKit.ExecutionEngine.MySql
```

## 🛠 Configuration

### Basic Setup

```csharp
using DevKit.ExecutionEngine.MySql;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Basic configuration
services.Configure<MySqlOptions>(options =>
{
    options.ConnectionString = "Server=localhost;Database=myapp;User ID=user;Password=password;";
    options.CommandTimeout = 30; // seconds
});

services.AddMySqlDatabase();
```

### Advanced Configuration

```csharp
services.Configure<MySqlOptions>(options =>
{
    options.ConnectionString = "Server=localhost;Database=myapp;User ID=user;Password=password;";
    options.CommandTimeout = 30;
    
    // Connection pooling
    options.ConnectionPooling = new ConnectionPoolingOptions 
    { 
        Pooling = true,
        MinPoolSize = 5,
        MaxPoolSize = 100,
        ConnectionIdleTimeout = 300 // seconds
    };
    
    // Bulk copy options
    options.BulkCopy = new BulkCopyAdvancedOptions 
    { 
        BatchSize = 1000,
        BulkCopyTimeout = 0, // no timeout
        EnableLogging = true,
        AllowLoadLocalInfile = true
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
    private readonly IMySqlDatabaseProvider _db;
    
    public CustomerService(IMySqlDatabaseProvider db)
    {
        _db = db;
    }
    
    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
    {
        return await _db.ExecuteQueryAsListAsync<Customer>(
            "SELECT * FROM customers WHERE is_active = @isActive",
            new { isActive = true });
    }
}
```

### Parameterized Queries

```csharp
// Using anonymous types for parameters
var customers = await _db.ExecuteQueryAsListAsync<Customer>(
    "SELECT * FROM customers WHERE created_at > @minDate AND status = @status",
    new { minDate = DateTime.UtcNow.AddDays(-30), status = "active" });

// Using dictionary for parameters
var parameters = new Dictionary<string, object>
{
    ["minDate"] = DateTime.UtcNow.AddDays(-30),
    ["status"] = "active"
};

var customers = await _db.ExecuteQueryAsListAsync<Customer>(
    "SELECT * FROM customers WHERE created_at > @minDate AND status = @status",
    parameters);
```

### Stored Procedures

```csharp
// Executing a stored procedure
var result = await _db.ExecuteStoredProcedureAsListAsync<CustomerReport>(
    "sp_GetCustomerReport",
    new 
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
await _db.ExecuteBulkInsertAsync("customers", customerData);

// Bulk insert from list of objects
List<Customer> customers = GetCustomers();
await _db.ExecuteBulkInsertToTableAsync("customers", customers);
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
| `CreateTemporaryTable` | Creates a temporary table at runtime |

### Async Variants

All methods have async counterparts with the `Async` suffix (e.g., `ExecuteQueryAsListAsync`).

## ⚙️ Advanced Topics

### Transaction Management

```csharp
using (var transaction = await _db.BeginTransactionAsync())
try
{
    // Perform multiple operations within the same transaction
    await _db.ExecuteNonQueryAsync("UPDATE accounts SET balance = balance - @amount WHERE id = @id", 
        new { id = 1, amount = 100 });
        
    await _db.ExecuteNonQueryAsync("UPDATE accounts SET balance = balance + @amount WHERE id = @id", 
        new { id = 2, amount = 100 });
        
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Custom Type Mapping

```csharp
// Register custom type handlers
MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(connectionString);
builder.AllowUserVariables = true;

// Configure JSON serialization for complex types
var options = new MySqlOptions
{
    ConnectionString = builder.ConnectionString,
    JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    }
};
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
    p => { var prm = (MySqlConnector.MySqlParameterCollection)p; prm.AddMySqlParameter("p0", true); }
);
```

## Inserción masiva

```csharp
// Inserta un DataTable en una tabla existente
provider.ExecuteBulkInsert(dt, "schema.Customers");

// Crea una tabla temporal con el esquema del DataTable y luego inserta
provider.ExecuteBulkInsertToTable(dt, "temp.Customers_Load");
```

## Métodos clave

- Conjunto síncrono: `ExecuteQueryAsTable`, `ExecuteQueryAsList<T>`, `ExecuteQueryAsDictionary`, `ExecuteNonQuery`, `ExecuteProcedure*`, `BeginTransaction`, `CommitTransaction`, `RollbackTransaction`.
- Conjunto asíncrono: `ExecuteQueryAsTableAsync`, `ExecuteQueryAsListAsync<T>`, `ExecuteQueryAsDictionaryAsync`, `ExecuteNonQueryAsync`, `ExecuteProcedure*Async`, `ExecuteBulkInsertAsync`, `ExecuteBulkInsertToTableAsync`.

## Requisitos

- .NET 8.0+
- [MySqlConnector](https://mysqlconnector.net/)

## Licencia

MIT. Ver `LICENSE` en el repositorio raíz.
