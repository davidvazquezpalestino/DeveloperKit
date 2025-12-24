
using System.Text.RegularExpressions;

namespace DevKit.Extensions.Uris;

/// <summary>Proporciona métodos de extensión para trabajar con URLs y consultas HTTP.</summary>
public static class HttpExtensions
{

    /// <summary>
    /// Agrega los parámetros de un objeto como cadena de consulta a la URL.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="item">El objeto cuyos parámetros agregar.</param>
    /// <param name="defaultDateFormat">Formato de fecha (por defecto "yyyy-MM-dd").</param>
    /// <returns>La URL con los parámetros de consulta agregados.</returns>
    /// <example>
    /// var obj = new { Name = "John", Age = 25 };
    /// string url = obj.AppendQueryString("https://api.example.com/users");
    /// // Resultado: https://api.example.com/users?Name=John&Age=25
    /// </example>
    public static string UrlFormatQuery<T>(string url, T item, string defaultDateFormat = "yyyy-MM-dd") where T : class
    {
        if (item == null)
        {
            return url;
        }

        List<string> queryParams = new();

        foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead)
            {
                continue;
            }

            object value = property.GetValue(item);
            if (value == null)
            {
                continue;
            }

            string stringValue;

            switch (value)
            {
                case DateTime dateTime:
                    stringValue = Uri.EscapeDataString(dateTime.ToString(defaultDateFormat));
                    break;
                case DateTimeOffset dateTimeOffset:
                    stringValue = Uri.EscapeDataString(dateTimeOffset.ToString(defaultDateFormat));
                    break;
                default:
                    stringValue = Uri.EscapeDataString(value.ToString()!);
                    break;
            }

            queryParams.Add($"{Uri.EscapeDataString(property.Name)}={stringValue}");
        }

        if (queryParams.Count == 0)
        {
            return url;
        }

        // Manejar si la URL ya tiene query string o no
        string separator = url.Contains('?') ? "&" : "?";
        return $"{url}{separator}{string.Join("&", queryParams)}";
    }

    /// <summary>
    /// Reemplaza placeholders {0}, {1}, etc. en la plantilla con valores escapados para URL.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="values">Valores a insertar.</param>

    /// <returns>Cadena formateada con valores escapados.</returns>
    /// <example>
    /// string url = "api/{0}/{1}".UrlFormat("users", "123");
    /// // Resultado: "api/users/123"
    /// </example>
    public static string UrlFormat(string url, params object[] values)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("La URL no puede ser nula o vacía.");

        MatchCollection matches = Regex.Matches(url, @"\{(\d+)\}");
        if (matches.Count == 0)
            throw new FormatException("La URL no contiene ningún marcador {0}, {1}, etc.");

        if (values == null || values.Length == 0)
            throw new ArgumentException("No se proporcionaron parámetros para reemplazar los marcadores.");

        int maxIndex = matches.Cast<Match>().Max(m => int.Parse(m.Groups[1].Value));
        if (maxIndex >= values.Length)
            throw new ArgumentException($"La URL requiere al menos {maxIndex + 1} parámetros, pero solo se recibieron {values.Length}.");


        object[] encodedValues = new object[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            object currentValue = values[i];

            if (currentValue == null)
            {
                encodedValues[i] = string.Empty;
                continue;
            }

            string stringValue;

            switch (currentValue)
            {
                case DateTime dateTime:
                    stringValue = dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case DateTimeOffset dateTimeOffset:
                    stringValue = dateTimeOffset.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                default:
                    stringValue = currentValue.ToString() ?? "";
                    break;
            }

            encodedValues[i] = Uri.EscapeDataString(stringValue);
        }

        return string.Format(CultureInfo.InvariantCulture, url, encodedValues);
    }
}
