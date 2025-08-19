# CoreCrystalReports

Biblioteca que proporciona integración con Crystal Reports para la generación de reportes en aplicaciones .NET.

## Características

- Integración completa con Crystal Reports
- Soporte para múltiples formatos de exportación (PDF, Excel, Word, etc.)
- Manejo de parámetros de reporte
- Configuración personalizable
- Integración con inyección de dependencias
- Caché de reportes para mejor rendimiento
- Manejo de excepciones robusto

## Instalación

El componente se puede instalar como un paquete NuGet:

```bash
dotnet add package DeveloperKit.CoreCrystalReports
```

## Requisitos

- Crystal Reports Runtime para .NET Framework
- .NET Framework 4.8 o superior
- Visual Studio 2019 o superior

## Configuración

### Configuración Inicial

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddCrystalReports(options =>
    {
        // Directorio donde se encuentran los archivos .rpt
        options.ReportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
        
        // Formato por defecto para la exportación
        options.DefaultFormat = CrystalReportsFormat.PDF;
        
        // Configuración de caché
        options.EnableCache = true;
        options.CacheDuration = TimeSpan.FromMinutes(30);
    });
}
```

## Uso

### Generación de Reportes Básica

```C#
// Inyección del servicio
private readonly ICrystalReportsService _reportsService;

public MyController(ICrystalReportsService reportsService)
{
    _reportsService = reportsService;
}

// Generar reporte
public async Task<IActionResult> GenerateReport()
{
    var report = await _reportsService.GenerateReportAsync(
        reportName: "SalesReport",  // Nombre del archivo .rpt sin extensión
        parameters: new Dictionary<string, object>
        {
            { "StartDate", DateTime.Now.AddMonths(-1) },
            { "EndDate", DateTime.Now }
        },
        format: CrystalReportsFormat.PDF
    );

    return File(report, "application/pdf", "SalesReport.pdf");
}
```

### Manejo de Errores

```C#
try
{
    var report = await _reportsService.GenerateReportAsync(
        reportName: "SalesReport",
        parameters: parameters
    );
}
catch (CrystalReportsException ex)
{
    // Manejo específico de errores de Crystal Reports
    _logger.LogError(ex, "Error generando reporte");
    throw;
}
```

### Exportación a Diferentes Formatos

```C#
// Exportar a PDF
var pdf = await _reportsService.GenerateReportAsync(
    reportName: "SalesReport",
    format: CrystalReportsFormat.PDF
);

// Exportar a Excel
var excel = await _reportsService.GenerateReportAsync(
    reportName: "SalesReport",
    format: CrystalReportsFormat.Excel
);

// Exportar a Word
var word = await _reportsService.GenerateReportAsync(
    reportName: "SalesReport",
    format: CrystalReportsFormat.Word
);
```

### Configuración Avanzada

```C#
// Configuración de datos
public async Task<IActionResult> CustomReport()
{
    var report = await _reportsService.GenerateReportAsync(
        reportName: "CustomReport",
        dataSource: new List<Customer>
        {
            new Customer { Id = 1, Name = "John Doe" },
            new Customer { Id = 2, Name = "Jane Smith" }
        },
        format: CrystalReportsFormat.PDF
    );

    return File(report, "application/pdf", "CustomReport.pdf");
}
```

## Mejores Prácticas

1. Siempre validar los parámetros antes de generar el reporte
2. Implementar caché para reportes frecuentemente usados
3. Manejar adecuadamente los errores de Crystal Reports
4. Usar nombres descriptivos para los archivos .rpt
5. Documentar los parámetros requeridos en cada reporte

## Soporte

Para reportar errores o solicitar características, por favor abre un issue en el repositorio de GitHub.

## Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.
        parameters: new Dictionary<string, object>
        {
            { "FromDate", DateTime.Now.AddMonths(-1) },
            { "ToDate", DateTime.Now }
        });

    return File(report.Bytes, report.ContentType);
}
```

## Configuración Avanzada

### Formatos de Exportación

```C#
public enum CrystalReportsFormat
{
    PDF,
    Excel,
    Word,
    Html,
    Rtf
}
```

### Manejo de Parámetros

```C#
public class ReportParameters
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ReportTitle { get; set; }
}
```

## Soporte

Para reportar errores o solicitar características, por favor abre un issue en el repositorio principal de DeveloperKit V2.

## Licencia

Este componente está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.
}
