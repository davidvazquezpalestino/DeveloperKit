namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Proporciona métodos de extensión para optimizar el mapeo de entidades usando caché de propiedades.
/// </summary>
public static class EntityMapperExtensions
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    /// <summary>
    /// Crea una entidad a partir de un IDataReader usando mapeo optimizado con caché de propiedades.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a crear.</typeparam>
    /// <param name="reader">DataReader con los datos.</param>
    /// <returns>Entidad mapeada.</returns>
    public static T GetEntity<T>(this IDataReader reader) where T : class, new()
    {
        var item = new T();
        PropertyInfo[] properties = _propertyCache.GetOrAdd(typeof(T), GetWritableProperties);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            PropertyInfo property = properties.FirstOrDefault(p =>
                string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            if (property != null && !reader.IsDBNull(i))
            {
                object value = reader[i];
                if (value != null && value != DBNull.Value)
                {
                    property.SetValue(item, value.ConvertToType(property.PropertyType));
                }
            }
        }

        return item;
    }

    /// <summary>
    /// Crea una entidad a partir de un DbDataReader de forma asíncrona usando mapeo optimizado.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a crear.</typeparam>
    /// <param name="reader">DataReader con los datos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Entidad mapeada.</returns>
    public static async ValueTask<T> GetEntityAsync<T>(this DbDataReader reader, CancellationToken cancellationToken = default) where T : class, new()
    {
        var item = new T();
        PropertyInfo[] properties = _propertyCache.GetOrAdd(typeof(T), GetWritableProperties);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            PropertyInfo property = properties.FirstOrDefault(p =>
                string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            if (property != null && !await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(false))
            {
                object value = reader[i];
                if (value != null && value != DBNull.Value)
                {
                    property.SetValue(item, value.ConvertToType(property.PropertyType));
                }
            }
        }

        return item;
    }

    private static PropertyInfo[] GetWritableProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                   .ToArray();
    }
}
