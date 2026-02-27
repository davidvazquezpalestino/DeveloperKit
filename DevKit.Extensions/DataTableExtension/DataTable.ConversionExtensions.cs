namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>Convierte un DataTable en una lista de objetos del tipo especificado.</summary>
    /// <typeparam name="T">El tipo de los objetos a los que se convertirá cada fila.</typeparam>
    /// <param name="table">El <see cref="DataTable"/> a convertir.</param>
    /// <returns>Una lista de objetos del tipo <typeparamref name="T"/>.</returns>
    public static List<T> ToDataList<T>(this DataTable table) where T : new()
    {
        return table.Rows.Cast<DataRow>()
            .Select(row => row.GetItem<T>())
            .ToList();
    }

    /// <summary>Convierte un DataTable en una lista de diccionarios.</summary>
    /// <param name="table">El <see cref="DataTable"/> a convertir.</param>
    /// <returns>Una colección de diccionarios donde cada uno representa una fila del DataTable.</returns>
    public static IEnumerable<Dictionary<string, object>> ToDictionary(this DataTable table)
    {
        return table
            .Rows
            .Cast<DataRow>()
            .Select(row =>
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>(table.Columns.Count);
                foreach (DataColumn column in table.Columns)
                {
                    dictionary[column.ColumnName] = row[column];
                }
                return dictionary;
            })
            .ToList();
    }
}
