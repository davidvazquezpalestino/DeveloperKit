namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>Obtiene el nombre de la tabla con un prefijo '#' para tablas temporales locales.</summary>
    public static string GetTableNameLocal(this DataTable table)
    {
        return $"#{table.TableName}";
    }

    /// <summary>Obtiene el nombre de la tabla con un prefijo '##' para tablas temporales globales.</summary>
    public static string GetTableNameGlobal(this DataTable table)
    {
        return $"##{table.TableName}";
    }
}
