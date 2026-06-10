namespace DevKit.Extensions.DataTableExtension;

/// <summary>Proporciona métodos de extensión para trabajar con DataTables de manera más eficiente.</summary>
public static partial class DataTableExtension
{
    /// <summary>Convierte una colección de objetos en un DataTable.</summary>
    /// <typeparam name="TSource">El tipo de los objetos en la colección.</typeparam>
    /// <param name="source">La colección de objetos a convertir.</param>
    /// <param name="tableName">Nombre opcional para el DataTable.</param>
    /// <param name="log">Acción opcional para registrar el progreso.</param>
    /// <returns>Un <see cref="DataTable"/> que contiene los datos de la colección.</returns>
    public static DataTable ToDataTable<TSource>(this IEnumerable<TSource> source, string tableName = null, Action<string> log = null) where TSource : class
    {
        if (log != null)
        {
            log.Invoke($"Iniciando conversión de colección a DataTable. Tipo: {typeof(TSource).Name}");
        }
        GuardNotPrimitiveType<TSource>();

        DataTable dataTable = new DataTable(tableName ?? typeof(TSource).Name);

        // Obtiene todas las propiedades públicas de la clase T
        PropertyInfo[] properties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Crea las columnas en el DataTable basadas en las propiedades
        foreach (PropertyInfo prop in properties)
        {
            if (prop.PropertyType.IsSimpleType())
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
        }

        // Llena el DataTable con los valores de la lista
        foreach (TSource item in source.AsEnumerable())
        {
            DataRow row = dataTable.NewRow();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.PropertyType.IsSimpleType())
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
            }

            dataTable.Rows.Add(row);
        }

        if (log != null)
        {
            log.Invoke($"Conversión completada exitosamente. Filas generadas: {dataTable.Rows.Count}");
        }
        return dataTable;
    }

    /// <summary>Convierte un solo objeto en un DataTable con una sola fila.</summary>
    /// <typeparam name="T">El tipo del objeto.</typeparam>
    /// <param name="item">El objeto a convertir.</param>
    /// <param name="tableName">Nombre opcional para el DataTable.</param>
    /// <param name="log">Acción opcional para registrar el progreso.</param>
    /// <returns>Un <see cref="DataTable"/> que contiene los datos del objeto.</returns>
    public static DataTable ToDataTable<T>(this T item, string tableName = null, Action<string> log = null) where T : class, new()
    {
        if (log != null)
        {
            log.Invoke($"Iniciando conversión de objeto a DataTable. Tipo: {typeof(T).Name}");
        }
        GuardNotPrimitiveType<T>();

        DataTable dataTable = new DataTable(tableName ?? typeof(T).Name);

        // Obtiene todas las propiedades públicas de la clase T
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Crea una columna en el DataTable por cada propiedad de la clase
        foreach (PropertyInfo prop in properties)
        {
            if (prop.PropertyType.IsSimpleType())
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
        }

        // Crea una nueva fila
        DataRow row = dataTable.NewRow();

        // Llena la fila con los valores de las propiedades
        foreach (PropertyInfo prop in properties)
        {
            if (prop.PropertyType.IsSimpleType())
            {
                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            }
        }

        // Agrega la fila al DataTable
        dataTable.Rows.Add(row);

        if (log != null)
        {
            log.Invoke($"Conversión completada exitosamente. Filas generadas: {dataTable.Rows.Count}");
        }
        return dataTable;
    }

    /// <summary>Convierte una cadena JSON que representa un arreglo de objetos simples (con valores primarios como string, number, bool) en un DataTable.</summary>
    /// <param name="json">La cadena JSON a convertir.</param>
    /// <returns>Un <see cref="DataTable"/> que representa los datos JSON.</returns>
    public static DataTable FromJson(string json)
    {
        DataTable table = new DataTable();
        List<Dictionary<string, object>> rawRows = json.ToDictionaryList().ToList();

        if (rawRows.Count == 0)
        {
            return table;
        }

        // Normalizar valores (JsonElement -> .NET, Null/Undefined -> null)
        List<Dictionary<string, object>> normalized = new List<Dictionary<string, object>>(rawRows.Count);
        foreach (Dictionary<string, object> dictionary in rawRows)
        {
            Dictionary<string, object> normalizedDictionary = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> keyValuePair in dictionary)
            {
                normalizedDictionary[keyValuePair.Key] = NormalizeJsonElement(keyValuePair.Value);
            }
            normalized.Add(normalizedDictionary);
        }

        // Determinar tipo por columna recorriendo todas las filas
        Dictionary<string, Type> columnTypes = new Dictionary<string, Type>();
        foreach (string key in normalized[0].Keys)
        {
            Type type = DetermineColumnType(normalized.Select(row => row.ContainsKey(key) ? row[key] : null));
            columnTypes[key] = type ?? typeof(object);
        }

        // Crear columnas
        foreach (KeyValuePair<string, Type> columnType in columnTypes)
        {
            table.Columns.Add(columnType.Key, columnType.Value);
        }

        // Agregar filas
        foreach (Dictionary<string, object> dictionary in normalized)
        {
            DataRow row = table.NewRow();
            foreach (KeyValuePair<string, object> keyValuePair in dictionary)
            {
                row[keyValuePair.Key] = keyValuePair.Value ?? DBNull.Value;
            }
            table.Rows.Add(row);
        }

        return table;
    }
}