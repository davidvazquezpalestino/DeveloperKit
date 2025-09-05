namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>Reemplaza todos los valores DBNull en el DataTable por el valor predeterminado especificado.</summary>
    public static void ReplaceNulls(this DataTable table, object defaultValue)
    {
        if (table == null)
        {
            return;
        }

        foreach (DataRow row in table.Rows)
        {
            foreach (DataColumn column in table.Columns)
            {
                if (row[column] == DBNull.Value)
                {
                    row[column] = defaultValue;
                }
            }
        }
    }

    /// <summary>Elimina todas las filas que contengan valores nulos o DBNull en cualquier columna.</summary>
    public static DataTable RemoveRowsWithNulls(this DataTable table)
    {
        if (table == null)
        {
            return new DataTable();
        }

        List<DataRow> rowsToRemove = new List<DataRow>();

        foreach (DataRow row in table.Rows)
        {
            bool hasNull = false;
            foreach (object field in row.ItemArray)
            {
                if (field == null || field == DBNull.Value)
                {
                    hasNull = true;
                    break;
                }
            }
            if (hasNull)
            {
                rowsToRemove.Add(row);
            }
        }

        foreach (DataRow row in rowsToRemove)
        {
            table.Rows.Remove(row);
        }

        return table;
    }
}
