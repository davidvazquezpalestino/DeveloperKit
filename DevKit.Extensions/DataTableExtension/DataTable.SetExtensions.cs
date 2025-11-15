namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="first"></param>
    extension(DataTable first)
    {
        /// <summary>Devuelve las filas del primer DataTable que no existen en el segundo, comparando por una columna clave.</summary>
        public DataTable Except(DataTable second, string keyColumn)
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
        public DataTable Union(DataTable second)
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
}
