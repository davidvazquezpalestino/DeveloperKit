namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>Filtra un DataTable por el valor de una columna.</summary>
    public static DataTable FilterByColumn(this DataTable table, string columnName, object value)
    {
        if (table == null) return new DataTable();
        IEnumerable<DataRow> filteredRows = table
            .AsEnumerable()
            .Where(row => ObjectsEqual(DbNullToNull(row[columnName]), value));
        return filteredRows.ToDataTable(table);
    }

    /// <summary>Busca filas en el DataTable que cumplan con el predicado especificado.</summary>
    public static IEnumerable<DataRow> FindRows(this DataTable table, Func<DataRow, bool> predicate)
    {
        return table.AsEnumerable().Where(predicate);
    }

    /// <summary>Devuelve un DataTable con valores únicos para la columna especificada.</summary>
    public static DataTable Distinct(this DataTable table, string columnName)
    {
        if (table == null) return new DataTable();
        return table.Rows.Count != 0
            ? table.DefaultView.ToTable(true, columnName)
            : table.Clone();
    }

    /// <summary>Selecciona columnas específicas de un DataTable, ignorando las inexistentes.</summary>
    public static DataTable SelectColumns(this DataTable table, params string[] columnNames)
    {
        if (table == null) return new DataTable();
        if (columnNames == null || columnNames.Length == 0) return table.Copy();
        string[] existingColumns = columnNames.Where(table.Columns.Contains).ToArray();
        return existingColumns.Length == 0 ? table.Clone() : table.DefaultView.ToTable(false, existingColumns);
    }
}
