# 📚 Excel Provider - Guía de Uso Completa

## 🚀 Introducción

El **Excel Provider** es una librería moderna para .NET que proporciona una API limpia, asíncrona y optimizada para interactuar con archivos Excel (.xls y .xlsx). Ofrece soporte completo para operaciones de lectura y escritura con las mejores prácticas de .NET moderno.

### ✅ Características Principales

- **🔄 Async/Await**: Soporte completo para operaciones asíncronas con cancelación
- **🎯 Type-Safe**: Mapeo fuertemente tipado de entidades
- **⚡ Alto Rendimiento**: Lectura y escritura eficiente de archivos Excel
- **📊 Multi-formato**: Soporte para .xls y .xlsx
- **🗄️ Múltiples Hojas**: Gestión de múltiples worksheets
- **🛡️ Seguridad**: Manejo seguro de streams y recursos
- **📊 DataTable**: Soporte completo para operaciones con DataTable
- **🔧 Configuración**: Opciones flexibles y validación

---

## 📋 Tabla de Contenido

1. [Configuración Inicial](#-configuración-inicial)
2. [Operaciones Básicas](#-operaciones-básicas)
3. [Lectura de Datos](#-lectura-de-datos)
4. [Operaciones Asíncronas](#-operaciones-asíncronas)
5. [Manejo de Hojas](#manejo-de-hojas)
6. [Streams y Procesamiento](#streams-y-procesamiento)
7. [Manejo de Errores](#-manejo-de-errores)
8. [Configuración Avanzada](#-configuración-avanzada)
9. [Mejores Prácticas](#-mejores-prácticas)

---

## 🛠️ Configuración Inicial

### 1. Instalación del Paquete

```bash
dotnet add package DevKit.ExecutionEngine.Excel
```

### 2. Configuración en Startup.cs

```csharp
using DevKit.ExecutionEngine.Excel;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configuración básica
        services.AddDotNetCoreExcelPackage();
        
        // O con opciones personalizadas
        services.AddDotNetCoreExcelPackage(options =>
        {
            options.ConfigureExcelReader = reader =>
            {
                reader.FallbackEncoding = Encoding.UTF8;
                reader.LeaveOpen = false;
            };
        });
    }
}
```

### 3. Inyección de Dependencias

```csharp
public class ExcelService
{
    private readonly IExcelProvider _excelProvider;

    public ExcelService(IExcelProvider excelProvider)
    {
        _excelProvider = excelProvider;
    }
}
```

---

## 🔍 Operaciones Básicas

### Lectura desde Archivo

```csharp
// Leer archivo Excel desde ruta
public async Task<List<Product>> GetProductsFromFileAsync(string filePath)
{
    using var excelProvider = new ExcelProvider(filePath);
    
    var products = excelProvider.GetItems<Product>("Products");
    
    return products.ToList();
}
```

### Lectura desde Stream

```csharp
// Leer archivo Excel desde stream
public async Task<List<Product>> GetProductsFromStreamAsync(Stream excelStream)
{
    using var excelProvider = new ExcelProvider(excelStream);
    
    var products = excelProvider.GetItems<Product>("Products");
    
    return products.ToList();
}
```

### Obtener DataTable

```csharp
// Obtener DataTable de una hoja específica
public async Task<DataTable> GetProductsTableAsync(string filePath)
{
    using var excelProvider = new ExcelProvider(filePath);
    
    var table = excelProvider.GetTable("Products");
    
    return table;
}
```

---

## 📊 Lectura de Datos

### Mapeo a Entidades

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public bool Active { get; set; }
}

// Leer y mapear automáticamente
public async Task<List<Product>> GetAllProductsAsync(string filePath)
{
    using var excelProvider = new ExcelProvider(filePath);
    
    var products = excelProvider.GetItems<Product>("Products");
    
    return products.ToList();
}
```

### Lectura con DataTable Manual

```csharp
public async Task<List<Product>> GetProductsManuallyAsync(string filePath)
{
    using var excelProvider = new ExcelProvider(filePath);
    
    DataTable table = excelProvider.GetTable("Products");
    
    var products = new List<Product>();
    
    foreach (DataRow row in table.Rows)
    {
        var product = new Product
        {
            Id = Convert.ToInt32(row["Id"]),
            Name = row["Name"].ToString(),
            Price = Convert.ToDecimal(row["Price"]),
            Category = row["Category"].ToString(),
            Active = Convert.ToBoolean(row["Active"])
        };
        
        products.Add(product);
    }
    
    return products;
}
```

### Obtener Todas las Hojas

```csharp
public async Task<List<string>> GetAllSheetNamesAsync(string filePath)
{
    using var excelProvider = new ExcelProvider(filePath);
    
    var sheetNames = excelProvider.GetSheetNames();
    
    return sheetNames.ToList();
}
```

---

## ⚡ Operaciones Asíncronas

### Lectura Asíncrona desde Archivo

```csharp
public async Task<List<Product>> GetProductsAsync(string filePath)
{
    await using var excelProvider = new ExcelProvider(filePath);
    
    var products = excelProvider.GetItems<Product>("Products");
    
    return products.ToList();
}
```

### Lectura Asíncrona con Cancelación

```csharp
public async Task<List<Product>> GetProductsWithCancellationAsync(
    string filePath, 
    CancellationToken cancellationToken = default)
{
    await using var excelProvider = new ExcelProvider(filePath);
    
    // Simular operación larga con cancelación
    await Task.Delay(100, cancellationToken);
    
    var products = excelProvider.GetItems<Product>("Products");
    
    return products.ToList();
}
```

### Procesamiento Asíncrono de Múltiples Archivos

```csharp
public async Task<List<Product>> ProcessMultipleExcelFilesAsync(
    List<string> filePaths, 
    CancellationToken cancellationToken = default)
{
    var allProducts = new List<Product>();
    
    foreach (var filePath in filePaths)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await using var excelProvider = new ExcelProvider(filePath);
        var products = excelProvider.GetItems<Product>("Products");
        allProducts.AddRange(products);
    }
    
    return allProducts;
}
```

---

## 🗄️ Manejo de Hojas

### Obtener Información de Hojas

```csharp
public async Task<ExcelWorkbookInfo> GetWorkbookInfoAsync(string filePath)
{
    using var excelProvider = new ExcelProvider(filePath);
    
    var sheetNames = excelProvider.GetSheetNames();
    var tables = excelProvider.GetTables();
    
    return new ExcelWorkbookInfo
    {
        FilePath = filePath,
        SheetNames = sheetNames.ToList(),
        TableInfo = tables.Select(t => new SheetInfo
        {
            Name = t.TableName,
            RowCount = t.Rows.Count,
            ColumnCount = t.Columns.Count
        }).ToList()
    };
}
```

### Lectura de Hoja Específica

```csharp
public async Task<List<Customer>> GetCustomersFromSheetAsync(string filePath, string sheetName)
{
    using var excelProvider = new ExcelProvider(filePath);
    
    // El nombre de la hoja debe coincidir exactamente
    var customers = excelProvider.GetItems<Customer>(sheetName);
    
    return customers.ToList();
}
```

### Procesamiento de Múltiples Hojas

```csharp
public async Task<Dictionary<string, List<T>>> ProcessAllSheetsAsync<T>(
    string filePath) where T : new()
{
    var results = new Dictionary<string, List<T>>();
    
    await using var excelProvider = new ExcelProvider(filePath);
    
    var sheetNames = excelProvider.GetSheetNames();
    
    foreach (var sheetName in sheetNames)
    {
        try
        {
            var items = excelProvider.GetItems<T>(sheetName);
            results[sheetName] = items.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing sheet: {SheetName}", sheetName);
            results[sheetName] = new List<T>();
        }
    }
    
    return results;
}
```

---

## 🌊 Streams y Procesamiento

### Lectura desde MemoryStream

```csharp
public async Task<List<Product>> ProcessExcelFromMemoryAsync(byte[] excelBytes)
{
    using var memoryStream = new MemoryStream(excelBytes);
    
    await using var excelProvider = new ExcelProvider(memoryStream);
    
    var products = excelProvider.GetItems<Product>("Products");
    
    return products.ToList();
}
```

### Procesamiento sin Almacenamiento en Disco

```csharp
public async Task<List<Product>> StreamProcessExcelAsync(
    Stream inputStream,
    Func<Product, Product> processor = null)
{
    await using var excelProvider = new ExcelProvider(inputStream);
    
    var products = excelProvider.GetItems<Product>("Products");
    
    if (processor != null)
    {
        return products.Select(processor).ToList();
    }
    
    return products.ToList();
}
```

### Procesamiento de Grandes Archivos

```csharp
public async Task ProcessLargeExcelAsync(
    string filePath,
    IProgress<int> progress = null)
{
    const int batchSize = 1000;
    var processedCount = 0;
    
    await using var excelProvider = new ExcelProvider(filePath);
    
    var table = excelProvider.GetTable("Products");
    var totalRows = table.Rows.Count;
    
    var products = new List<Product>();
    
    for (int i = 0; i < totalRows; i += batchSize)
    {
        var batch = table.Rows.Cast<DataRow>()
                           .Skip(i)
                           .Take(batchSize)
                           .Select(row => new Product
                           {
                               Id = Convert.ToInt32(row["Id"]),
                               Name = row["Name"].ToString(),
                               Price = Convert.ToDecimal(row["Price"]),
                               Category = row["Category"].ToString(),
                               Active = Convert.ToBoolean(row["Active"])
                           });
        
        products.AddRange(batch);
        processedCount += batch.Count;
        
        progress?.Report(processedCount);
        
        // Pequeña pausa para no sobrecargar
        await Task.Delay(1);
    }
    
    return products;
}
```

---

## ⚠️ Manejo de Errores

### Captura de Excepciones Específicas

```csharp
public async Task<List<Product>> SafeGetProductsAsync(string filePath)
{
    try
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"El archivo Excel no existe: {filePath}");
        }
        
        await using var excelProvider = new ExcelProvider(filePath);
        
        var products = excelProvider.GetItems<Product>("Products");
        
        if (!products.Any())
        {
            _logger.LogWarning("No se encontraron productos en el archivo Excel");
        }
        
        return products.ToList();
    }
    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
    {
        _logger.LogError(ex, "Error de acceso al archivo Excel: {FilePath}", filePath);
        throw new BusinessException($"Error al acceder al archivo Excel: {filePath}", ex)
        {
            ErrorCode = "FILE_ACCESS_ERROR"
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error inesperado al procesar archivo Excel: {FilePath}", filePath);
        throw new BusinessException($"Error al procesar archivo Excel: {filePath}", ex)
        {
            ErrorCode = "EXCEL_PROCESSING_ERROR"
        };
    }
}
```

### Validación de Estructura

```csharp
public async Task<bool> ValidateExcelStructureAsync(string filePath, string expectedSheetName)
{
    try
    {
        await using var excelProvider = new ExcelProvider(filePath);
        
        var sheetNames = excelProvider.GetSheetNames();
        
        if (!sheetNames.Contains(expectedSheetName))
        {
            _logger.LogError("Hoja no encontrada: {ExpectedSheetName}. Hojas disponibles: {Sheets}", 
                expectedSheetName, string.Join(", ", sheetNames));
            return false;
        }
        
        var table = excelProvider.GetTable(expectedSheetName);
        
        if (table.Rows.Count == 0)
        {
            _logger.LogWarning("La hoja {SheetName} está vacía", expectedSheetName);
            return false;
        }
        
        if (table.Columns.Count == 0)
        {
            _logger.LogWarning("La hoja {SheetName} no tiene columnas", expectedSheetName);
            return false;
        }
        
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al validar estructura de Excel: {FilePath}", filePath);
        return false;
    }
}
```

---

## ⚙️ Configuración Avanzada

### Configuración Completa de Opciones

```csharp
services.AddDotNetCoreExcelPackage(options =>
{
    // Configuración del lector
    options.ConfigureExcelReader = reader =>
    {
        reader.FallbackEncoding = Encoding.UTF8;
        reader.LeaveOpen = false;
        reader.Password = "password123"; // Para archivos protegidos
        reader.BufferSize = 4096;
    };
    
    // Configuración del escritor (si se usa para escritura)
    options.ConfigureExcelWriter = writer =>
    {
        writer.BufferSize = 4096;
        writer.LeaveOpen = false;
        writer.StylesheetName = "Data";
    };
    
    // Configuración de DataSet
    options.ConfigureExcelDataSet = dataSet =>
    {
        dataSet.UseHeaderRow = true;
        dataSet.ConfigureDataTable = (table) =>
        {
            table.UseHeaderRow = true;
            table.ReadHeaderRow = true;
        };
    };
});
```

### Validación Personalizada

```csharp
public class ExcelFileValidator : IValidateOptions<ExcelOptions>
{
    public ValidateOptionsResult Validate(string name, ExcelOptions options)
    {
        var failures = new List<string>();
        
        if (options.ConfigureExcelReader == null)
            failures.Add("ConfigureExcelReader es requerido");
            
        // Validaciones adicionales según tus necesidades
        if (options.ConfigureExcelReader != null)
        {
            // Ejemplo: Validar tamaño de buffer
            // if (options.ConfigureExcelReader.Invoke(new ExcelReaderConfiguration()).BufferSize < 1024)
            //     failures.Add("BufferSize debe ser al menos 1024");
        }
        
        return failures.Any() 
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

// Registrar validador
services.AddSingleton<IValidateOptions<ExcelOptions>, ExcelFileValidator>();
```

---

## 🎯 Mejores Prácticas

### 1. Usar Siempre Async/Await con Streams

```csharp
// ❌ MAL - Bloquea el hilo principal
public List<Product> GetProductsBlocking(string filePath)
{
    using var excelProvider = new ExcelProvider(filePath);
    return excelProvider.GetItems<Product>("Products").ToList();
}

// ✅ BIEN - Asíncrono y eficiente
public async Task<List<Product>> GetProductsAsync(string filePath)
{
    await using var excelProvider = new ExcelProvider(filePath);
    return excelProvider.GetItems<Product>("Products").ToList();
}
```

### 2. Liberar Recursos Apropiadamente

```csharp
// ❌ MAL - No libera el recurso
public List<Product> BadGetProducts(string filePath)
{
    var excelProvider = new ExcelProvider(filePath);
    return excelProvider.GetItems<Product>("Products").ToList();
}

// ✅ BIEN - Usa using para liberación automática
public async Task<List<Product>> GoodGetProductsAsync(string filePath)
{
    await using var excelProvider = new ExcelProvider(filePath);
    return excelProvider.GetItems<Product>("Products").ToList();
}
```

### 3. Validar Antes de Procesar

```csharp
// ❌ MAL - No valida la estructura
public async Task<List<Product>> ProcessExcelWithoutValidation(string filePath)
{
    await using var excelProvider = new ExcelProvider(filePath);
    return excelProvider.GetItems<Product>("Products").ToList();
}

// ✅ BIEN - Valida antes de procesar
public async Task<List<Product>> ProcessExcelWithValidation(string filePath)
{
    if (!await ValidateExcelStructureAsync(filePath, "Products"))
    {
        throw new InvalidOperationException("Estructura de Excel inválida");
    }
    
    await using var excelProvider = new ExcelProvider(filePath);
    return excelProvider.GetItems<Product>("Products").ToList();
}
```

### 4. Manejar Grandes Archivos Eficientemente

```csharp
// ❌ MAL - Carga todo en memoria
public async Task<List<Product>> LoadAllInMemory(string filePath)
{
    await using var excelProvider = new ExcelProvider(filePath);
    var table = excelProvider.GetTable("Products"); // Todo en memoria
    return table.Rows.Cast<DataRow>()
                 .Select(row => new Product { /* mapeo */ })
                 .ToList();
}

// ✅ BIEN - Procesa en lotes
public async Task<List<Product>> ProcessInBatches(string filePath)
{
    const int batchSize = 1000;
    var results = new List<Product>();
    
    await using var excelProvider = new ExcelProvider(filePath);
    var table = excelProvider.GetTable("Products");
    
    for (int i = 0; i < table.Rows.Count; i += batchSize)
    {
        var batch = table.Rows.Cast<DataRow>()
                           .Skip(i)
                           .Take(batchSize)
                           .Select(row => new Product { /* mapeo */ });
        results.AddRange(batch);
        
        // Pequeña pausa para no sobrecargar
        await Task.Delay(1);
    }
    
    return results;
}
```

### 5. Configuración de Timeouts Apropiados

```csharp
// ✅ BIEN - Timeouts configurados según operación
public class ExcelOperations
{
    private readonly IServiceProvider _serviceProvider;
    
    public async Task<List<Product>> GetSmallFileAsync(string filePath)
    {
        // Archivo pequeño - procesamiento rápido
        await using var excelProvider = _serviceProvider.GetRequiredService<IExcelProvider>();
        excelProvider.SetDatabaseLogon(filePath);
        
        return excelProvider.GetItems<Product>("Products").ToList();
    }
    
    public async Task<List<Product>> GetLargeFileAsync(string filePath)
    {
        // Archivo grande - procesamiento con pausas
        const int batchSize = 500;
        var results = new List<Product>();
        
        await using var excelProvider = _serviceProvider.GetRequiredService<IExcelProvider>();
        excelProvider.SetDatabaseLogon(filePath);
        
        var table = excelProvider.GetTable("Products");
        
        for (int i = 0; i < table.Rows.Count; i += batchSize)
        {
            var batch = table.Rows.Cast<DataRow>()
                           .Skip(i)
                           .Take(batchSize)
                           .Select(row => new Product { /* mapeo */ });
            results.AddRange(batch);
            
            // Pausa para no sobrecargar el sistema
            await Task.Delay(10);
        }
        
        return results;
    }
}
```

---

## 📚 Referencia Rápida de Métodos

### Operaciones de Lectura
```csharp
GetTable(tableName)                           // DataTable
GetItems<T>(tableName)                        // List<T>
GetSheetNames()                              // List<string>
GetTables()                                  // List<DataTable>
```

### Configuración
```csharp
SetDatabaseLogon(connectionString)          // Archivo por ruta
SetDatabaseLogon(stream)                       // Archivo por stream
```

### Ciclo de Vida
```csharp
await using var provider = new ExcelProvider() // Creación y liberación
await provider.DisposeAsync()                   // Liberación asíncrona
provider.Dispose()                              // Liberación síncrona
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
