namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>Ordena un DataTable por la columna especificada.</summary>
    public static DataTable OrderBy(this DataTable table, string columnName, bool ascending = true)
    {
        if (table == null)
        {
            return new DataTable();
        }

        IEnumerable<DataRow> orderedRows = ascending
            ? table.AsEnumerable().OrderBy(row => row[columnName])
            : table.AsEnumerable().OrderByDescending(row => row[columnName]);
        return orderedRows.ToDataTableOrEmpty(table);
    }
}
