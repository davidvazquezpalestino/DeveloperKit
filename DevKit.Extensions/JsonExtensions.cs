namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión para trabajar con JSON.</summary>
public static class JsonExtensions
{
    private static readonly JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Convierte una cadena JSON en un diccionario de string/object.</summary>
    /// <param name="json">La cadena JSON a convertir.</param>
    /// <returns>Un diccionario con los datos del JSON.</returns>
    public static Dictionary<string, object> ToDictionary(this string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, object>();
        }

        JsonObject jsonNode = null;
        try
        {
            jsonNode = JsonNode.Parse(json)?.AsObject();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
        if (jsonNode == null)
        {
            return new Dictionary<string, object>();
        }

        Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, JsonNode> item in jsonNode)
        {
            dictionary[item.Key] = NormalizeNodeValue(item.Value);
        }
        return dictionary;
    }

    /// <summary>Convierte una cadena JSON en un objeto del tipo especificado.</summary>
    /// <typeparam name="T">El tipo del objeto de destino.</typeparam>
    /// <param name="json">La cadena JSON a convertir.</param>
    /// <param name="options">Opciones de serialización opcionales.</param>
    /// <returns>El objeto convertido o default(T) si hay error.</returns>
    public static T ToObject<T>(this string json, JsonSerializerOptions options = null)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default(T);
        }
        try
        {
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultJsonOptions);
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>Convierte una cadena JSON que contiene un array de objetos en una lista de diccionarios.</summary>
    /// <param name="json">La cadena JSON a convertir.</param>
    /// <returns>Una colección de diccionarios.</returns>
    public static IEnumerable<Dictionary<string, object>> ToDictionaryList(this string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Enumerable.Empty<Dictionary<string, object>>();
        }

        JsonArray jsonArray = null;
        try
        {
            jsonArray = JsonNode.Parse(json)?.AsArray();
        }
        catch
        {
            return Enumerable.Empty<Dictionary<string, object>>();
        }
        if (jsonArray == null)
        {
            return Enumerable.Empty<Dictionary<string, object>>();
        }

        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
        foreach (JsonNode item in jsonArray)
        {
            if (item is JsonObject jsonObject)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, JsonNode> prop in jsonObject)
                {
                    dictionary[prop.Key] = NormalizeNodeValue(prop.Value);
                }
                list.Add(dictionary);
            }
        }
        return list;
    }

    /// <summary>Normaliza un JsonNode a un tipo .NET primario cuando sea posible.</summary>
    private static object NormalizeNodeValue(JsonNode node)
    {
        if (node == null)
        {
            return null;
        }
        if (node is JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue(out long longValue))
            {
                return longValue;
            }

            if (jsonValue.TryGetValue(out decimal decimalValue))
            {
                return decimalValue;
            }

            if (jsonValue.TryGetValue(out double doubleValue))
            {
                return doubleValue;
            }

            if (jsonValue.TryGetValue(out bool boolValue))
            {
                return boolValue;
            }

            if (jsonValue.TryGetValue(out DateTimeOffset dtoValue))
            {
                return dtoValue;
            }

            if (jsonValue.TryGetValue(out DateTime dtValue))
            {
                return dtValue;
            }

            if (jsonValue.TryGetValue(out string stringValue))
            {
                return stringValue;
            }

            if (jsonValue.TryGetValue(out object value))
            {
                return value;
            }

            return null;
        }
        // Para objetos o arreglos, devolver la representación JSON cruda
        try
        {
            return node.ToJsonString();
        }
        catch
        {
            return null;
        }
    }
}
