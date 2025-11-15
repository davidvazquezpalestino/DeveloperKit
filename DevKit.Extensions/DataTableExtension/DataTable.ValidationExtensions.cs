namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="table"></param>
    extension(DataTable table)
    {
        /// <summary>Verifica si el DataTable contiene una columna con el nombre especificado.</summary>
        public bool HasColumn(string columnName)
        {
            return table != null && table.Columns.Contains(columnName);
        }

        /// <summary>Indica si la tabla contiene al menos una fila.</summary>
        public bool HasRows()
        {
            if (table == null)
            {
                return false;
            }
            return table.Rows.Count > 0;
        }

        /// <summary>Verifica si el DataTable está vacío o es nulo.</summary>
        public bool IsEmpty()
        {
            return table == null || table.Rows.Count == 0;
        }
    }
}
