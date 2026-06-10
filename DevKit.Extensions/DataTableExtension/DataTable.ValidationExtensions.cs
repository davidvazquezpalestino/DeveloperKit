namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtension
{
    /// <summary>Verifica si el DataTable contiene una columna con el nombre especificado.</summary>
    /// <param name="table">El <see cref="DataTable"/> a verificar.</param>
    /// <param name="columnName">Nombre de la columna a buscar.</param>
    /// <returns>Verdadero si la columna existe; de lo contrario, falso.</returns>
    public static bool HasColumn(this DataTable table, string columnName)
    {
        return table != null && table.Columns.Contains(columnName);
    }

    /// <summary>Indica si la tabla contiene al menos una fila.</summary>
    /// <param name="table">El <see cref="DataTable"/> a verificar.</param>
    /// <returns>Verdadero si tiene al menos una fila; de lo contrario, falso.</returns>
    public static bool HasRows(this DataTable table)
    {
        return table != null && table.Rows.Count > 0;
    }

    /// <summary>Verifica si el DataTable está vacío o es nulo.</summary>
    /// <param name="table">El <see cref="DataTable"/> a verificar.</param>
    /// <returns>Verdadero si es nulo o no tiene filas; de lo contrario, falso.</returns>
    public static bool IsEmpty(this DataTable table)
    {
        return table == null || table.Rows.Count == 0;
    }
}
