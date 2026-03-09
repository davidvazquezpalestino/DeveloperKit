# 📚 SQL Server Provider - Guía de Uso Completa

## 🚀 Introducción

El **SQL Server Provider** es una librería moderna para .NET 10 que proporciona una API limpia, asíncrona y optimizada para interactuar con SQL Server. Cumple con los principios SOLID y las mejores prácticas de Clean Code de Robert C. Martin.

### ✅ Características Principales

- **🔄 Async/Await**: Soporte completo para operaciones asíncronas con cancelación
- **🎯 Type-Safe**: Mapeo fuertemente tipado de entidades
- **⚡ Alto Rendimiento**: Caché de propiedades, TypeConverters optimizados, Span<T>/Memory<T>
- **🔒 Concurrencia**: Control con SemaphoreSlim para transacciones
- **📊 Telemetría**: Integración con DiagnosticSource para monitoreo
- **🛡️ Excepciones**: Jerarquía de excepciones con contexto rico
- **🌊 Streaming**: IAsyncEnumerable para procesar grandes volúmenes de datos
- **✅ Validación**: Configuración validada con IValidateOptions

---

## 📋 Tabla de Contenido

1. [Configuración Inicial](#-configuración-inicial)
2. [Operaciones Básicas](#-operaciones-básicas)
3. [Consultas con Parámetros](#-consultas-con-parámetros)
4. [Procedimientos Almacenados](#-procedimientos-almacenados)
5. [Operaciones Asíncronas](#-operaciones-asíncronas)
6. [Streaming de Datos](#-streaming-de-datos)
7. [Transacciones](#-transacciones)
8. [Operaciones Bulk](#-operaciones-bulk)
9. [Manejo de Errores](#-manejo-de-errores)
10. [Configuración Avanzada](#-configuración-avanzada)
11. [Mejores Prácticas](#-mejores-prácticas)

---

## 🛠️ Configuración Inicial

### 1. Instalación del Paquete

```bash
dotnet add package DevKit.ExecutionEngine.SqlServer
```

### 2. Configuración en Startup.cs

```csharp
using DevKit.ExecutionEngine.SQLServer;
using DevKit.ExecutionEngine.SQLServer.Validation;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configuración básica
        services.ConfigureAndValidateSqlOptions(options =>
        {
            options.ConnectionString = "Server=localhost;Database=MyDB;Trusted_Connection=true;";
            options.CommandTimeout = 30;
            options.BulkCopy.BulkCopyTimeout = 300;
            options.BulkCopy.BatchSize = 1000;
        });

        // Registrar el provider
        services.AddTransient<ISQLServerProvider, SQLServerProvider>();
    }
}
```

### 3. Inyección de Dependencias

```csharp
public class UserService
{
    private readonly ISQLServerProvider _dbProvider;

    public UserService(ISQLServerProvider dbProvider)
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
        parameters => parameters.AddSqlParameter("Id", userId));
    
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

El proyecto incluye `SqlParameterExtensions` que proporciona métodos avanzados para manejo de parámetros:

#### Parámetros desde Objetos

```csharp
public async Task<User> CreateUserAsync(User user)
{
    string query = @"
        INSERT INTO Users (Name, Email, Age, Active) 
        OUTPUT INSERTED.Id 
        VALUES (@Name, @Email, @Age, @Active)";
    
    var userId = await _dbProvider.ExecuteScalarAsync<int>(query,
        parameters => parameters.AddSqlParameters(user));
    
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
        query += " AND Name LIKE @Name";
        parameters["Name"] = $"%{filters["Name"]}%";
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
        parameters => parameters.AddSqlParameters(parameters));
}
```

#### Parámetros con Tipos Específicos

```csharp
public async Task<bool> InsertProductAsync(Product product)
{
    string query = @"
        INSERT INTO Products (Name, Price, Description, Image, CategoryId) 
        VALUES (@Name, @Price, @Description, @Image, @CategoryId)";
    
    await _dbProvider.ExecuteProcedureCommandAsync(query,
        parameters => 
        {
            // Parámetro con tipo específico y tamaño
            parameters.AddSqlParameter("Name", product.Name, 
                SqlDbType.NVarChar, 100);
                
            // Parámetro decimal con precisión
            parameters.AddSqlParameter("Price", product.Price, 
                SqlDbType.Decimal, precision: 18, scale: 2);
                
            // Parámetro grande (texto largo)
            parameters.AddSqlParameter("Description", product.Description, 
                SqlDbType.NVarChar, -1); // -1 = MAX
                
            // Parámetro binario
            parameters.AddSqlParameter("Image", product.ImageData, 
                SqlDbType.VarBinary);
                
            // Parámetro de salida
            parameters.AddSqlParameter("CategoryId", product.CategoryId, 
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
        parameters => parameters.AddSqlParameter("Id", userId, logTo: Console.WriteLine));
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
        parameters => parameters.AddSqlParameter("MinAge", minAge));
}
```

### Múltiples Parámetros

```csharp
public async Task<List<User>> SearchUsersAsync(string name, int? minAge, string email)
{
    string query = @"
        SELECT Id, Name, Email, Age 
        FROM Users 
        WHERE (@Name IS NULL OR Name LIKE '%' + @Name + '%')
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
            parameters.AddSqlParameter("Name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
            parameters.AddSqlParameter("MinAge", minAge ?? (object)DBNull.Value);
            parameters.AddSqlParameter("Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
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
        parameters => parameters.AddSqlParameter("UserId", userId));
}
```

### Ejecutar SP sin Resultados

```csharp
public async Task UpdateUserLastLoginAsync(int userId)
{
    await _dbProvider.ExecuteProcedureCommandAsync(
        "sp_UpdateUserLastLogin",
        parameters => parameters.AddSqlParameter("UserId", userId));
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
public async IAsyncEnumerable<User> StreamUsersWithCancellationAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    string query = "SELECT Id, Name, Email FROM Users ORDER BY Id";
    
    await foreach (var user in _dbProvider.StreamAsync<User>(query, cancellationToken: cancellationToken))
    {
        yield return user;
    }
}
```

---

## 🌊 Streaming de Datos

### Procesar Grandes Volúmenes

```csharp
public async Task ProcessLargeDatasetAsync(string filePath)
{
    await using var writer = new StreamWriter(filePath);
    
    // Streaming de 1 millón de registros sin cargar todo en memoria
    await foreach (var user in _dbProvider.StreamAsync<User>(
        "SELECT Id, Name, Email FROM Users ORDER BY Id"))
    {
        // Procesar cada usuario individualmente
        await writer.WriteLineAsync($"{user.Id},{user.Name},{user.Email}");
        
        // Pequeña pausa para no sobrecargar
        if (user.Id % 1000 == 0)
        {
            await Task.Delay(1);
        }
    }
}
```

### Streaming con Mapeo Personalizado

```csharp
public async IAsyncEnumerable<UserDto> StreamUserDtosAsync()
{
    string query = @"
        SELECT u.Id, u.Name, u.Email, p.ProfileType 
        FROM Users u 
        LEFT JOIN UserProfiles p ON u.Id = p.UserId";
    
    await foreach (var user in _dbProvider.StreamAsync(
        query,
        reader => new UserDto
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            ProfileType = reader.IsDBNull("ProfileType") ? "Basic" : reader.GetString("ProfileType")
        }))
    {
        yield return user;
    }
}
```

---

## 🔒 Transacciones

### Transacción Simple

```csharp
public async Task<bool> TransferFundsAsync(int fromAccountId, int toAccountId, decimal amount)
{
    try
    {
        await _dbProvider.ExecuteInTransactionAsync(async (transaction) =>
        {
            // Debitar cuenta origen
            await _dbProvider.ExecuteProcedureCommandAsync(
                "sp_DebitAccount",
                parameters => 
                {
                    parameters.AddSqlParameter("AccountId", fromAccountId);
                    parameters.AddSqlParameter("Amount", amount);
                });

            // Acreditar cuenta destino
            await _dbProvider.ExecuteProcedureCommandAsync(
                "sp_CreditAccount",
                parameters => 
                {
                    parameters.AddSqlParameter("AccountId", toAccountId);
                    parameters.AddSqlParameter("Amount", amount);
                });

            return true;
        }, IsolationLevel.ReadCommitted);
        
        return true;
    }
    catch (Exception)
    {
        return false;
    }
}
```

### Transacción con Control de Concurrencia

```csharp
public async Task<OrderResult> ProcessOrderAsync(Order order, List<OrderItem> items)
{
    // El provider controla automáticamente la concurrencia con SemaphoreSlim
    return await _dbProvider.ExecuteInTransactionAsync(async (transaction) =>
    {
        // Validar stock
        foreach (var item in items)
        {
            var stock = await _dbProvider.ExecuteScalarAsync<int>(
                "SELECT Stock FROM Products WHERE ProductId = @ProductId",
                parameters => parameters.AddSqlParameter("ProductId", item.ProductId));
                
            if (stock < item.Quantity)
            {
                throw new InvalidOperationException($"Stock insuficiente para producto {item.ProductId}");
            }
        }
        
        // Crear orden
        var orderId = await _dbProvider.ExecuteScalarAsync<int>(
            "INSERT INTO Orders (CustomerId, TotalAmount) OUTPUT INSERTED.Id VALUES (@CustomerId, @Total)",
            parameters => 
            {
                parameters.AddSqlParameter("CustomerId", order.CustomerId);
                parameters.AddSqlParameter("Total", order.Total);
            });
        
        // Insertar items
        foreach (var item in items)
        {
            await _dbProvider.ExecuteProcedureCommandAsync(
                "sp_AddOrderItem",
                parameters => 
                {
                    parameters.AddSqlParameter("OrderId", orderId);
                    parameters.AddSqlParameter("ProductId", item.ProductId);
                    parameters.AddSqlParameter("Quantity", item.Quantity);
                    parameters.AddSqlParameter("Price", item.Price);
                });
        }
        
        // Actualizar stock
        foreach (var item in items)
        {
            await _dbProvider.ExecuteProcedureCommandAsync(
                "sp_UpdateStock",
                parameters => 
                {
                    parameters.AddSqlParameter("ProductId", item.ProductId);
                    parameters.AddSqlParameter("Quantity", -item.Quantity);
                });
        }
        
        return new OrderResult 
        { 
            Success = true, 
            OrderId = orderId,
            Message = "Orden procesada exitosamente"
        };
    });
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
    await _dbProvider.ExecuteBulkInsertAsync("Users", dataTable);
}
```

### Bulk Insert Configurado

```csharp
public async Task ImportLargeDatasetAsync(List<User> users)
{
    var dataTable = users.ToDataTable();
    
    // Configurar opciones de bulk copy
    var bulkOptions = new SqlBulkCopyOptions
    {
        CheckConstraints = true,
        FireTriggers = false,
        KeepIdentity = true,
        KeepNulls = true,
        TableLock = true
    };
    
    await _dbProvider.ExecuteBulkInsertAsync("Users", dataTable, bulkOptions);
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
            parameters => parameters.AddSqlParameter("Id", userId));
    }
    catch (SqlQueryException ex)
    {
        _logger.LogError(ex, "Error en consulta SQL: {CommandText}", ex.CommandText);
        
        // Excepción con contexto rico
        throw new BusinessException($"Error al obtener usuario {userId}", ex)
        {
            ErrorCode = "USER_NOT_FOUND",
            CorrelationId = ex.CorrelationId
        };
    }
    catch (SqlTimeoutException ex)
    {
        _logger.LogWarning(ex, "Timeout al obtener usuario {UserId}", userId);
        throw new BusinessException("La operación tardó demasiado tiempo", ex)
        {
            ErrorCode = "TIMEOUT",
            RetryAfter = TimeSpan.FromSeconds(5)
        };
    }
    catch (SqlConnectionException ex)
    {
        _logger.LogCritical(ex, "Error de conexión a base de datos");
        throw new BusinessException("No se pudo conectar a la base de datos", ex)
        {
            ErrorCode = "CONNECTION_ERROR"
        };
    }
}
```

### Excepciones con Telemetría

```csharp
public async Task<bool> SafeExecuteAsync(string operation)
{
    try
    {
        await _dbProvider.ExecuteProcedureCommandAsync("sp_ProcessData");
        return true;
    }
    catch (SqlServerProviderException ex)
    {
        // Las excepciones ya incluyen contexto para telemetría
        var telemetryProperties = ex.ToTelemetryProperties();
        
        _telemetry.TrackEvent("DatabaseError", telemetryProperties);
        
        _logger.LogError(ex, "Error en operación {Operation}: {Message}", 
            operation, ex.Message);
            
        return false;
    }
}
```

---

## ⚙️ Configuración Avanzada

### Configuración Completa de Opciones

```csharp
services.ConfigureAndValidateSqlOptions(options =>
{
    // Conexión
    options.ConnectionString = builder.ConnectionString;
    
    // Timeouts
    options.CommandTimeout = 60;
    options.BulkCopy.BulkCopyTimeout = 600;
    options.BulkCopy.BatchSize = 5000;
    
    // Pool de conexiones
    options.MaxPoolSize = 100;
    options.MinPoolSize = 5;
    options.ConnectionTimeout = 30;
    
    // Concurrencia
    options.MaxConcurrentTransactions = 10;
    
    // Telemetría
    options.EnableDiagnosticSource = true;
    options.EnableActivityTracking = true;
});
```

### Validación Personalizada

```csharp
public class CustomSqlOptionsValidator : IValidateOptions<SqlOptions>
{
    public ValidateOptionsResult Validate(string name, SqlOptions options)
    {
        var failures = new List<string>();
        
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            failures.Add("ConnectionString es requerido");
            
        if (options.CommandTimeout <= 0 || options.CommandTimeout > 3600)
            failures.Add("CommandTimeout debe estar entre 1 y 3600 segundos");
            
        if (options.BulkCopy.BatchSize <= 0)
            failures.Add("BulkCopy.BatchSize debe ser mayor a 0");
        
        return failures.Any() 
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

// Registrar validador
services.AddSingleton<IValidateOptions<SqlOptions>, CustomSqlOptionsValidator>();
```

---

## 🎯 Mejores Prácticas

### 1. Usar Siempre Parámetros

```csharp
// ❌ MAL - Vulnerable a SQL Injection
string query = $"SELECT * FROM Users WHERE Name = '{userName}'";

// ✅ BIEN - Seguro con parámetros
string query = "SELECT * FROM Users WHERE Name = @Name";
await _dbProvider.ExecuteQueryAsListAsync<User>(
    query,
    reader => reader.GetEntity<User>(),
    parameters => parameters.AddSqlParameter("Name", userName));
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
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
    
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
            parameters.AddSqlParameter("Offset", offset);
            parameters.AddSqlParameter("PageSize", pageSize);
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

### 5. Manejo Apropiado de Recursos

```csharp
// ✅ BIEN - Usando await using para recursos asíncronos
public async Task ProcessWithStreamingAsync()
{
    await foreach (var item in _dbProvider.StreamAsync<Data>("SELECT * FROM LargeTable"))
    {
        // Procesar item
        await ProcessItemAsync(item);
        
        // Liberar memoria periódicamente
        if (item.Id % 1000 == 0)
        {
            GC.Collect(0, GCCollectionMode.Optimized);
        }
    }
}
```

### 6. Configuración de Timeouts Apropiados

```csharp
// ✅ BIEN - Timeouts configurados según operación
public class DatabaseOperations
{
    private readonly ISQLServerProvider _fastProvider;   // 5s timeout
    private readonly ISQLServerProvider _slowProvider;   // 300s timeout
    
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
            "EXEC sp_GenerateComplexReport");
    }
}
```

---

## � Referencia Rápida de Métodos

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

### Streaming
```csharp
StreamAsync<T>(query, mapper, parameters, timeout, ct)
StreamProcedureAsync<T>(sp, parameters, timeout, ct)
```

### Transacciones
```csharp
ExecuteInTransactionAsync<T>(operation, isolationLevel)
ExecuteInTransaction<T>(operation, isolationLevel)
```

### Bulk Operations
```csharp
ExecuteBulkInsertAsync(table, dataTable, options)
```

---

## 📄 Licencia

Este proyecto está licenciado bajo MIT License. Ver archivo [LICENSE](../LICENSE) para más detalles.

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
