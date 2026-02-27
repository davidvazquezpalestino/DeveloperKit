namespace DevKit.Exporter;

/// <summary>
/// Proporciona métodos de extensión para exportar datos a formato CSV.
/// </summary>
public static class ExportCsvExtensions
{
    /// <summary>
    /// Delimitador predeterminado para archivos CSV.
    /// </summary>
    public const string DefaultDelimiter = ",";

    #region DataTable Extensions

    /// <summary>
    /// Exporta un <see cref="DataTable"/> a un archivo CSV.
    /// </summary>
    /// <param name="table">El <see cref="DataTable"/> a exportar.</param>
    /// <param name="fileName">La ruta del archivo donde se guardará el CSV.</param>
    /// <param name="delimiter">El delimitador a utilizar (por defecto es <see cref="DefaultDelimiter"/>).</param>
    public static void ExportToCsv(this DataTable table, string fileName, string delimiter = DefaultDelimiter)
    {
        if (table == null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        using (StreamWriter writer = new StreamWriter(fileName))
        {
            writer.Write(ToCsvContent(table, delimiter));
        }
    }

    /// <summary>
    /// Exporta un <see cref="DataTable"/> a un <see cref="MemoryStream"/> en formato CSV.
    /// </summary>
    /// <param name="table">El <see cref="DataTable"/> a exportar.</param>
    /// <param name="delimiter">El delimitador a utilizar (por defecto es <see cref="DefaultDelimiter"/>).</param>
    /// <returns>Un <see cref="MemoryStream"/> que contiene el CSV.</returns>
    public static MemoryStream ExportToCsv(this DataTable table, string delimiter = DefaultDelimiter)
    {
        if (table == null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        MemoryStream memoryStream = new MemoryStream();
        StreamWriter writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true);
        writer.Write(ToCsvContent(table, delimiter));
        writer.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    private static string ToCsvContent(DataTable table, string delimiter)
    {
        StringBuilder sb = new StringBuilder();

        // Encabezados
        sb.AppendLine(string.Join(delimiter, table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));

        // Filas
        foreach (DataRow row in table.Rows)
        {
            sb.AppendLine(string.Join(delimiter, row.ItemArray.Select(field => FormatCsvValue(field))));
        }

        return sb.ToString();
    }

    #endregion

    #region Dictionary Extensions

    /// <summary>
    /// Exporta una colección de diccionarios a un archivo CSV.
    /// </summary>
    /// <param name="data">La colección de diccionarios a exportar.</param>
    /// <param name="fileName">La ruta del archivo donde se guardará el CSV.</param>
    /// <param name="delimiter">El delimitador a utilizar (por defecto es <see cref="DefaultDelimiter"/>).</param>
    public static void ExportToCsv(this IEnumerable<Dictionary<string, object>> data, string fileName, string delimiter = DefaultDelimiter)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        using (StreamWriter writer = new StreamWriter(fileName))
        {
            writer.Write(ToCsvContent(data, delimiter));
        }
    }

    /// <summary>
    /// Exporta una colección de diccionarios a un <see cref="MemoryStream"/> en formato CSV.
    /// </summary>
    /// <param name="data">La colección de diccionarios a exportar.</param>
    /// <param name="delimiter">El delimitador a utilizar (por defecto es <see cref="DefaultDelimiter"/>).</param>
    /// <returns>Un <see cref="MemoryStream"/> que contiene el CSV.</returns>
    public static MemoryStream ExportToCsv(this IEnumerable<Dictionary<string, object>> data, string delimiter = DefaultDelimiter)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        MemoryStream memoryStream = new MemoryStream();
        StreamWriter writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true);
        writer.Write(ToCsvContent(data, delimiter));
        writer.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    private static string ToCsvContent(IEnumerable<Dictionary<string, object>> data, string delimiter)
    {
        List<Dictionary<string, object>> items = data.ToList();
        if (!items.Any())
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();
        List<string> keys = items.First().Keys.ToList();

        // Encabezados
        sb.AppendLine(string.Join(delimiter, keys));

        // Filas
        foreach (Dictionary<string, object> item in items)
        {
            sb.AppendLine(string.Join(delimiter, keys.Select(key => FormatCsvValue(item[key]))));
        }

        return sb.ToString();
    }

    #endregion

    #region Generic Collection Extensions

    /// <summary>
    /// Exporta una colección de objetos a un archivo CSV.
    /// </summary>
    /// <typeparam name="T">El tipo de los objetos en la colección.</typeparam>
    /// <param name="data">La colección de objetos a exportar.</param>
    /// <param name="fileName">La ruta del archivo donde se guardará el CSV.</param>
    /// <param name="delimiter">El delimitador a utilizar (por defecto es <see cref="DefaultDelimiter"/>).</param>
    public static void ExportToCsv<T>(this IEnumerable<T> data, string fileName, string delimiter = DefaultDelimiter)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        using (StreamWriter writer = new StreamWriter(fileName))
        {
            writer.Write(ToCsvContent(data, delimiter));
        }
    }

    /// <summary>
    /// Exporta una colección de objetos a un <see cref="MemoryStream"/> en formato CSV.
    /// </summary>
    /// <typeparam name="T">El tipo de los objetos en la colección.</typeparam>
    /// <param name="data">La colección de objetos a exportar.</param>
    /// <param name="delimiter">El delimitador a utilizar (por defecto es <see cref="DefaultDelimiter"/>).</param>
    /// <returns>Un <see cref="MemoryStream"/> que contiene el CSV.</returns>
    public static MemoryStream ExportToCsv<T>(this IEnumerable<T> data, string delimiter = DefaultDelimiter)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        MemoryStream memoryStream = new MemoryStream();
        StreamWriter writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true);
        writer.Write(ToCsvContent(data, delimiter));
        writer.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
    private static string ToCsvContent<T>(IEnumerable<T> data, string delimiter)
    {
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        StringBuilder sb = new StringBuilder();

        // Encabezados
        sb.AppendLine(string.Join(delimiter, properties.Select(p => p.Name)));

        // Filas
        foreach (T item in data)
        {
            sb.AppendLine(string.Join(delimiter, properties.Select(p => FormatCsvValue(p.GetValue(item)))));
        }

        return sb.ToString();
    }

    #endregion

    /// <summary>
    /// Formatea un valor para ser incluido en un archivo CSV.
    /// </summary>
    private static string FormatCsvValue(object value)
    {
        if (value == null || value == DBNull.Value)
        {
            return string.Empty;
        }

        string text = value.ToString();

        // Escapar comillas dobles
        if (text.Contains("\"") || text.Contains(",") || text.Contains("\n") || text.Contains("\r"))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }

        return text;
    }
}
