namespace DevKit.ExecutionEngine.Excel.Implementations;

/// <summary>Helper interno para mapear filas de un <see cref="DataTable"/> a objetos POCO por nombre de columna/propiedad.</summary>
internal static class DataTableMapper
{
    /// <summary>Mapea las filas de un <see cref="DataTable"/> a una lista de <typeparamref name="T"/> haciendo coincidir nombres de columna con propiedades públicas escribibles (case-insensitive).</summary>
    public static List<T> MapRowsToItems<T>(DataTable table) where T : new()
    {
        if (table == null)
        {
            return new List<T>();
        }

        Dictionary<string, PropertyInfo> properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        List<(DataColumn Column, PropertyInfo Property)> columns = table.Columns
            .Cast<DataColumn>()
            .Where(c => properties.ContainsKey(c.ColumnName))
            .Select(c => (Column: c, Property: properties[c.ColumnName]))
            .ToList();

        List<T> items = new List<T>(table.Rows.Count);
        foreach (DataRow row in table.Rows)
        {
            T item = new T();
            foreach ((DataColumn column, PropertyInfo property) in columns)
            {
                object raw = row[column];
                if (raw == null || raw == DBNull.Value)
                {
                    continue;
                }
                try
                {
                    Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    object converted = targetType == typeof(string)
                        ? raw.ToString()
                        : Convert.ChangeType(raw, targetType, table.Locale);
                    property.SetValue(item, converted);
                }
                catch (Exception ex)
                {
                    // valor incompatible con la propiedad: se registra y se omite.
                    Console.Error.WriteLine(
                        $"[DataTableMapper] No se pudo mapear columna '{column.ColumnName}' (valor='{raw}', tipo='{raw?.GetType().FullName}') a la propiedad '{typeof(T).Name}.{property.Name}' (tipo='{property.PropertyType.FullName}'): {ex.GetType().Name}: {ex.Message}");
                }
            }
            items.Add(item);
        }
        return items;
    }
}
