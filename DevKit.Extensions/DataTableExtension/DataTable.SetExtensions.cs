namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtension
{
    /// <summary>Devuelve las filas del primer DataTable que no existen en el segundo, comparando por una columna clave.</summary>
    /// <param name="first">El <see cref="DataTable"/> principal.</param>
    /// <param name="second">El <see cref="DataTable"/> para comparar.</param>
    /// <param name="keyColumn">Nombre de la columna clave para la comparación.</param>
    /// <returns>Un <see cref="DataTable"/> con las filas que solo están en el primer objeto.</returns>
    public static DataTable Except(this DataTable first, DataTable second, string keyColumn)
    {
        if (first == null)
        {
            return new DataTable();
        }

        IEnumerable<DataRow> rows = first.AsEnumerable()
            .Where(row => second == null || !second.AsEnumerable()
                .Any(otherRow => ObjectsEqual(DbNullToNull(row[keyColumn]), DbNullToNull(otherRow[keyColumn]))));
        return rows.ToDataTableOrEmpty(first);
    }

    /// <summary>Combina dos DataTables en uno solo, eliminando duplicados.</summary>
    /// <param name="first">El primer <see cref="DataTable"/>.</param>
    /// <param name="second">El segundo <see cref="DataTable"/>.</param>
    /// <returns>Un <see cref="DataTable"/> con la unión de ambos.</returns>
    public static DataTable Union(this DataTable first, DataTable second)
    {
        DataTable result = first?.Clone() ?? second?.Clone() ?? new DataTable();
        if (first != null)
        {
            result.Merge(first, true, MissingSchemaAction.Add);
        }

        if (second != null)
        {
            result.Merge(second, true, MissingSchemaAction.Add);
        }

        string[] columnNames = result.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
        if (columnNames.Length == 0)
        {
            return result;
        }

        return result.DefaultView.ToTable(true, columnNames);
    }
}
