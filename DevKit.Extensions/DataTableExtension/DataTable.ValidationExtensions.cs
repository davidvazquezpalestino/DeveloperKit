namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>Verifica si el DataTable contiene una columna con el nombre especificado.</summary>
    public static bool HasColumn(this DataTable table, string columnName)
    {
        return table != null && table.Columns.Contains(columnName);
    }

    /// <summary>Indica si la tabla contiene al menos una fila.</summary>
    public static bool HasRows(this DataTable table)
    {
        if (table == null)
        {
            return false;
        }
        return table.Rows.Count > 0;
    }

    /// <summary>Verifica si el DataTable está vacío o es nulo.</summary>
    public static bool IsEmpty(this DataTable table)
    {
        return table == null || table.Rows.Count == 0;
    }
}
