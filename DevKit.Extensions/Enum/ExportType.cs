namespace DevKit.Extensions.Enum;

/// <summary>Define los formatos de exportación disponibles para la generación de archivos.</summary>
public enum ExportType
{
    /// <summary>Formato de texto separado por comas (CSV). Ideal para la exportación de datos tabulares que serán abiertos en hojas de cálculo.</summary>
    Csv = 1,

    /// <summary>Formato JSON (JavaScript Object Notation). Ideal para el intercambio de datos estructurados entre aplicaciones.</summary>
    Json = 2
}