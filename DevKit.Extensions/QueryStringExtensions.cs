namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión para trabajar con cadenas de consulta (query strings).</summary>
public static class QueryStringExtensions
{
    /// <summary>Convierte un objeto en un diccionario de cadenas de consulta.</summary>
    public static IDictionary<string, string> BindFrom<T>(T parameters)
    {
        Dictionary<string, string> queryString = new Dictionary<string, string>(StringComparer.Ordinal);

        if (parameters == null)
        {
            return queryString;
        }

        foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead)
            {
                continue;
            }

            object value = property.GetValue(parameters);
            if (value == null)
            {
                continue;
            }

            if (value is DateTime dateTime)
            {
                queryString.Add(property.Name, dateTime.ToString("yyyy-MM-dd"));
                continue;
            }
            if (value is DateTimeOffset dateTimeOffset)
            {
                queryString.Add(property.Name, dateTimeOffset.ToString("yyyy-MM-dd"));
                continue;
            }

            queryString.Add(property.Name, value.ToString());
        }

        return queryString;
    }
}