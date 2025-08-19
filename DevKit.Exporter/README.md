# DotNet.ExporterToolkit

Una biblioteca poderosa y flexible para exportar datos a diferentes formatos de archivo, con soporte para tipos genéricos y manejo robusto de errores.

## Arquitectura del Proyecto

El proyecto está compuesto por dos bibliotecas principales que trabajan juntas:

1. **CoreExcelPackage**
   - Manejo específico de archivos Excel (.xls y .xlsx)
   - Lectura de datos desde hojas de Excel
   - Conexión a través de string de conexión o stream
   - Conversión de datos a tipos genéricos

2. **DotNet.ExporterToolkit**
   - Exportación a múltiples formatos (CSV, Excel, JSON, XML)
   - Soporte para tipos genéricos
   - Manejo de propiedades de cualquier tipo
   - Formateo y escapado de datos
   - Logging integrado
   - Manejo robusto de errores

## Instalación

Para usar ambas bibliotecas, instala los paquetes NuGet:

```bash
dotnet add package DotNet.ExcelToolkit
dotnet add package DotNet.ExporterToolkit
```

También están disponibles en el Visual Studio Package Manager:

```
Install-Package DotNet.ExcelToolkit
Install-Package DotNet.ExporterToolkit
```

## Características Principales

- Exportación a múltiples formatos:
  - CSV (Comma-Separated Values)
  - Excel (Tabulados)
  - JSON (JavaScript Object Notation)
  - XML (eXtensible Markup Language)

- Características avanzadas:
  - Soporte para tipos genéricos
  - Manejo automático de propiedades de cualquier tipo
  - Formateo especial de fechas y strings
  - Escapado de caracteres especiales
  - Logging integrado
  - Manejo robusto de excepciones
  - Exportación asíncrona
  - Soporte para archivos temporales
  - Validación de datos

## Instalación

El paquete se puede instalar usando NuGet:

```bash
dotnet add package DotNet.ExporterToolkit
```

También está disponible en el Visual Studio Package Manager:

```
Install-Package DotNet.ExporterToolkit
```

## Requisitos

- .NET Core 3.1 o superior
- Visual Studio 2019 o superior

## Uso Básico

### Exportación a CSV

```csharp
var data = new List<MyClass> {
    new MyClass { Id = 1, Name = "John", CreatedDate = DateTime.Now },
    new MyClass { Id = 2, Name = "Jane", CreatedDate = DateTime.Now }
};

data.ExportToCsv("archivo.csv");
```

### Exportación a Excel

```csharp
data.ExportToExcel("archivo.txt", includeHeaders: true);
```

### Exportación a JSON

```csharp
data.ExportToJson("archivo.json");
```

### Exportación a XML

```csharp
data.ExportToXml("archivo.xml");
```

## Configuración Avanzada

### Formateo Personalizado

```csharp
// Formateo de fechas personalizado
var data = new List<MyClass> {
    new MyClass { Id = 1, Name = "John", CreatedDate = DateTime.Now }
};

data.ExportToCsv("archivo.csv", includeHeaders: true)
    .WithDateFormat("dd/MM/yyyy HH:mm:ss")
    .WithNumberFormat("N2");
```

### Manejo de Errores

```csharp
try
{
    data.ExportToCsv("archivo.csv");
}
catch (ExportException ex)
{
    // Manejo de errores específicos
    ExtensionsLogger.LogError(ex, "Error durante la exportación");
}
```

## Mejores Prácticas

1. **Seguridad de Datos**
   - Validar datos antes de exportar
   - Escapar caracteres especiales
   - Manejar nulls correctamente
   - Usar formateo consistente

2. **Rendimiento**
   - Usar exportación asíncrona para grandes volúmenes
   - Implementar buffering cuando sea necesario
   - Manejar memoria eficientemente
   - Usar paginación para conjuntos de datos grandes

3. **Logging**
   - Registrar inicio y fin de exportación
   - Registrar errores específicos
   - Registrar estadísticas de exportación
   - Implementar logging configurable

4. **Manejo de Archivos**
   - Validar permisos de escritura
   - Manejar archivos temporales
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
