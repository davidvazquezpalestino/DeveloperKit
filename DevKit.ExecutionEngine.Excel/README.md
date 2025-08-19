# ExecutionEngine.Excel

[![NuGet Version](https://img.shields.io/nuget/v/ExecutionEngine.Excel.svg)](https://www.nuget.org/packages/ExecutionEngine.Excel/)
[![.NET Standard](https://img.shields.io/badge/.NET_Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Una biblioteca moderna y eficiente para el manejo de archivos Excel (.xls y .xlsx) en aplicaciones .NET, diseñada para ofrecer un rendimiento óptimo y una API intuitiva tanto para operaciones síncronas como asíncronas.

## Características Principales

- **Soporte multiplataforma**: Compatible con .NET Standard 2.0+ y .NET 6.0+
- **Rendimiento optimizado**: Lectura y escritura eficiente de archivos Excel
- **Soporte asíncrono**: Todas las operaciones principales tienen versiones asíncronas
- **Tipado fuerte**: Conversión automática a tipos fuertemente tipados
- **Soporte para**:
  - Formatos .xls y .xlsx
  - Múltiples hojas de cálculo
  - Operaciones con DataTable
  - Streams para procesamiento sin almacenamiento en disco
  - Inyección de dependencias

## Instalación

Instala el paquete NuGet en tu proyecto:

```bash
dotnet add package ExecutionEngine.Excel
```

O a través del Administrador de paquetes NuGet en Visual Studio:
```
Install-Package ExecutionEngine.Excel
```

## Uso Básico

### Configuración de Inyección de Dependencias

```csharp
// Program.cs / Startup.cs
using DevKit.ExecutionEngine.Excel;

// Registro estándar (Scoped por defecto)
services.AddDotNetCoreExcelPackage();

// Registro keyed (si necesitas múltiples instancias identificadas por clave)
services.AddDotNetCoreExcelPackage("Reporting");
```

Inyección sencilla:

```csharp
public class ReportService
{
    private readonly IExcelPackage _excel;
    public ReportService(IExcelPackage excel) => _excel = excel;
}

// Keyed DI (.NET 8+)
public class ReportingController
{
    private readonly IExcelPackage _excel;
    public ReportingController([FromKeyedServices("Reporting")] IExcelPackage excel) => _excel = excel;
}
```

### Opciones (Options Pattern) — simple y sin cambios de API

Si quieres centralizar valores por defecto (por ejemplo, ruta de archivo por defecto, si la primera fila es encabezado, codificación), puedes usar un POCO con Options Pattern y consumirlo junto con `IExcelPackage`.

appsettings.json:

```json
{
  "ExcelOptions": {
    "DefaultConnectionString": "C:/data/report.xlsx",
    "UseHeaderRow": true,
    "FallbackEncoding": "utf-8"
  }
}
```

POCO de opciones:

```csharp
public class ExcelOptions
{
    public string DefaultConnectionString { get; set; }
    public bool UseHeaderRow { get; set; } = true;
    public string FallbackEncoding { get; set; } = "utf-8";
}
```

Registro en Program.cs / Startup.cs:

```csharp
// Vincular opciones desde configuración
services.Configure<ExcelOptions>(builder.Configuration.GetSection("ExcelOptions"));

// Registrar el paquete de Excel
services.AddDotNetCoreExcelPackage();
```

Uso en un servicio (estableciendo conexión por defecto):

```csharp
public class ExcelReportService
{
    private readonly IExcelPackage _excel;
    private readonly ExcelOptions _options;

    public ExcelReportService(IExcelPackage excel, IOptions<ExcelOptions> options)
    {
        _excel = excel;
        _options = options.Value;

        if (!string.IsNullOrWhiteSpace(_options.DefaultConnectionString))
        {
            _excel.SetDatabaseLogon(_options.DefaultConnectionString);
        }

        // Nota: UseHeaderRow y FallbackEncoding son valores guía; la implementación actual ya usa encabezado y UTF-8 por defecto.
    }
}
```

### Ejemplos de Código

#### Lectura de datos (simple)

```csharp
// Opción A: usando DI (recomendado)
// IExcelPackage excelPackage inyectado en tu servicio/controlador

// Opción B: instancia manual (para ejemplos rápidos)
using DevKit.ExecutionEngine.Excel.Implementations;
var excelPackage = new ExcelPackage();

// Cargar archivo
await excelPackage.SetDatabaseLogonAsync("ruta/al/archivo.xlsx");

// Obtener nombres de hojas
var sheetNames = excelPackage.GetSheetNames();
// o asíncrono
var sheetNamesAsync = await excelPackage.GetSheetNamesAsync();

// Leer datos como DataTable
var dataTable = await excelPackage.GetTableAsync("Hoja1");

// Leer datos como lista fuertemente tipada
var items = await excelPackage.GetItemsAsync<MiModelo>("Hoja1");

// Liberar recursos
excelPackage.Dispose();
```

#### Escritura de datos

```csharp
using var excelPackage = new ExcelPackage();

// Crear una nueva hoja con datos
var data = new List<MiModelo> 
{ 
    new MiModelo { Id = 1, Nombre = "Ejemplo" } 
};

await excelPackage.CreateSheetAsync("MiHoja", data);

// Guardar a archivo
await excelPackage.SaveAsAsync("ruta/guardado.xlsx");
```

#### Uso con Streams

```csharp
using var stream = new MemoryStream();
using var excelPackage = new ExcelPackage();

// Cargar desde stream
await excelPackage.SetDatabaseLogonAsync(stream);

// Procesar datos...

// Guardar a otro stream
using var outputStream = new MemoryStream();
await excelPackage.SaveAsAsync(outputStream);
```

## Modelado de Datos

Puedes mapear tus clases a las hojas de Excel usando atributos:

```csharp
public class Producto
{
    [ExcelColumn("ID", 1)]
    public int Id { get; set; }
    
    [ExcelColumn("NOMBRE", 2)]
    public string Nombre { get; set; }
    
    [ExcelColumn("PRECIO", 3, Format = "C2")]
    public decimal Precio { get; set; }
    
    [ExcelColumn("FECHA_REGISTRO", 4, Format = "dd/MM/yyyy")]
    public DateTime FechaRegistro { get; set; }
}
```

## Manejo de Errores

Todas las operaciones lanzan excepciones específicas que puedes capturar:

```csharp
try
{
    await excelPackage.SetDatabaseLogonAsync("archivo_inexistente.xlsx");
}
catch (ExcelFileNotFoundException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
catch (ExcelException ex)
{
    Console.WriteLine($"Error de Excel: {ex.Message}");
}
```

## Rendimiento

Para archivos grandes, se recomienda:

1. Usar los métodos asíncronos
2. Procesar los datos en lotes
3. Utilizar streams en lugar de operaciones de archivo directas
4. Liberar recursos con `using` o llamando a `Dispose()`

## Contribución

Las contribuciones son bienvenidas. Por favor, lee las [pautas de contribución](CONTRIBUTING.md) antes de enviar un pull request.

## Licencia

Este proyecto está licenciado bajo la licencia MIT. Consulta el archivo [LICENSE](LICENSE) para más detalles.
