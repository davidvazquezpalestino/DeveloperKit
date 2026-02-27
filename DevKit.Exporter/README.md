# DevKit.Exporter

Biblioteca potente para la exportación de datos a múltiples formatos, incluyendo Excel (XLS/XLSX), CSV y JSON, mediante métodos de extensión sencillos y eficientes.

## Instalación

Instala el paquete vía NuGet:

```bash
dotnet add package DevKit.Exporter
```

## Uso de Exportadores

Todos los exportadores están implementados como métodos de extensión sobre `DataTable`, `IEnumerable<T>` y `IEnumerable<Dictionary<string, object>>`.

### 1. Exportación a Excel (XLS/XLSX)

Genera archivos de Excel directamente a disco o como `MemoryStream`.

```csharp
// Exportar DataTable a archivo
tabla.ExportToMicrosoftExcel("reporte.xlsx");

// Exportar lista de objetos a MemoryStream
using var stream = miLista.ExportToMicrosoftExcel();

// Con formato de fecha específico
tabla.ExportToMicrosoftExcel("reporte_fecha.xls", DateFormatType.Long);
```

### 2. Exportación a CSV

Generación de archivos planos o streams con delimitadores personalizables.

```csharp
// Exportar a archivo con delimitador por defecto (,)
tabla.ExportToCsv("datos.csv");

// Exportar con delimitador personalizado (;)
miLista.ExportToCsv("datos_europa.csv", delimiter: ";");

// Obtener como MemoryStream
var csvStream = tabla.ExportToCsv(delimiter: "|");
```

### 3. Exportación a JSON

Conversión rápida a cadenas JSON o archivos.

```csharp
// Obtener cadena JSON desde DataTable
string json = tabla.ExportToJson();

// Exportar lista a archivo JSON
miLista.ExportToJson("data.json");

// Obtener cadena JSON desde objeto único (Pretty Print por defecto)
string jsonObj = miObjeto.ToJson();
```

## Características Técnicas

- **Formatos**: Excel (.xls, .xlsx), CSV, JSON.
- **Soporte de Tipos**: `DataTable`, `DataRow`, colecciones genéricas y diccionarios.
- **Manejo de Memoria**: Soporte para `MemoryStream` para integraciones con APIs web.
- **Formateo**: Manejo inteligente de tipos `DateTime` y valores `DBNull`.
