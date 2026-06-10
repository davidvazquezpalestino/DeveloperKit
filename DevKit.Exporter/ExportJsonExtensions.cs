namespace DevKit.Exporter;

/// <summary>
/// Proporciona métodos de extensión para exportar datos a formato JSON.
/// </summary>
public static class ExportJsonExtensions
{
    private static readonly JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    #region DataTable Extensions

    /// <summary>
    /// Convierte el contenido de un <see cref="DataTable"/> a JSON.
    /// </summary>
    /// <param name="table">El <see cref="DataTable"/> a convertir.</param>
    /// <returns>Una cadena JSON que representa el <see cref="DataTable"/>.</returns>
    public static string ExportToJson(this DataTable table)
    {
        if (table == null)
        {
            return null;
        }

        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

        foreach (DataRow row in table.Rows)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (DataColumn column in table.Columns)
            {
                object value = row[column];
                dictionary[column.ColumnName] = value == DBNull.Value ? null : value;
            }
            rows.Add(dictionary);
        }

        return JsonSerializer.Serialize(rows, DefaultJsonOptions);
    }

    /// <summary>
    /// Exporta un <see cref="DataTable"/> a un archivo JSON.
    /// </summary>
    /// <param name="table">El <see cref="DataTable"/> a exportar.</param>
    /// <param name="fileName">La ruta del archivo donde se guardará el JSON.</param>
    public static void ExportToJson(this DataTable table, string fileName)
    {
        if (table == null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        File.WriteAllText(fileName, table.ExportToJson());
    }

    #endregion

    #region Generic & Dictionary Extensions

    /// <summary>
    /// Exporta una colección de objetos a un archivo JSON.
    /// </summary>
    /// <typeparam name="T">El tipo de los objetos en la colección.</typeparam>
    /// <param name="data">La colección de objetos a exportar.</param>
    /// <param name="fileName">La ruta del archivo donde se guardará el JSON.</param>
    public static void ExportToJson<T>(this IEnumerable<T> data, string fileName)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        File.WriteAllText(fileName, data.ExportToJson());
    }

    /// <summary>
    /// Convierte un objeto genérico en una cadena JSON.
    /// </summary>
    /// <typeparam name="T">El tipo del objeto.</typeparam>
    /// <param name="obj">El objeto a convertir.</param>
    /// <param name="options">Opciones de serialización opcionales.</param>
    /// <returns>Una cadena JSON que representa el objeto.</returns>
    public static string ExportToJson<T>(this T obj, JsonSerializerOptions options = null)
    {
        if (obj == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(obj, options ?? DefaultJsonOptions);
    }

    /// <summary>
    /// Convierte una colección de diccionarios en una cadena JSON.
    /// </summary>
    /// <param name="dictionaries">La colección de diccionarios a convertir.</param>
    /// <param name="options">Opciones de serialización opcionales.</param>
    /// <returns>Una cadena JSON que representa la colección.</returns>
    public static string ExportToJson(this IEnumerable<Dictionary<string, object>> dictionaries, JsonSerializerOptions options = null)
    {
        if (dictionaries == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(dictionaries, options ?? DefaultJsonOptions);
    }

    /// <summary>
    /// Convierte un diccionario en una cadena JSON.
    /// </summary>
    /// <param name="dictionary">El diccionario a convertir.</param>
    /// <param name="options">Opciones de serialización opcionales.</param>
    /// <returns>Una cadena JSON que representa el diccionario.</returns>
    public static string ExportToJson(this Dictionary<string, object> dictionary, JsonSerializerOptions options = null)
    {
        if (dictionary == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(dictionary, options ?? DefaultJsonOptions);
    }

    #endregion
}
