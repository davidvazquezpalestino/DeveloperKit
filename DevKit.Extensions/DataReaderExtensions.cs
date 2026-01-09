namespace DevKit.Extensions;

/// <summary>
/// Proporciona métodos de extensión sencillos para <see cref="IDataReader" />.
/// </summary>
public static class DataReaderExtensions
{
    /// <param name="reader">El IDataReader actual.</param>
    extension(IDataReader reader)
    {
        /// <summary>
        /// Obtiene el valor tipado de la columna; retorna el valor por defecto del tipo si es <see cref="DBNull" /> o nulo.
        /// </summary>
        /// <typeparam name="T">El tipo de dato esperado.</typeparam>
        /// <param name="columnName">El nombre de la columna.</param>
        /// <returns>El valor convertido o default(T).</returns>
        public T GetValue<T>(string columnName)
        {
            // Validar si existe la columna para evitar IndexOutOfRangeException no controlado
            // Aunque GetOrdinal lanza la excepción si no existe, es el comportamiento esperado.
            int ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
                return default;

            try
            {
                return (T)reader.GetValue(ordinal);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"No fue posible convertir la columna '{columnName}' con valor '{reader.GetValue(ordinal)}' al tipo {typeof(T).Name}.", ex);
            }
        }

        /// <summary>
        /// Mapea el registro actual hacia una instancia del tipo indicado, haciendo coincidir nombres de propiedades con columnas.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto a crear. Debe tener un constructor sin parámetros.</typeparam>
        /// <returns>Una nueva instancia de T con los valores poblados.</returns>
        public T GetItem<T>() where T : new()
        {
            T item = new T();
            Type type = typeof(T);

            // Obtener propiedades escribibles una sola vez es más eficiente,
            // pero para un método de extensión genérico, iteramos las columnas disponibles en el reader
            // para buscar su contraparte en el objeto.

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.IsDBNull(i)) continue;

                string columnName = reader.GetName(i);

                // Buscar propiedad con el mismo nombre (case-insensitive)
                PropertyInfo prop = type.GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                object value = reader.GetValue(i);

                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        object safeValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                        prop.SetValue(item, safeValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidCastException($"No fue posible convertir la columna '{columnName}' con valor '{value}' al tipo {typeof(T).Name}.", ex);
                    }
                }
            }

            return item;
        }
    }
}