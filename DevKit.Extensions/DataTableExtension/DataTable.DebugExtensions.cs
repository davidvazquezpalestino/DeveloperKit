namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="table"></param>
    extension(DataTable table)
    {
        /// <summary>Genera una representación en cadena del esquema del DataTable.</summary>
        public string PrintSchema()
        {
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
        public Dictionary<string, Dictionary<string, double>> GetColumnStatistics()
        {
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
}
