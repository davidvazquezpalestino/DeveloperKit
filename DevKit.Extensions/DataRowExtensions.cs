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
                            propertyInfo.SetValue(item, value);
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
    }
}