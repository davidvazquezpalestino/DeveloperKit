namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="table"></param>
    extension(DataTable table)
    {
        /// <summary>Convierte un DataTable en una lista de objetos del tipo especificado.</summary>
        public List<T> ToDataList<T>() where T : new()
        {
            return table.Rows.Cast<DataRow>()
                .Select(row => row.GetItem<T>())
                .ToList();
        }

        /// <summary>Convierte un DataTable en una lista de diccionarios.</summary>
        public IEnumerable<Dictionary<string, object>> ToDictionary()
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
}
