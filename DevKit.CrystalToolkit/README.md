# Crystal Reports Toolkit for DeveloperKit

[![NuGet](https://img.shields.io/nuget/v/DevKit.CrystalToolkit.svg?style=flat-square)](https://www.nuget.org/packages/DevKit.CrystalToolkit/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://dotnet.microsoft.com/)

A comprehensive library for integrating SAP Crystal Reports into .NET applications with a clean, modern API. Part of the DeveloperKit ecosystem, this component simplifies report generation, export, and printing with support for dependency injection and async operations.

## ✨ Features

- **Seamless Integration**
  - Full support for Crystal Reports in .NET applications
  - Modern, dependency-injection ready API
  - Async/await support for all operations
  - Built-in connection string management

- **Multiple Export Formats**
  - PDF, Excel (XLS/XLSX), Word (DOC/DOCX)
  - CSV, RTF, XML, and HTML formats
  - Customizable export options for each format
  - Stream-based output for web scenarios

- **Advanced Features**
  - Report parameter management
  - Built-in report caching
  - Custom data source binding
  - Subreport support
  - Watermarking and security options
  - Page numbering and header/footer customization

- **Performance**
  - Configurable caching mechanisms
  - Connection pooling
  - Resource cleanup
  - Memory-efficient streaming

## 🚀 Getting Started

### Prerequisites

- .NET 6.0+ or .NET Standard 2.0+
- SAP Crystal Reports Runtime for .NET Framework (version 32-bit or 64-bit, matching your application)
- Visual Studio 2019+ with Crystal Reports extension (for report design)
- For .NET Core/.NET 5+: Ensure your project is configured for Windows compatibility

### Installation

```bash
dotnet add package DevKit.CrystalToolkit
```

## 🔧 Configuration

### Basic Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add Crystal Reports with default settings
    services.AddCrystalReports();
    
    // Add controllers and other services
    services.AddControllers();
}
```

### Advanced Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCrystalReports(options =>
    {
        // Required: Path to the directory containing .rpt files
        options.ReportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
        
        // Default export format (default: PDF)
        options.DefaultFormat = ExportFormatType.PortableDocFormat;
        
        // Cache settings
        options.EnableCache = true;
        options.CacheDuration = TimeSpan.FromMinutes(30);
        
        // Connection string settings (optional)
        options.ConnectionString = Configuration.GetConnectionString("ReportDatabase");
        
        // Logging configuration
        options.Logging = new CrystalReportsLoggingOptions
        {
            LogReportExecution = true,
            LogExportOperations = true,
            LogPerformanceMetrics = true
        };
    });
    
    // Register additional services
    services.AddControllers();
}
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "ReportDatabase": "Server=your-server;Database=your-db;User Id=user;Password=password;"
  },
  "CrystalReports": {
    "ReportPath": "Reports",
    "DefaultFormat": "PDF",
    "EnableCache": true,
    "CacheDuration": "00:30:00",
    "Logging": {
      "LogReportExecution": true,
      "LogExportOperations": true,
      "LogPerformanceMetrics": true
    }
  }
}
```


## 💻 Usage Examples

### 1. Basic Report Generation

```csharp
public class ReportService
{
    private readonly IReportManager _reportManager;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IReportManager reportManager, ILogger<ReportService> logger)
    {
        _reportManager = reportManager;
        _logger = logger;
    }

    public async Task<byte[]> GenerateCustomerReportAsync(int customerId, string format = "PDF")
    {
        try
        {
            // Load the report
            var report = await _reportManager.LoadReportAsync("CustomerOrders.rpt");
            
            // Set parameters
            report.SetParameterValue("CustomerID", customerId);
            report.SetParameterValue("ShowDetails", true);
            
            // Export to specified format
            var exportFormat = format.ToUpper() switch
            {
                "EXCEL" => ExportFormatType.Excel,
                "WORD" => ExportFormatType.WordForWindows,
                _ => ExportFormatType.PortableDocFormat
            };
            
            return await _reportManager.ExportToByteArrayAsync(report, exportFormat);
        }
        catch (ReportNotFoundException ex)
        {
            _logger.LogError(ex, "Report not found: {ReportName}", ex.ReportName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report for customer {CustomerId}", customerId);
            throw new ApplicationException("Failed to generate report. Please try again later.", ex);
        }
    }
}
```

### 2. Advanced Report with Data Source

```csharp
public async Task<byte[]> GenerateSalesReportAsync(DateTime startDate, DateTime endDate, int? regionId = null)
{
    using var report = await _reportManager.LoadReportAsync("SalesByRegion.rpt");
    
    // Set report parameters
    report.SetParameterValue("StartDate", startDate);
    report.SetParameterValue("EndDate", endDate);
    
    if (regionId.HasValue)
    {
        report.SetParameterValue("RegionID", regionId.Value);
    }
    
    // Set data source from database
    var salesData = await _salesRepository.GetSalesDataAsync(startDate, endDate, regionId);
    report.SetDataSource(salesData);
    
    // Apply custom formatting
    ApplyReportFormatting(report);
    
    // Export to PDF with options
    var options = new PdfExportOptions
    {
        ExportPageRange = true,
        FirstPageNumber = 1,
        LastPageNumber = 100,
        UsePageRange = true
    };
    
    return await _reportManager.ExportToByteArrayAsync(report, ExportFormatType.PortableDocFormat, options);
}

private void ApplyReportFormatting(ReportDocument report)
{
    // Format currency fields
    foreach (Table table in report.Database.Tables)
    {
        foreach (FieldDefinition field in table.DataDefinition.FormulaFields)
        {
            if (field.ValueType == FieldValueType.CurrencyField)
            {
                field.Text = "ToText({" + field.Name + "}, 'C')";
            }
        }
    }
    
    // Set page settings
    report.PrintOptions.PaperSize = PaperSize.PaperA4;
    report.PrintOptions.PaperOrientation = PaperOrientation.Landscape;
}
```

### 3. Subreports and Multiple Data Sources

```csharp
public async Task<byte[]> GenerateInvoiceReportAsync(int invoiceId)
{
    using var report = await _reportManager.LoadReportAsync("Invoice.rpt");
    
    // Get data from database
    var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
    var invoiceItems = await _invoiceService.GetInvoiceItemsAsync(invoiceId);
    var companyInfo = await _companyService.GetCompanyInfoAsync();
    
    // Set main report data source
    report.SetDataSource(new List<InvoiceViewModel> { invoice });
    
    // Set subreport data sources
    var subreport = report.Subreports["InvoiceItemsSubreport"];
    subreport.SetDataSource(invoiceItems);
    
    var companySubreport = report.Subreports["CompanyHeaderSubreport"];
    companySubreport.SetDataSource(new List<CompanyInfo> { companyInfo });
    
    // Set any additional parameters
    report.SetParameterValue("PrintDate", DateTime.Now);
    report.SetParameterValue("IsDraft", false);
    
    // Apply custom formatting
    ApplyInvoiceFormatting(report);
    
    // Export to PDF
    return await _reportManager.ExportToByteArrayAsync(report);
}
```

### 4. Report Caching

```csharp
public class CachedReportService
{
    private readonly IReportManager _reportManager;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedReportService> _logger;
    
    public CachedReportService(
        IReportManager reportManager, 
        IMemoryCache cache,
        ILogger<CachedReportService> logger)
    {
        _reportManager = reportManager;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<byte[]> GetCachedReportAsync(string reportName, object parameters, TimeSpan? cacheDuration = null)
    {
        var cacheKey = $"report_{reportName}_{parameters.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out byte[] cachedReport))
        {
            _logger.LogDebug("Cache hit for report: {ReportName}", reportName);
            return cachedReport;
        }
        
        _logger.LogDebug("Cache miss for report: {ReportName}. Generating...", reportName);
        
        using var report = await _reportManager.LoadReportAsync(reportName);
        
        // Apply parameters (simplified example)
        if (parameters != null)
        {
            var properties = parameters.GetType().GetProperties();
            foreach (var prop in properties)
            {
                report.SetParameterValue(prop.Name, prop.GetValue(parameters));
            }
        }
        
        var reportBytes = await _reportManager.ExportToByteArrayAsync(report);
        
        // Cache the report
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(cacheDuration ?? TimeSpan.FromMinutes(30));
            
        _cache.Set(cacheKey, reportBytes, options);
        
        return reportBytes;
    }
}
```

## 📚 API Reference

### Interfaces

#### `IReportManager`
Main interface for report management and operations.

**Methods:**
- `Task<ReportDocument> LoadReportAsync(string reportName)` - Loads a report by name
- `Task<byte[]> ExportToByteArrayAsync(ReportDocument report, ExportFormatType format, object options = null)` - Exports a report to a byte array
- `Task<Stream> ExportToStreamAsync(ReportDocument report, ExportFormatType format, object options = null)` - Exports a report to a stream
- `Task PrintAsync(ReportDocument report, PrintOptions options = null)` - Prints a report
- `Task<ReportDocument> CloneReportAsync(ReportDocument report)` - Creates a deep copy of a report

#### `IReportCache`
Interface for report caching.

**Methods:**
- `Task<ReportDocument> GetOrCreateAsync(string cacheKey, Func<Task<ReportDocument>> factory, TimeSpan? expiration = null)`
- `void Remove(string cacheKey)`
- `void Clear()`

### Models

#### `ReportOptions`
Configuration options for the Crystal Reports service.

**Properties:**
- `string ReportPath` - Path to the report files (required)
- `ExportFormatType DefaultFormat` - Default export format (default: PDF)
- `bool EnableCache` - Whether to enable report caching (default: true)
- `TimeSpan CacheDuration` - Default cache duration (default: 30 minutes)
- `string ConnectionString` - Default database connection string
- `ReportLoggingOptions Logging` - Logging configuration

#### `PdfExportOptions`
Options for PDF export.

**Properties:**
- `bool ExportPageRange` - Whether to export a specific page range
- `int FirstPageNumber` - First page to export
- `int LastPageNumber` - Last page to export
- `bool UsePageRange` - Whether to use page range
- `PdfRasterizationQuality RasterizationQuality` - Quality of rasterization

#### `ExcelExportOptions`
Options for Excel export.

**Properties:**
- `bool UseConstantColumnWidth` - Whether to use constant column width
- `bool ExcelTabHasColumnHeadings` - Whether to include column headings
- `bool ExcelShowGridLines` - Whether to show grid lines
- `bool IsOnePagePerSheet` - Whether to use one page per sheet

## 🔒 Security Considerations

1. **Report Security**
   - Validate all report parameters to prevent injection attacks
   - Implement proper authentication and authorization for report access
   - Use parameterized queries for database access

2. **Data Protection**
   - Encrypt sensitive data in reports
   - Implement proper access controls for report files
   - Use row-level security in database queries

3. **Performance**
   - Implement caching for frequently accessed reports
   - Use pagination for large reports
   - Monitor and optimize report queries

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
