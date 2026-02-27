namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión sencillos para <see cref="DataRow" /> y sus colecciones.</summary>
public static class DataRowExtensions
{
    /// <summary>Obtiene el valor tipado de la columna; retorna el valor predeterminado cuando el campo es <see cref="DBNull" />.</summary>
    /// <typeparam name="T">El tipo de dato esperado.</typeparam>
    /// <param name="row">La <see cref="DataRow"/> de origen.</param>
    /// <param name="columnName">El nombre de la columna.</param>
    /// <returns>El valor convertido o default(T).</returns>
    public static T GetValue<T>(this DataRow row, string columnName)
    {
        if (row == null)
        {
            throw new ArgumentNullException(nameof(row));
        }

        try
        {
            object value = row[columnName];
            if (value == DBNull.Value)
            {
                return default;
            }

            return (T)value;
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException)
        {
            throw new InvalidCastException($"No fue posible convertir la columna '{columnName}' al tipo {typeof(T).Name}.", ex);
        }
    }

    /// <summary>Mapea el registro actual hacia una instancia del tipo indicado.</summary>
    /// <typeparam name="T">El tipo de objeto a crear.</typeparam>
    /// <param name="row">La <see cref="DataRow"/> de origen.</param>
    /// <returns>Una nueva instancia de T con los valores poblados.</returns>
    public static T GetItem<T>(this DataRow row) where T : new()
    {
        if (row == null)
        {
            throw new ArgumentNullException(nameof(row));
        }

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