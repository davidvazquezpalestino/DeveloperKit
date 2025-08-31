namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión para DataRow que facilitan el acceso seguro a los datos.</summary>
public static class DataRowExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyCache =
        new();

    /// <summary>Obtiene el valor tipado de la columna. Si es DBNull retorna default(T) (null para tipos referencia y tipos valor anulables).</summary>
    /// <exception cref="InvalidCastException">Se lanza cuando hay un error al convertir el valor de la columna al tipo especificado.</exception>
    /// <exception cref="ArgumentException">Se lanza cuando el nombre de la columna no existe en el DataRow.</exception>
    public static T GetValue<T>(this DataRow row, string columnName)
    {
        try
        {
            object value = row[columnName];
            return value == DBNull.Value ? default : (T)value;
        }
        catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
        {
            object columnValue = row[columnName];
            string valueType = columnValue.GetType().Name;
            string valueString = columnValue.ToString() ?? "null";

            throw new InvalidCastException(
                $"Error al convertir el valor de la columna '{columnName}' al tipo {typeof(T).Name}. " +
                $"Tipo del valor: {valueType}, Valor: {valueString}", ex);
        }
    }

    /// <summary>
    /// Intenta obtener el valor tipado de la columna de forma segura, sin lanzar excepciones.
    /// </summary>
    /// <typeparam name="T">Tipo de dato esperado</typeparam>
    /// <param name="row">Fila de datos</param>
    /// <param name="columnName">Nombre de la columna</param>
    /// <param name="defaultValue">Valor predeterminado a devolver en caso de error (opcional)</param>
    /// <returns>
    /// El valor convertido al tipo especificado si la columna existe y la conversión es exitosa;
    /// de lo contrario, el valor predeterminado del tipo o el valor especificado en defaultValue.
    /// </returns>
    public static T TryGetValue<T>(this DataRow row, string columnName, T defaultValue = default)
    {
        try
        {
            // Verificar si la columna existe
            if (!row.Table.Columns.Contains(columnName))
            {
                return defaultValue;
            }

            // Obtener el valor
            object value = row[columnName];

            // Manejar valores nulos
            if (value == DBNull.Value)
            {
                return defaultValue;
            }

            // Manejar conversión especial para tipos anulables
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

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

            // Conversión estándar para tipos primitivos
            try
            {
                return (T)Convert.ChangeType(value, targetType);
            }
            catch
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
    /// Mapea los datos del DataRow a un objeto del tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de objeto a mapear</typeparam>
    /// <param name="row">Fila de datos</param>
    /// <returns>Instancia del tipo T con los datos del DataRow</returns>
    /// <remarks>
    /// Este método utiliza caché de propiedades para mejorar el rendimiento en llamadas repetidas.
    /// Soporta mapeo de columnas con nombres que difieren en mayúsculas/minúsculas.
    /// Maneja conversiones de tipos y valores nulos de forma segura.
    /// </remarks>
    public static T GetItem<T>(this DataRow row) where T : new()
    {
        if (row == null)
        {
            throw new ArgumentNullException(nameof(row));
        }

        T item = new T();
        Type type = typeof(T);

        // Obtener o crear la caché de propiedades para el tipo T
        if (!PropertyCache.TryGetValue(type, out Dictionary<string, PropertyInfo> properties))
        {
            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .Where(propertyInfo => propertyInfo.CanWrite)
                           .ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo, StringComparer.OrdinalIgnoreCase);

            PropertyCache[type] = properties;
        }

        // Mapear columnas a propiedades
        foreach (DataColumn col in row.Table.Columns)
        {
            string columnName = col.ColumnName;

            // Buscar propiedad que coincida con el nombre de la columna (case-insensitive)
            if (properties.TryGetValue(columnName, out PropertyInfo prop) &&
                row[columnName] != DBNull.Value)
            {
                try
                {
                    object value = row[columnName];

                    // Manejar conversión de tipos
                    Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    object safeValue = Convert.ChangeType(value, propType);
                    prop.SetValue(item, safeValue);
                }
                catch (Exception ex) when (ex is InvalidCastException or FormatException)
                {
                    // Registrar el error y continuar con la siguiente propiedad
                    System.Diagnostics.Debug.WriteLine($"Error al mapear la columna '{columnName}': {ex.Message}");
                }
            }
        }

        return item;
    }
}