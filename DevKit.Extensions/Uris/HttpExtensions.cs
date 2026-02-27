
namespace DevKit.Extensions.Uris;

/// <summary>Proporciona métodos de extensión para trabajar con URLs y consultas HTTP.</summary>
public static class HttpExtensions
{
    /// <summary>
    /// Agrega los parámetros de un objeto como cadena de consulta (QueryString) a la URL especificada.
    /// </summary>
    /// <typeparam name="T">El tipo del objeto cuyos parámetros se van a agregar.</typeparam>
    /// <param name="requestUri">La URL base a la que se le agregarán los parámetros.</param>
    /// <param name="item">El objeto cuyas propiedades públicas se convertirán en parámetros de consulta.</param>
    /// <param name="defaultDateFormat">Formato de fecha opcional (por defecto "yyyy-MM-dd").</param>
    /// <returns>La URL original con los parámetros de consulta agregados de forma segura (escapados).</returns>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John", Age = 25 };
    /// string url = "https://api.example.com/users".UrlFromQuery(obj);
    /// // Resultado: https://api.example.com/users?Name=John
    /// </code>
    /// </example>
    public static string UrlFromQuery<T>(this string requestUri, T item, string defaultDateFormat = "yyyy-MM-dd") where T : class
    {
        if (string.IsNullOrEmpty(requestUri))
        {
            throw new ArgumentException("La URL no puede ser nula o vacía.", nameof(requestUri));
        }

        if (item == null)
        {
            return requestUri;
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
            return requestUri;
        }

        // Manejar si la URL ya tiene query string o no
        string separator = requestUri.Contains('?') ? "&" : "?";
        return $"{requestUri}{separator}{string.Join("&", queryParams)}";
    }

    /// <summary>
    /// Reemplaza placeholders {0}, {1}, etc. en una plantilla de URL con los valores proporcionados, escapándolos para su uso seguro en la ruta.
    /// </summary>
    /// <param name="requestUri">La URL con marcadores de posición (plantilla).</param>
    /// <param name="values">Los valores que se insertarán en los marcadores de posición.</param>
    /// <returns>La URL formateada con los valores escapados insertados.</returns>
    /// <exception cref="ArgumentException">Se lanza si la URL es nula o si no se proporcionan valores.</exception>
    /// <exception cref="FormatException">Se lanza si la URL no contiene marcadores de posición válidos.</exception>
    /// <example>
    /// <code>
    /// string url = "api/{0}/{1}".UrlFromRoute("users", "123");
    /// // Resultado: "api/users/123"
    /// </code>
    /// </example>
    public static string UrlFromRoute(this string requestUri, params object[] values)
    {
        if (string.IsNullOrEmpty(requestUri))
        {
            throw new ArgumentException("La URL no puede ser nula o vacía.", nameof(requestUri));
        }

        MatchCollection matches = Regex.Matches(requestUri, @"\{(\d+)\}");
        if (matches.Count == 0)
        {
            throw new FormatException("La URL no contiene ningún marcador {0}, {1}, etc.");
        }

        if (values == null || values.Length == 0)
        {
            throw new ArgumentException("No se proporcionaron parámetros para reemplazar los marcadores.", nameof(values));
        }

        int maxIndex = matches.Cast<Match>().Max(m => int.Parse(m.Groups[1].Value));
        if (maxIndex >= values.Length)
        {
            throw new ArgumentException($"La URL requiere al menos {maxIndex + 1} parámetros, pero solo se recibieron {values.Length}.");
        }


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
                    stringValue = currentValue.ToString();
                    break;
            }

            encodedValues[i] = Uri.EscapeDataString(stringValue);
        }

        return string.Format(CultureInfo.InvariantCulture, requestUri, encodedValues);
    }
}
