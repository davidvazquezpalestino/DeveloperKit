namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión sencillos para <see cref="DataRow" /> y sus colecciones.</summary>
public static class DataRowExtensions
{
    /// <summary>Proporciona métodos de extensión sencillos para <see cref="DataRow" /> y sus colecciones.</summary>
    extension(DataRow row)
    {
        /// <summary>Obtiene el valor tipado de la columna; retorna el valor predeterminado cuando el campo es <see cref="DBNull" />.</summary>
        public T GetValue<T>(string columnName)
        {
            try
            {
                return (T)row[columnName];
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException)
            {
                throw new InvalidCastException($"No fue posible convertir la columna '{columnName}' al tipo {typeof(T).Name}.", ex);
            }
        }

        /// <summary>Obtiene el valor de forma segura o devuelve el valor predeterminado del tipo si la columna no existe o es nula.</summary>
        public T TryGetValue<T>(string columnName)
        {
            try
            {
                object value = row[columnName];
                return value == DBNull.Value ? DefaultValueProvider.GetDefaultValue<T>() : (T)value;
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException)
            {
                throw new InvalidCastException($"No fue posible convertir la columna '{columnName}' al tipo {typeof(T).Name}.", ex);
            }
        }

        /// <summary>Mapea el registro actual hacia una instancia del tipo indicado.</summary>
        public T GetItem<T>() where T : new()
        {
            T item = new T();
            Type itemType = typeof(T);

            foreach (DataColumn column in row.Table.Columns)
            {
                PropertyInfo propertyInfo = itemType.GetProperty(column.ColumnName,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase);

                if (propertyInfo?.CanWrite == true)
                {
                    object value = row[column];

                    if (value != DBNull.Value)
                    {
                        try
                        {
                            object convertedValue = propertyInfo.PropertyType.IsInstanceOfType(value)
                                ? value
                                : DefaultValueProvider.ConvertValue(value, propertyInfo.PropertyType);

                            propertyInfo.SetValue(item, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidCastException($"No fue posible convertir la columna '{column.ColumnName}' al tipo {typeof(T).Name}.", ex);
                        }
                    }
                }
            }

            return item;
        }

        /// <summary>Mapea el registro actual hacia una instancia del tipo indicado, asignando valores predeterminados si son nulos.</summary>
        public T TryGetItem<T>() where T : new()
        {
            T item = new T();
            Type itemType = typeof(T);

            foreach (DataColumn column in row.Table.Columns)
            {
                PropertyInfo property = itemType.GetProperty(column.ColumnName,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase);

                if (property?.CanWrite == true)
                {
                    try
                    {
                        object value = row[column];
                        object convertedValue;
                        if (value == DBNull.Value)
                        {
                            convertedValue = DefaultValueProvider.GetDefaultValue(property.PropertyType);
                        }
                        else
                        {
                            convertedValue = property.PropertyType.IsInstanceOfType(value)
                                ? value
                                : DefaultValueProvider.ConvertValue(value, property.PropertyType);
                        }

                        property.SetValue(item, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidCastException($"No fue posible convertir la columna '{column.ColumnName}' al tipo {typeof(T).Name}.", ex);
                    }
                }
            }
            return item;
        }
    }
}