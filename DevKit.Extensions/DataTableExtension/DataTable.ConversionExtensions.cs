namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>Convierte un DataTable en una lista de objetos del tipo especificado.</summary>
    public static List<T> ToDataList<T>(this DataTable table) where T : new()
    {
        return table.Rows.Cast<DataRow>()
            .Select(row => row.GetItem<T>())
            .ToList();
    }

    /// <summary>Convierte un DataTable en una lista de diccionarios.</summary>
    public static IEnumerable<Dictionary<string, object>> ToDictionary(this DataTable dataTable)
    {
        return dataTable
            .Rows
            .Cast<DataRow>()
            .Select(row =>
            {
                Dictionary<string, object> dict = new Dictionary<string, object>(dataTable.Columns.Count);
                foreach (DataColumn column in dataTable.Columns)
                {
                    dict[column.ColumnName] = row[column];
                }
                return dict;
            })
            .ToList();
    }
}
