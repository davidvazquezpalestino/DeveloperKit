namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="table"></param>
    extension(DataTable table)
    {
        /// <summary>Obtiene el nombre de la tabla con un prefijo '#' para tablas temporales locales.</summary>
        public string GetTableNameLocal()
        {
            return $"#{table.TableName}";
        }

        /// <summary>Obtiene el nombre de la tabla con un prefijo '##' para tablas temporales globales.</summary>
        public string GetTableNameGlobal()
        {
            return $"##{table.TableName}";
        }
    }
}
