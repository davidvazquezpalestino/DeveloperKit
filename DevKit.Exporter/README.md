# Excel Exporter for DeveloperKit

[![NuGet](https://img.shields.io/nuget/v/DevKit.Exporter.svg?style=flat-square)](https://www.nuget.org/packages/DevKit.Exporter/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://dotnet.microsoft.com/)

A powerful and flexible library for exporting data to various file formats with support for generics, advanced formatting, and robust error handling. Part of the DeveloperKit ecosystem, this component provides seamless integration with .NET applications for all your data export needs.

## ✨ Features

- **Multiple Export Formats**
  - Excel (XLS/XLSX) with advanced formatting
  - CSV (Comma-Separated Values)
  - JSON (JavaScript Object Notation)
  - XML (eXtensible Markup Language)

- **Advanced Excel Capabilities**
  - Support for both .xls and .xlsx formats
  - Worksheet management (create, read, update, delete)
  - Cell formatting (styles, colors, fonts, borders)
  - Formulas and calculations
  - Data validation and protection
  - Charts and graphs
  - Conditional formatting

- **Developer Experience**
  - Strongly-typed API with generics support
  - Fluent configuration
  - Async/await support
  - Comprehensive logging
  - Detailed error handling
  - Dependency injection ready

## 🚀 Getting Started

### Prerequisites

- .NET 6.0+ or .NET Standard 2.0+
- Visual Studio 2022+ or JetBrains Rider (recommended)
- For Excel features: Microsoft Excel or compatible viewer (for testing)

### Installation

Install the package via NuGet Package Manager Console:

```bash
dotnet add package DevKit.Exporter
```

Or add the package reference to your `.csproj` file:

```xml
<PackageReference Include="DevKit.Exporter" Version="1.0.0" />
```

## 🔧 Configuration

### Basic Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add Exporter with default settings
    services.AddExporter();
    
    // Register other services
    services.AddControllers();
}
```

### Advanced Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddExporter(options =>
    {
        // Configure default export settings
        options.DefaultExcelSettings = new ExcelExportSettings
        {
            AutoFitColumns = true,
            IncludeHeader = true,
            SheetName = "Export",
            HeaderStyle = new CellStyle
            {
                FontBold = true,
                BackgroundColor = Color.LightGray,
                HorizontalAlignment = HorizontalAlignment.Center
            },
            DataStyle = new CellStyle
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        
        // Configure CSV settings
        options.DefaultCsvSettings = new CsvExportSettings
        {
            Delimiter = ",",
            IncludeHeader = true,
            Encoding = Encoding.UTF8
        };
        
        // Enable detailed logging
        options.EnableLogging = true;
        
        // Configure default date/time formats
        options.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        options.DateFormat = "yyyy-MM-dd";
        options.TimeFormat = "HH:mm:ss";
    });
    
    // Register other services
    services.AddControllers();
}
```

### appsettings.json

```json
{
  "ExporterOptions": {
    "DefaultExcelSettings": {
      "AutoFitColumns": true,
      "IncludeHeader": true,
      "SheetName": "Export",
      "HeaderStyle": {
        "FontBold": true,
        "BackgroundColor": "LightGray",
        "HorizontalAlignment": "Center"
      },
      "DataStyle": {
        "HorizontalAlignment": "Left",
        "VerticalAlignment": "Center"
      }
    },
    "DefaultCsvSettings": {
      "Delimiter": ",",
      "IncludeHeader": true,
      "Encoding": "UTF8"
    },
    "EnableLogging": true,
    "DateTimeFormat": "yyyy-MM-dd HH:mm:ss",
    "DateFormat": "yyyy-MM-dd",
    "TimeFormat": "HH:mm:ss"
  }
}
```
## 💻 Usage Examples

### 1. Basic Excel Export

```csharp
public class ReportService
{
    private readonly IExcelExporter _excelExporter;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IExcelExporter excelExporter, ILogger<ReportService> logger)
    {
        _excelExporter = excelExporter;
        _logger = logger;
    }

    public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Export")
    {
        try
        {
            var options = new ExcelExportOptions<T>
            {
                SheetName = sheetName,
                AutoFitColumns = true,
                IncludeHeader = true,
                HeaderStyle = new CellStyle
                {
                    FontBold = true,
                    BackgroundColor = Color.LightBlue,
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                DataStyle = new CellStyle
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            return await _excelExporter.ExportToByteArrayAsync(data, options);
        }
        catch (ExportException ex)
        {
            _logger.LogError(ex, "Error exporting data to Excel");
            throw;
        }
    }
}
```

### 2. Advanced Excel Export with Multiple Sheets

```csharp
public async Task<byte[]> ExportMultipleSheetsAsync(
    IEnumerable<Customer> customers, 
    IEnumerable<Order> orders)
{
    var workbook = new ExcelWorkbook();
    
    // Add first sheet
    var customerSheet = workbook.AddWorksheet("Customers");
    await customerSheet.WriteDataAsync(customers, new ExcelExportOptions<Customer> 
    { 
        AutoFitColumns = true 
    });
    
    // Add second sheet
    var ordersSheet = workbook.AddWorksheet("Orders");
    await ordersSheet.WriteDataAsync(orders, new ExcelExportOptions<Order>
    {
        AutoFitColumns = true,
        IncludeHeader = true,
        HeaderStyle = new CellStyle { FontBold = true }
    });
    
    // Export to byte array
    using var stream = new MemoryStream();
    await workbook.SaveAsAsync(stream);
    return stream.ToArray();
}
```

### 3. Custom Column Mapping and Formatting

```csharp
public async Task<byte[]> ExportWithCustomMapping(IEnumerable<Product> products)
{
    var options = new ExcelExportOptions<Product>
    {
        SheetName = "Products",
        AutoFitColumns = true,
        IncludeHeader = true,
        ColumnMappings = new List<ColumnMapping<Product>>
        {
            new()
            {
                Property = p => p.Id,
                Header = "ID",
                Width = 10,
                Style = new CellStyle { HorizontalAlignment = HorizontalAlignment.Center }
            },
            new()
            {
                Property = p => p.Name,
                Header = "Product Name",
                Width = 30,
                Style = new CellStyle { FontBold = true }
            },
            new()
            {
                Property = p => p.Price,
                Header = "Price (USD)",
                Format = "$#,##0.00",
                Style = new CellStyle 
                { 
                    NumberFormat = NumberFormat.Currency,
                    HorizontalAlignment = HorizontalAlignment.Right
                }
            },
            new()
            {
                Property = p => p.StockQuantity,
                Header = "In Stock",
                Style = new CellStyle { HorizontalAlignment = HorizontalAlignment.Center },
                ConditionalFormats = new List<ConditionalFormat>
                {
                    new()
                    {
                        Condition = cell => (int)cell.Value < 10,
                        Style = new CellStyle { BackgroundColor = Color.LightPink }
                    },
                    new()
                    {
                        Condition = cell => (int)cell.Value == 0,
                        Style = new CellStyle 
                        { 
                            BackgroundColor = Color.Red, 
                            FontColor = Color.White 
                        }
                    }
                }
            },
            new()
            {
                Property = p => p.LastRestocked,
                Header = "Last Restocked",
                Format = "yyyy-MM-dd",
                Style = new CellStyle { HorizontalAlignment = HorizontalAlignment.Center }
            }
        }
    };

    return await _excelExporter.ExportToByteArrayAsync(products, options);
}
```

### 4. Exporting to CSV

```csharp
public async Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string delimiter = ",")
{
    var options = new CsvExportOptions
    {
        Delimiter = delimiter,
        IncludeHeader = true,
        Encoding = Encoding.UTF8
    };

    return await _csvExporter.ExportToStringAsync(data, options);
}
```

## 📚 API Reference

### Interfaces

#### `IExcelExporter`
Main interface for Excel export operations.

**Methods:**
- `Task<byte[]> ExportToByteArrayAsync<T>(IEnumerable<T> data, ExcelExportOptions<T> options = null)`
- `Task<Stream> ExportToStreamAsync<T>(IEnumerable<T> data, ExcelExportOptions<T> options = null)`
- `Task ExportToFileAsync<T>(string filePath, IEnumerable<T> data, ExcelExportOptions<T> options = null)`
- `Task<ExcelWorkbook> CreateWorkbookAsync<T>(IEnumerable<T> data, ExcelExportOptions<T> options = null)`

#### `ICsvExporter`
Interface for CSV export operations.

**Methods:**
- `Task<string> ExportToStringAsync<T>(IEnumerable<T> data, CsvExportOptions options = null)`
- `Task<byte[]> ExportToByteArrayAsync<T>(IEnumerable<T> data, CsvExportOptions options = null)`
- `Task ExportToFileAsync<T>(string filePath, IEnumerable<T> data, CsvExportOptions options = null)`

### Models

#### `ExcelExportOptions<T>`
Options for exporting data to Excel.

**Properties:**
- `string SheetName` - Name of the worksheet (default: "Sheet1")
- `bool AutoFitColumns` - Whether to auto-fit column widths (default: true)
- `bool IncludeHeader` - Whether to include column headers (default: true)
- `CellStyle HeaderStyle` - Style for header cells
- `CellStyle DataStyle` - Default style for data cells
- `IList<ColumnMapping<T>> ColumnMappings` - Custom column mappings
- `IDictionary<string, string> CustomProperties` - Custom document properties
- `bool FreezeHeader` - Whether to freeze the header row (default: true)
- `bool ShowGridLines` - Whether to show grid lines (default: true)

#### `CsvExportOptions`
Options for exporting data to CSV.

**Properties:**
- `string Delimiter` - Field delimiter (default: ",")
- `bool IncludeHeader` - Whether to include column headers (default: true)
- `Encoding Encoding` - Text encoding (default: UTF-8)
- `string NewLine` - Line terminator (default: Environment.NewLine)
- `bool QuoteAllFields` - Whether to quote all fields (default: false)

## 🔒 Security Considerations

1. **Input Validation**
   - Always validate input data before exporting
   - Sanitize user-provided file names and paths
   - Implement proper error handling for file operations

2. **Data Protection**
   - Be cautious when exporting sensitive data
   - Implement proper access controls
   - Consider encryption for sensitive exports

3. **Performance**
   - Use streaming for large datasets
   - Implement pagination for very large datasets
   - Consider server-side processing for complex exports

## 🚀 Performance Tips

1. **Batch Processing**
   - Process data in batches for large datasets
   - Use streaming when possible to reduce memory usage
   - Consider server-side pagination for web applications

2. **Memory Management**
   - Dispose of resources properly
   - Use `IAsyncDisposable` for async operations
   - Consider using `ArrayPool<T>` for large arrays

3. **Caching**
   - Cache frequently accessed templates
   - Consider caching generated reports when appropriate
   - Implement proper cache invalidation

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
   - Limpiar recursos correctamente
   - Usar rutas seguras

## Ejemplos Avanzados

### Exportación Asíncrona

```csharp
await data.ExportToCsvAsync("archivo.csv");
```

### Exportación con Filtrado

```csharp
var filteredData = data.Where(x => x.IsActive)
    .ExportToCsv("archivo_filtrado.csv");
```

### Exportación con Transformación

```csharp
var transformedData = data.Select(x => new {
    x.Id,
    x.Name,
    FormattedDate = x.CreatedDate.ToString("dd/MM/yyyy")
});

transformedData.ExportToCsv("archivo_transformado.csv");
```

## Soporte y Contribución

Para reportar errores o solicitar características, por favor abre un issue en el repositorio de GitHub.

## Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.
