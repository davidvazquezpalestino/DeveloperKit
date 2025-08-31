namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión para IDataReader que facilitan la lectura de datos de forma segura.</summary>
public static class DataReaderExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyCache =
        new();

    /// <summary>Obtiene el valor tipado de la columna. Si es DBNull retorna default(T) (null para tipos referencia y tipos valor anulables).</summary>
    /// <exception cref="InvalidCastException">Se lanza cuando hay un error al convertir el valor de la columna al tipo especificado.</exception>
    /// <exception cref="IndexOutOfRangeException">Se lanza cuando el nombre de la columna no existe en el lector.</exception>
    public static T GetValue<T>(this IDataReader reader, string columnName)
    {
        try
        {
            object value = reader[columnName];
            return value == DBNull.Value ? default : (T)value;
        }
        catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
        {
            object columnValue = reader[columnName];
            string valueType = columnValue?.GetType().Name ?? "null";
            string valueString = columnValue?.ToString() ?? "null";

            throw new InvalidCastException(
                $"Error al convertir el valor de la columna '{columnName}' al tipo {typeof(T).Name}. " +
                $"Tipo del valor: {valueType}, Valor: {valueString}",
                ex);
        }
    }

    /// <summary>
    /// Intenta obtener el valor tipado de la columna de forma segura, sin lanzar excepciones.
    /// </summary>
    /// <typeparam name="T">Tipo de dato esperado</typeparam>
    /// <param name="reader">Lector de datos</param>
    /// <param name="columnName">Nombre de la columna</param>
    /// <param name="defaultValue">Valor predeterminado a devolver en caso de error (opcional)</param>
    /// <returns>
    /// El valor convertido al tipo especificado si la columna existe y la conversión es exitosa;
    /// de lo contrario, el valor predeterminado del tipo o el valor especificado en defaultValue.
    /// </returns>
    public static T TryGetValue<T>(this IDataReader reader, string columnName, T defaultValue = default)
    {
        try
        {
            // Verificar si la columna existe
            int columnIndex = -1;
            try
            {
                columnIndex = reader.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException)
            {
                return defaultValue;
            }

            // Verificar si el valor es nulo
            if (reader.IsDBNull(columnIndex))
            {
                return defaultValue;
            }

            // Obtener y convertir el valor
            object value = reader[columnIndex];

            // Manejar conversión especial para tipos anulables
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            try
            {
                // Si es un enum
                if (targetType.IsEnum)
                {
                    try
                    {
                        if (value is string strValue)
                        {
                            return (T)Enum.Parse(targetType, strValue, true);
                        }
                        return (T)value;
                    }
                    catch { return defaultValue; }
                }

                // Para tipos anulables, manejar el caso especial cuando el valor es nulo
                if (value == DBNull.Value)
                {
                    return defaultValue;
                }

                // Conversión estándar para tipos primitivos
                return (T)Convert.ChangeType(value, targetType);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Mapea los datos del IDataReader a un objeto del tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de objeto a mapear</typeparam>
    /// <param name="reader">Lector de datos</param>
    /// <returns>Instancia del tipo T con los datos del lector</returns>
    /// <remarks>
    /// Este método utiliza caché de propiedades para mejorar el rendimiento en llamadas repetidas.
    /// Soporta mapeo de columnas con nombres que difieren en mayúsculas/minúsculas.
    /// Maneja conversiones de tipos y valores nulos de forma segura.
    /// </remarks>
    public static T GetItem<T>(this IDataReader reader) where T : new()
    {
        T item = new T();
        Type type = typeof(T);

        // Obtener o crear la caché de propiedades para el tipo T
        if (!PropertyCache.TryGetValue(type, out Dictionary<string, PropertyInfo> properties))
        {
            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanWrite)
                            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            PropertyCache[type] = properties;
        }

        // Mapear columnas a propiedades
        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);

            // Buscar propiedad que coincida con el nombre de la columna (case-insensitive)
            if (properties.TryGetValue(columnName, out PropertyInfo prop) && !reader.IsDBNull(i))
            {
                try
                {
                    object value = reader[i];

                    // Manejar conversión de tipos
                    if (value != DBNull.Value)
                    {
                        Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        object safeValue = Convert.ChangeType(value, propType);
                        prop.SetValue(item, safeValue);
                    }
                }
                catch (Exception ex) when (ex is InvalidCastException or FormatException)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al mapear la columna '{columnName}': {ex.Message}");
                }
            }
        }

        return item;
    }
}