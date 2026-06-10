namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtension
{
    /// <summary>Genera una representación en cadena del esquema del DataTable.</summary>
    /// <param name="table">El <see cref="DataTable"/> a procesar.</param>
    /// <returns>Una cadena con el nombre de la tabla y la lista de sus columnas y tipos.</returns>
    public static string PrintSchema(this DataTable table)
    {
        if (table == null)
        {
            return string.Empty;
        }

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Table: {table.TableName}");
        stringBuilder.AppendLine("Columns:");
        foreach (DataColumn column in table.Columns)
        {
            stringBuilder.AppendLine($"- {column.ColumnName} ({column.DataType.Name})");
        }
        return stringBuilder.ToString();
    }

    /// <summary>Calcula estadísticas (mínimo, máximo, promedio, suma y conteo) para columnas numéricas.</summary>
    /// <param name="table">El <see cref="DataTable"/> a procesar.</param>
    /// <returns>Un diccionario con las estadísticas por columna numérica.</returns>
    public static Dictionary<string, Dictionary<string, double>> GetColumnStatistics(this DataTable table)
    {
        if (table == null)
        {
            return new Dictionary<string, Dictionary<string, double>>();
        }

        Dictionary<string, Dictionary<string, double>> stats = new Dictionary<string, Dictionary<string, double>>();

        foreach (DataColumn col in table.Columns)
        {
            if (col.DataType == typeof(int) ||
                col.DataType == typeof(double) ||
                col.DataType == typeof(decimal) ||
                col.DataType == typeof(float))
            {
                List<double> values = table.AsEnumerable()
                    .Select(row => Convert.ToDouble(row[col]))
                    .Where(val => !double.IsNaN(val))
                    .ToList();

                if (values.Any())
                {
                    stats[col.ColumnName] = new Dictionary<string, double>
                    {
                        { "Min", values.Min() },
                        { "Max", values.Max() },
                        { "Average", values.Average() },
                        { "Sum", values.Sum() },
                        { "Count", values.Count }
                    };
                }
            }
        }

        return stats;
    }
}
