namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtension
{
    /// <summary>
    /// Filtra las filas de un <see cref="DataTable"/> según una condición especificada.
    /// </summary>
    /// <param name="table">Instancia del <see cref="DataTable"/> que se desea filtrar.</param>
    /// <param name="predicate">
    /// Función que define la condición que deben cumplir las filas para ser incluidas
    /// en el resultado. Solo las filas para las que el predicado devuelve <c>true</c>
    /// serán copiadas al nuevo <see cref="DataTable"/>.
    /// </param>
    /// <returns>
    /// Un nuevo <see cref="DataTable"/> que contiene únicamente las filas que cumplen
    /// con la condición especificada.  
    /// Si la tabla de entrada es <c>null</c>, se retorna una instancia vacía.  
    /// Si no existen filas en la tabla, se retorna una copia vacía de su estructura.
    /// </returns>
    /// <remarks>
    /// Este método no modifica la tabla original.  
    /// Internamente utiliza <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
    /// y el tipo de columnas del <see cref="DataTable"/> original.
    /// 
    /// Ejemplo de uso:
    /// <code>
    /// DataTable filtrado = dataTable.Where(row => (string)row["Status"] == "Activo");
    /// </code>
    /// El ejemplo anterior devuelve una nueva tabla con las filas cuyo campo "Status"
    /// tiene el valor "Activo".
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Se lanza si <paramref name="predicate"/> es <c>null</c>.
    /// </exception>
    public static DataTable Where(this DataTable table, Func<DataRow, bool> predicate)
    {
        // Validación más robusta del predicado
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate), "El predicado no puede ser null");
        }

        // Retorna DataTable vacío si la tabla es null
        if (table == null)
        {
            return new DataTable();
        }

        // Optimización: si no hay filas, retornar tabla clonada vacía
        if (table.Rows.Count == 0)
        {
            return table.Clone();
        }

        // Usar CopyToDataTable para mejor rendimiento y mantener la estructura
        EnumerableRowCollection<DataRow> filteredRows = table.AsEnumerable().Where(predicate);
        if (!filteredRows.Any())
        {
            return table.Clone();
        }

        return filteredRows.CopyToDataTable();
    }

    /// <summary>
    /// Elimina del <see cref="DataTable"/> todas las filas que cumplen con el 
    /// predicado especificado.
    /// </summary>
    /// <param name="table">El <see cref="DataTable"/> a modificar.</param>
    /// <param name="predicate">
    /// Función que define la condición que deben cumplir las filas para ser eliminadas.
    /// Si el predicado devuelve <c>true</c>, la fila será eliminada.
    /// </param>
    /// <remarks>
    /// Este método modifica directamente el <paramref name="table"/> original.
    /// Si el <paramref name="predicate"/> coincide con todas las filas, 
    /// el resultado será un <see cref="DataTable"/> vacío.
    /// 
    /// Ejemplo de uso:
    /// <code>
    /// dataTable.RemoveAll(row => (int)row["Age"] == 18);
    /// </code>
    /// El ejemplo anterior elimina todas las filas donde la edad es 18.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Se lanza si <paramref name="table"/> o <paramref name="predicate"/> es <c>null</c>.
    /// </exception>
    public static void RemoveAll(this DataTable table, Func<DataRow, bool> predicate)
    {
        if (table == null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        List<DataRow> rows = table.AsEnumerable().Where(predicate).ToList();
        foreach (DataRow row in rows)
        {
            table.Rows.Remove(row);
        }
    }

    /// <summary>Devuelve un DataTable con valores únicos para la columna especificada.</summary>
    /// <param name="table">El <see cref="DataTable"/> a procesar.</param>
    /// <param name="columnName">Nombre de la columna para obtener valores únicos.</param>
    /// <returns>Un <see cref="DataTable"/> con los valores únicos.</returns>
    public static DataTable Distinct(this DataTable table, string columnName)
    {
        if (table == null)
        {
            return new DataTable();
        }

        return table.Rows.Count != 0
            ? table.DefaultView.ToTable(true, columnName)
            : table.Clone();
    }

    /// <summary>Selecciona columnas específicas de un DataTable, ignorando las inexistentes.</summary>
    /// <param name="table">El <see cref="DataTable"/> a procesar.</param>
    /// <param name="columnNames">Nombres de las columnas a seleccionar.</param>
    /// <returns>Un <see cref="DataTable"/> con las columnas seleccionadas.</returns>
    public static DataTable SelectColumns(this DataTable table, params string[] columnNames)
    {
        if (table == null)
        {
            return new DataTable();
        }

        if (columnNames == null || columnNames.Length == 0)
        {
            return table.Copy();
        }

        string[] existingColumns = columnNames.Where(table.Columns.Contains).ToArray();
        return existingColumns.Length == 0 ? table.Clone() : table.DefaultView.ToTable(false, existingColumns);
    }
}
