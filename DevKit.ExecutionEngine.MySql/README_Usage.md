# 📚 MySQL Provider - Guía de Uso Completa

## 🚀 Introducción

El **MySQL Provider** es una librería moderna para .NET que proporciona una API limpia, asíncrona y optimizada para interactuar con MySQL. Ofrece soporte completo para operaciones de base de datos con las mejores prácticas de .NET moderno.

### ✅ Características Principales

- **🔄 Async/Await**: Soporte completo para operaciones asíncronas con cancelación
- **🎯 Type-Safe**: Mapeo fuertemente tipado de entidades
- **⚡ Alto Rendimiento**: TypeConverters optimizados y caché de propiedades
- **📦 Bulk Operations**: Soporte para inserciones masivas de alto rendimiento
- **🗄️ Temp Tables**: Creación y gestión de tablas temporales
- **🛡️ Seguridad**: Parámetros tipados y prevención de SQL Injection
- **📊 Logging**: Integración para depuración y monitoreo
- **🔧 Configuración**: Opciones flexibles y validación

---

## 📋 Tabla de Contenido

1. [Configuración Inicial](#-configuración-inicial)
2. [Operaciones Básicas](#-operaciones-básicas)
3. [Consultas con Parámetros](#-consultas-con-parámetros)
4. [Procedimientos Almacenados](#-procedimientos-almacenados)
5. [Operaciones Asíncronas](#-operaciones-asíncronas)
6. [Operaciones Bulk](#-operaciones-bulk)
7. [Tablas Temporales](#-tablas-temporales)
8. [Manejo de Errores](#-manejo-de-errores)
9. [Configuración Avanzada](#-configuración-avanzada)
10. [Mejores Prácticas](#-mejores-prácticas)

---

## 🛠️ Configuración Inicial

### 1. Instalación del Paquete

```bash
dotnet add package DevKit.ExecutionEngine.MySql
```

### 2. Configuración en Startup.cs

```csharp
using DevKit.ExecutionEngine.MySQL;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configuración básica
        services.Configure<MySqlOptions>(options =>
        {
            options.ConnectionString = "Server=localhost;Database=MyDB;User ID=user;Password=password;";
            options.CommandTimeout = 30;
            options.BulkCopyTimeout = 300;
            options.BulkCopyBatchSize = 1000;
        });

        // Registrar el provider
        services.AddTransient<IMySqlProvider, MySqlProvider>();
    }
}
```

### 3. Inyección de Dependencias

```csharp
public class UserService
{
    private readonly IMySqlProvider _dbProvider;

    public UserService(IMySqlProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }
}
```

---

## 🔍 Operaciones Básicas

### Consulta Simple

```csharp
// Obtener todos los usuarios
public async Task<List<User>> GetAllUsersAsync()
{
    string query = "SELECT Id, Name, Email FROM Users";
    
    var users = await _dbProvider.ExecuteQueryAsListAsync<User>(
        query, 
        reader => new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email")
        });
    
    return users.ToList();
}
```

### Obtener un Solo Registro

```csharp
// Obtener usuario por ID
public async Task<User> GetUserByIdAsync(int userId)
{
    string query = "SELECT Id, Name, Email FROM Users WHERE Id = @Id";
    
    var user = await _dbProvider.ExecuteQueryAsSingleAsync<User>(
        query,
        reader => new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email")
        },
        parameters => parameters.AddMySqlParameter("Id", userId));
    
    return user;
}
```

### Consulta Escalar

```csharp
// Contar usuarios
public async Task<int> GetUserCountAsync()
{
    string query = "SELECT COUNT(*) FROM Users";
    
    return await _dbProvider.ExecuteScalarAsync<int>(query);
}
```

---

## 📝 Consultas con Parámetros

### Parámetros con Extensiones Especializadas

El proyecto incluye `MySqlParameterExtensions` que proporciona métodos avanzados para manejo de parámetros:

#### Parámetros desde Objetos

```csharp
public async Task<User> CreateUserAsync(User user)
{
    string query = @"
        INSERT INTO Users (Name, Email, Age, Active) 
        VALUES (@Name, @Email, @Age, @Active);
        
    SELECT LAST_INSERT_ID();";
    
    var userId = await _dbProvider.ExecuteScalarAsync<int>(query,
        parameters => parameters.AddMySqlParameters(user));
    
    user.Id = userId;
    return user;
}
```

#### Parámetros desde Diccionarios

```csharp
public async Task<List<User>> SearchUsersAsync(Dictionary<string, object> filters)
{
    string query = "SELECT Id, Name, Email FROM Users WHERE 1=1";
    
    // Construir consulta dinámica con parámetros
    var parameters = new Dictionary<string, object>();
    
    if (filters.ContainsKey("Name"))
    {
        query += " AND Name LIKE CONCAT('%', @Name, '%')";
        parameters["Name"] = filters["Name"];
    }
    
    if (filters.ContainsKey("MinAge"))
    {
        query += " AND Age >= @MinAge";
        parameters["MinAge"] = filters["MinAge"];
    }
    
    return (List<User>)await _dbProvider.ExecuteQueryAsListAsync<User>(
        query,
        reader => new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email")
        },
        parameters => parameters.AddMySqlParameters(parameters));
}
```

#### Parámetros con Tipos Específicos

```csharp
public async Task<bool> InsertProductAsync(Product product)
{
    string query = @"
        INSERT INTO Products (Name, Price, Description, Image, CategoryId) 
        VALUES (@Name, @Price, @Description, @Image, @CategoryId)";
    
    await _dbProvider.ExecuteNonQueryAsync(query,
        parameters => 
        {
            // Parámetro con tipo específico y tamaño
            parameters.AddMySqlParameter("Name", product.Name, 
                MySqlDbType.VarChar, 100);
                
            // Parámetro decimal con precisión
            parameters.AddMySqlParameter("Price", product.Price, 
                MySqlDbType.Decimal, precision: 18, scale: 2);
                
            // Parámetro grande (texto largo)
            parameters.AddMySqlParameter("Description", product.Description, 
                MySqlDbType.Text);
                
            // Parámetro binario
            parameters.AddMySqlParameter("Image", product.ImageData, 
                MySqlDbType.LongBlob);
                
            // Parámetro de salida
            parameters.AddMySqlParameter("CategoryId", product.CategoryId, 
                direction: ParameterDirection.Input);
        });
    
    return true;
}
```

#### Parámetros con Logging

```csharp
public async Task<User> GetUserWithLoggingAsync(int userId)
{
    string query = "SELECT * FROM Users WHERE Id = @Id";
    
    return await _dbProvider.ExecuteQueryAsSingleAsync<User>(
        query,
        reader => reader.GetEntity<User>(),
        parameters => parameters.AddMySqlParameter("Id", userId, 
            log: Console.WriteLine));
}
```

### Parámetros Posicionales

```csharp
public async Task<List<User>> GetUsersByAgeAsync(int minAge)
{
    string query = "SELECT Id, Name, Email, Age FROM Users WHERE Age >= @MinAge";
    
    return (List<User>)await _dbProvider.ExecuteQueryAsListAsync<User>(
        query,
        reader => new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            Age = reader.GetInt32("Age")
        },
        parameters => parameters.AddMySqlParameter("MinAge", minAge));
}
```

### Múltiples Parámetros

```csharp
public async Task<List<User>> SearchUsersAsync(string name, int? minAge, string email)
{
    string query = @"
        SELECT Id, Name, Email, Age 
        FROM Users 
        WHERE (@Name IS NULL OR Name LIKE CONCAT('%', @Name, '%'))
        AND (@MinAge IS NULL OR Age >= @MinAge)
        AND (@Email IS NULL OR Email = @Email)";
    
    return (List<User>)await _dbProvider.ExecuteQueryAsListAsync<User>(
        query,
        reader => new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            Age = reader.GetInt32("Age")
        },
        parameters => 
        {
            parameters.AddMySqlParameter("Name", 
                string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
            parameters.AddMySqlParameter("MinAge", 
                minAge ?? (object)DBNull.Value);
            parameters.AddMySqlParameter("Email", 
                string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
        });
}
```

---

## 🗄️ Procedimientos Almacenados

### Ejecutar SP con Resultados

```csharp
public async Task<User> GetUserByStoredProcedureAsync(int userId)
{
    return await _dbProvider.ExecuteProcedureAsSingleAsync<User>(
        "sp_GetUserById",
        reader => new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email")
        },
        parameters => parameters.AddMySqlParameter("UserId", userId));
}
```

### Ejecutar SP sin Resultados

```csharp
public async Task UpdateUserLastLoginAsync(int userId)
{
    await _dbProvider.ExecuteProcedureCommandAsync(
        "sp_UpdateUserLastLogin",
        parameters => parameters.AddMySqlParameter("UserId", userId));
}
```

### SP que Devuelve Lista

```csharp
public async Task<List<User>> GetActiveUsersAsync()
{
    return (List<User>)await _dbProvider.ExecuteProcedureAsListAsync<User>(
        "sp_GetActiveUsers",
        reader => new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email")
        });
}
```

---

## ⚡ Operaciones Asíncronas

### Configuración de Timeout

```csharp
public async Task<List<User>> GetUsersWithTimeoutAsync()
{
    string query = "SELECT * FROM Users WHERE Status = 'Active'";
    
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    
    try
    {
        return (List<User>)await _dbProvider.ExecuteQueryAsListAsync<User>(
            query,
            reader => new User
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Email = reader.GetString("Email")
            },
            cancellationToken: cts.Token);
    }
    catch (OperationCanceledException)
    {
        // Manejar timeout
        throw new TimeoutException("La consulta excedió el tiempo límite de 10 segundos");
    }
}
```

### Operación con Cancelación

```csharp
public async Task<List<User>> GetUsersWithCancellationAsync(
    CancellationToken cancellationToken = default)
{
    string query = "SELECT Id, Name, Email FROM Users ORDER BY Id";
    
    return (List<User>)await _dbProvider.ExecuteQueryAsListAsync<User>(
        query,
        reader => new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email")
        },
        cancellationToken: cancellationToken);
}
```

---

## 📦 Operaciones Bulk

### Bulk Insert Simple

```csharp
public async Task ImportUsersAsync(List<User> users)
{
    if (users.Count == 0) return;
    
    // Convertir a DataTable
    var dataTable = new DataTable();
    dataTable.Columns.Add("Id", typeof(int));
    dataTable.Columns.Add("Name", typeof(string));
    dataTable.Columns.Add("Email", typeof(string));
    dataTable.Columns.Add("Age", typeof(int));
    
    foreach (var user in users)
    {
        dataTable.Rows.Add(user.Id, user.Name, user.Email, user.Age);
    }
    
    // Bulk insert
    await _dbProvider.ExecuteBulkInsertAsync(dataTable, "Users");
}
```

### Bulk Insert Configurado

```csharp
public async Task ImportLargeDatasetAsync(List<User> users)
{
    var dataTable = users.ToDataTable();
    
    // Configurar opciones de bulk copy
    var bulkOptions = new MySqlBulkCopyOptions
    {
        CheckConstraints = true,
        FireTriggers = false,
        KeepIdentity = true,
        KeepNulls = true
    };
    
    await _dbProvider.ExecuteBulkInsertAsync(dataTable, "Users", bulkOptions);
}
```

---

## 🗄️ Tablas Temporales

### Crear y Usar Tabla Temporal

```csharp
public async Task<List<Order>> ProcessOrdersAsync(List<int> orderIds)
{
    // Crear tabla temporal
    string tempTableName = await _dbProvider.CreateTemporaryTableAsync<Order>(
        "temp_orders_process", 
        new[] { "Id", "CustomerId", "Total", "Status" });
    
    try
    {
        // Insertar IDs en tabla temporal
        await _dbProvider.BulkInsertToTemporaryTableAsync(
            orderIds.Select(id => new { Id = id }), 
            tempTableName);
        
        // Procesar usando la tabla temporal
        string query = $@"
            SELECT o.* FROM Orders o
            INNER JOIN {tempTableName} t ON o.Id = t.Id
            WHERE o.Status = 'Pending'";
        
        return (List<Order>)await _dbProvider.ExecuteQueryAsListAsync<Order>(
            query,
            reader => new Order
            {
                Id = reader.GetInt32("Id"),
                CustomerId = reader.GetInt32("CustomerId"),
                Total = reader.GetDecimal("Total"),
                Status = reader.GetString("Status")
            });
    }
    finally
    {
        // La tabla temporal se elimina automáticamente al dispose
    }
}
```

---

## ⚠️ Manejo de Errores

### Captura de Excepciones Específicas

```csharp
public async Task<User> SafeGetUserAsync(int userId)
{
    try
    {
        return await _dbProvider.ExecuteQueryAsSingleAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id",
            reader => reader.GetEntity<User>(),
            parameters => parameters.AddMySqlParameter("Id", userId));
    }
    catch (MySqlException ex) when (ex.Number == 1045)
    {
        throw new BusinessException("Error de autenticación en la base de datos", ex)
        {
            ErrorCode = "AUTH_ERROR"
        };
    }
    catch (MySqlException ex) when (ex.Number == 1205)
    {
        throw new BusinessException("Timeout en la operación de base de datos", ex)
        {
            ErrorCode = "TIMEOUT",
            RetryAfter = TimeSpan.FromSeconds(5)
        };
    }
    catch (MySqlException ex)
    {
        _logger.LogError(ex, "Error en consulta MySQL: {Message}", ex.Message);
        throw new BusinessException($"Error al obtener usuario {userId}", ex)
        {
            ErrorCode = "DATABASE_ERROR"
        };
    }
}
```

### Logging de Operaciones

```csharp
public async Task<bool> SafeExecuteAsync(string operation)
{
    try
    {
        await _dbProvider.ExecuteProcedureCommandAsync("sp_ProcessData",
            parameters => parameters.AddMySqlParameter("Operation", operation,
                log: msg => _logger.LogInformation("MySQL: {Message}", msg)));
        
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en operación MySQL {Operation}: {Message}", 
            operation, ex.Message);
        return false;
    }
}
```

---

## ⚙️ Configuración Avanzada

### Configuración Completa de Opciones

```csharp
services.Configure<MySqlOptions>(options =>
{
    // Conexión
    options.ConnectionString = builder.ConnectionString;
    
    // Timeouts
    options.CommandTimeout = 60;
    options.BulkCopyTimeout = 600;
    options.BulkCopyBatchSize = 5000;
    
    // Pool de conexiones
    options.MaximumPoolSize = 100;
    options.MinimumPoolSize = 5;
    options.ConnectionTimeout = 30;
    
    // SSL
    options.SslMode = MySqlSslMode.Required;
    options.CertificateFile = "/path/to/cert.pem";
});
```

### Validación Personalizada

```csharp
public class CustomMySqlOptionsValidator : IValidateOptions<MySqlOptions>
{
    public ValidateOptionsResult Validate(string name, MySqlOptions options)
    {
        var failures = new List<string>();
        
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            failures.Add("ConnectionString es requerido");
            
        if (options.CommandTimeout <= 0 || options.CommandTimeout > 3600)
            failures.Add("CommandTimeout debe estar entre 1 y 3600 segundos");
            
        if (options.BulkCopyBatchSize <= 0)
            failures.Add("BulkCopyBatchSize debe ser mayor a 0");
        
        return failures.Any() 
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

// Registrar validador
services.AddSingleton<IValidateOptions<MySqlOptions>, CustomMySqlOptionsValidator>();
```

---

## 🎯 Mejores Prácticas

### 1. Usar Siempre Parámetros

```csharp
// ❌ MAL - Vulnerable a SQL Injection
string query = $"SELECT * FROM Users WHERE Name = '{userName}'";

// ✅ BIEN - Seguro con parámetros
string query = "SELECT * FROM Users WHERE Name LIKE CONCAT('%', @Name, '%')";
await _dbProvider.ExecuteQueryAsListAsync<User>(
    query,
    reader => reader.GetEntity<User>(),
    parameters => parameters.AddMySqlParameter("Name", userName));
```

### 2. Selección de Columnas Específicas

```csharp
// ❌ MAL - Trae todas las columnas
string query = "SELECT * FROM Users";

// ✅ BIEN - Solo columnas necesarias
string query = "SELECT Id, Name, Email FROM Users WHERE Active = 1";
```

### 3. Paginación para Grandes Consultas

```csharp
public async Task<PagedResult<User>> GetUsersPagedAsync(int page, int pageSize)
{
    int offset = (page - 1) * pageSize;
    
    string query = @"
        SELECT Id, Name, Email 
        FROM Users 
        WHERE Active = 1
        ORDER BY Id
        LIMIT @PageSize OFFSET @Offset";
    
    var users = await _dbProvider.ExecuteQueryAsListAsync<User>(
        query,
        reader => new User
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email")
        },
        parameters => 
        {
            parameters.AddMySqlParameter("PageSize", pageSize);
            parameters.AddMySqlParameter("Offset", offset);
        });
    
    // Obtener total
    string countQuery = "SELECT COUNT(*) FROM Users WHERE Active = 1";
    int total = await _dbProvider.ExecuteScalarAsync<int>(countQuery);
    
    return new PagedResult<User>
    {
        Items = users.ToList(),
        Total = total,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling((double)total / pageSize)
    };
}
```

### 4. Transacciones Cortas

```csharp
// ❌ MAL - Transacción larga
public async Task ProcessLargeDataSetAsync(List<Data> items)
{
    await _dbProvider.ExecuteInTransactionAsync(async (transaction) =>
    {
        foreach (var item in items) // 10,000 items
        {
            await ProcessItemAsync(item); // Operación lenta
        }
    });
}

// ✅ BIEN - Transacciones cortas o batch processing
public async Task ProcessLargeDataSetAsync(List<Data> items)
{
    const int batchSize = 100;
    
    for (int i = 0; i < items.Count; i += batchSize)
    {
        var batch = items.Skip(i).Take(batchSize).ToList();
        
        await _dbProvider.ExecuteInTransactionAsync(async (transaction) =>
        {
            foreach (var item in batch)
            {
                await ProcessItemAsync(item);
            }
        });
    }
}
```

### 5. Configuración de Timeouts Apropiados

```csharp
// ✅ BIEN - Timeouts configurados según operación
public class DatabaseOperations
{
    private readonly IMySqlProvider _fastProvider;   // 5s timeout
    private readonly IMySqlProvider _slowProvider;   // 300s timeout
    
    public async Task<List<User>> GetActiveUsersAsync()
    {
        // Operación rápida - timeout corto
        return (List<User>)await _fastProvider.ExecuteQueryAsListAsync<User>(
            "SELECT Id, Name FROM Users WHERE Active = 1");
    }
    
    public async Task<Report> GenerateComplexReportAsync()
    {
        // Operación compleja - timeout largo
        return await _slowProvider.ExecuteQueryAsSingleAsync<Report>(
            "CALL sp_GenerateComplexReport");
    }
}
```

---

## 📚 Referencia Rápida de Métodos

### Consultas
```csharp
ExecuteQueryAsSingleAsync<T>(query, expression, parameters, ct)
ExecuteQueryAsListAsync<T>(query, expression, parameters, ct)
ExecuteQueryAsDictionaryAsync(query, parameters, ct)
ExecuteQueryAsTableAsync(query, parameters, ct)
ExecuteScalarAsync<T>(query, parameters, ct)
```

### Procedimientos Almacenados
```csharp
ExecuteProcedureAsSingleAsync<T>(sp, expression, parameters, ct)
ExecuteProcedureAsListAsync<T>(sp, expression, parameters, ct)
ExecuteProcedureAsDictionaryAsync(sp, parameters, ct)
ExecuteProcedureAsTableAsync(sp, parameters, ct)
ExecuteProcedureCommandAsync(sp, parameters, ct)
```

### Bulk Operations
```csharp
ExecuteBulkInsertAsync(dataTable, tableName, options)
ExecuteBulkInsertToTableAsync(dataTable, tableName, options)
```

### Tablas Temporales
```csharp
CreateTemporaryTableAsync<T>(tableName, columns)
BulkInsertToTemporaryTableAsync(data, tableName)
```

---

## 📄 Licencia

Este proyecto está licenciado bajo MIT License. Ver archivo [LICENSE](LICENSE) para más detalles.

---

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor:
1. Fork el repositorio
2. Crear una rama de características
3. Enviar un Pull Request con pruebas
4. Seguir las guías de estilo del código

---

**🎉 ¡Listo para usar!**

Para cualquier pregunta o soporte, por favor abre un issue en el repositorio.
