using System.Globalization;

namespace DevKit.ExecutionEngine.Excel.Abstractions;

/// <summary>Opciones de configuración para la lectura de archivos CSV.</summary>
public sealed class CsvOptions
{
    /// <summary>Delimitador de campos. Por defecto coma (',').</summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>Carácter usado para entrecomillar campos. Por defecto comillas dobles ('"').</summary>
    public char Quote { get; set; } = '"';

    /// <summary>Indica si la primera fila contiene los nombres de las columnas. Por defecto <c>true</c>.</summary>
    public bool HasHeader { get; set; } = true;

    /// <summary>Codificación de caracteres usada para leer el archivo. Por defecto UTF-8.</summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>Cultura usada para parsear el contenido (ej. para conversiones numéricas). Por defecto <see cref="CultureInfo.InvariantCulture"/>.</summary>
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>Si es <c>true</c>, omite líneas vacías. Por defecto <c>true</c>.</summary>
    public bool SkipEmptyLines { get; set; } = true;

    /// <summary>Recorta espacios en blanco al inicio y al final de cada campo. Por defecto <c>false</c>.</summary>
    public bool TrimFields { get; set; }

    /// <summary>Nombre asignado al <see cref="DataTable"/> resultante cuando no es posible inferirlo del origen.</summary>
    public string DefaultTableName { get; set; } = "Csv";
}
