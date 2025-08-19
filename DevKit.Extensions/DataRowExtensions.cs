namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión para DataRow que facilitan el acceso seguro a los datos.</summary>
public static class DataRowExtensions
{
    /// <summary>Obtiene el valor tipado de la columna. Si es DBNull retorna default(T) (null para tipos referencia y tipos valor anulables).</summary>
    public static T GetValue<T>(this DataRow row, string columnName)
    {
        object value = row[columnName];
        return value == DBNull.Value ? default : (T)value;
    }

    /// <summary>Mapea los datos del DataRow a un objeto del tipo especificado.</summary>
    public static T GetItem<T>(this DataRow row) where T : new()
    {
        T item = new T();

        foreach (DataColumn col in row.Table.Columns)
        {
            PropertyInfo prop = typeof(T).GetProperty(col.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop != null && prop.CanWrite && row[col] != DBNull.Value)
            {
                prop.SetValue(item, row[col]);
            }
        }

        return item;
    }

}