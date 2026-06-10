namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtension
{
    /// <summary>Ordena un DataTable por la columna especificada.</summary>
    /// <param name="table">El <see cref="DataTable"/> a ordenar.</param>
    /// <param name="columnName">Nombre de la columna por la cual ordenar.</param>
    /// <param name="ascending">Indica si el orden es ascendente (verdadero) o descendente (falso).</param>
    /// <returns>Un nuevo <see cref="DataTable"/> ordenado.</returns>
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
