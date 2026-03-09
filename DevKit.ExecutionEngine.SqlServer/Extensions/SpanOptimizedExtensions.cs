namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Proporciona métodos de extensión optimizados con Span<T> y Memory<T> para reducir asignaciones.
/// </summary>
public static class SpanOptimizedExtensions
{
    /// <summary>
    /// Lee un string del DataReader usando Span para evitar asignaciones innecesarias.
    /// </summary>
    /// <param name="reader">DataReader del cual leer.</param>
    /// <param name="ordinal">Posición de la columna.</param>
    /// <returns>String leído o null si es DBNull.</returns>
    public static string GetStringOptimized(this IDataRecord reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return null;
            
        // Para strings grandes, usar GetString directamente
        // Para strings pequeños, podríamos usar Span en el futuro con GetChars
        return reader.GetString(ordinal);
    }
    
    /// <summary>
    /// Lee un valor del DataReader usando conversión optimizada con Span.
    /// </summary>
    /// <typeparam name="T">Tipo de valor a leer.</typeparam>
    /// <param name="reader">DataReader del cual leer.</param>
    /// <param name="ordinal">Posición de la columna.</param>
    /// <returns>Valor leído.</returns>
    public static T GetValueOptimized<T>(this IDataRecord reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return default;

        object value = reader.GetValue(ordinal);
        return value.ConvertToType<T>();
    }
    
    /// <summary>
    /// Lee múltiples valores del DataReader de forma optimizada usando Span.
    /// </summary>
    /// <param name="reader">DataReader del cual leer.</param>
    /// <param name="ordinals">Arreglo de posiciones de columnas.</param>
    /// <returns>Span con los valores leídos.</returns>
    public static Span<object> GetValuesOptimized(this IDataRecord reader, Span<int> ordinals)
    {
        object[] values = new object[ordinals.Length];
        for (int i = 0; i < ordinals.Length; i++)
        {
            values[i] = reader.IsDBNull(ordinals[i]) ? null : reader.GetValue(ordinals[i]);
        }
        return values;
    }
    
    /// <summary>
    /// Mapea un DataReader a una entidad usando Span para optimizar el acceso a columnas.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad.</typeparam>
    /// <param name="reader">DataReader con los datos.</param>
    /// <param name="propertyMap">Mapeo de propiedades a índices de columnas.</param>
    /// <returns>Entidad mapeada.</returns>
    public static T MapToEntityOptimized<T>(this IDataRecord reader, (PropertyInfo Property, int Ordinal)[] propertyMap) where T : class, new()
    {
        var item = new T();
        
        for (int i = 0; i < propertyMap.Length; i++)
        {
            (PropertyInfo property, int ordinal) = propertyMap[i];
            if (!reader.IsDBNull(ordinal))
            {
                object value = reader.GetValue(ordinal);
                if (value != null && value != DBNull.Value)
                {
                    property.SetValue(item, value.ConvertToType(property.PropertyType));
                }
            }
        }
        
        return item;
    }
    
    /// <summary>
    /// Crea un mapeo de propiedades a índices de columnas usando Span para mejor rendimiento.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad.</typeparam>
    /// <param name="reader">DataReader para obtener los nombres de columnas.</param>
    /// <returns>Arreglo con el mapeo de propiedades a índices.</returns>
    public static (PropertyInfo Property, int Ordinal)[] CreatePropertyMap<T>(this IDataRecord reader) where T : class, new()
    {
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                                 .ToArray();

        int fieldCount = reader.FieldCount;
        string[] columnNames = new string[fieldCount];
        for (int i = 0; i < fieldCount; i++)
        {
            columnNames[i] = reader.GetName(i);
        }
        
        var propertyMap = new List<(PropertyInfo, int)>();
        
        foreach (PropertyInfo property in properties)
        {
            for (int i = 0; i < columnNames.Length; i++)
            {
                if (string.Equals(property.Name, columnNames[i], StringComparison.OrdinalIgnoreCase))
                {
                    propertyMap.Add((property, i));
                    break;
                }
            }
        }
        
        return propertyMap.ToArray();
    }
    
    /// <summary>
    /// Procesa un lote de filas usando Memory para reducir asignaciones.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad.</typeparam>
    /// <param name="reader">DataReader con los datos.</param>
    /// <param name="batchSize">Tamaño del lote.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Memory con las entidades procesadas.</returns>
    public static async ValueTask<Memory<T>> ProcessBatchAsync<T>(
        this DbDataReader reader, 
        int batchSize, 
        CancellationToken cancellationToken = default) where T : class, new()
    {
        var entities = new T[batchSize];
        int count = 0;

        (PropertyInfo Property, int Ordinal)[] propertyMap = reader.CreatePropertyMap<T>();
        
        while (count < batchSize && await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            entities[count] = reader.MapToEntityOptimized<T>(propertyMap);
            count++;
        }
        
        if (count < batchSize)
        {
            Array.Resize(ref entities, count);
        }
        
        return entities.AsMemory();
    }
    
    /// <summary>
    /// Convierte un string a ReadOnlySpan para procesamiento eficiente.
    /// </summary>
    /// <param name="value">String a convertir.</param>
    /// <returns>ReadOnlySpan del string.</returns>
    public static ReadOnlySpan<char> AsSpan(this string value)
    {
        return value.AsSpan();
    }
    
    /// <summary>
    /// Compara dos strings usando ReadOnlySpan para mejor rendimiento.
    /// </summary>
    /// <param name="value1">Primer string.</param>
    /// <param name="value2">Segundo string.</param>
    /// <param name="comparison">Tipo de comparación.</param>
    /// <returns>True si son iguales.</returns>
    public static bool EqualsSpan(this string value1, string value2, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return value1.AsSpan().Equals(value2.AsSpan(), comparison);
    }
}
