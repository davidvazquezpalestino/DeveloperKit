namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>Obtiene el nombre de la tabla con un prefijo '#' para tablas temporales locales.</summary>
    /// <param name="table">El <see cref="DataTable"/> de referencia.</param>
    /// <returns>El nombre de la tabla con el prefijo '#'.</returns>
    public static string GetTableNameLocal(this DataTable table)
    {
        if (table == null)
        {
            return "#";
        }

        return $"#{table.TableName}";
    }

    /// <summary>Obtiene el nombre de la tabla con un prefijo '##' para tablas temporales globales.</summary>
    /// <param name="table">El <see cref="DataTable"/> de referencia.</param>
    /// <returns>El nombre de la tabla con el prefijo '##'.</returns>
    public static string GetTableNameGlobal(this DataTable table)
    {
        if (table == null)
        {
            return "##";
        }

        return $"##{table.TableName}";
    }
}
