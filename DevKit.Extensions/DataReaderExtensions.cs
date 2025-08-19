namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión para IDataReader que facilitan la lectura de datos de forma segura.</summary>
public static class DataReaderExtensions
{
    /// <summary>Obtiene el valor tipado de la columna. Si es DBNull retorna default(T) (null para tipos referencia y tipos valor anulables).</summary>
    public static T GetValue<T>(this IDataReader reader, string columnName)
    {
        object value = reader[columnName];
        return value == DBNull.Value ? default : (T)value;
    }

    /// <summary>Mapea los datos del IDataReader a un objeto del tipo especificado.</summary>
    public static T GetItem<T>(this IDataReader reader) where T : new()
    {
        T item = new T();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            PropertyInfo prop = typeof(T).GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop != null && prop.CanWrite && !reader.IsDBNull(i))
            {
                prop.SetValue(item, reader[i]);
            }
        }

        return item;
    }
}