namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Proporciona métodos de extensión para conversiones de tipo optimizadas con caché.
/// </summary>
public static class TypeConverterExtensions
{
    private static readonly ConcurrentDictionary<(Type FromType, Type ToType), Func<object, object>> _converterCache = new();

    /// <summary>
    /// Convierte un valor al tipo especificado usando caché de convertidores para mejor rendimiento.
    /// </summary>
    /// <param name="value">Valor a convertir.</param>
    /// <param name="targetType">Tipo de destino.</param>
    /// <returns>Valor convertido.</returns>
    public static object ConvertToType(this object value, Type targetType)
    {
        if (value == null || value == DBNull.Value)
            return null;

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        Func<object, object> converter = _converterCache.GetOrAdd((value.GetType(), targetType), CreateConverter);
        return converter(value);
    }

    /// <summary>
    /// Convierte un valor al tipo especificado usando caché de convertidores.
    /// </summary>
    /// <typeparam name="T">Tipo de destino.</typeparam>
    /// <param name="value">Valor a convertir.</param>
    /// <returns>Valor convertido.</returns>
    public static T ConvertToType<T>(this object value)
    {
        if (value == null || value == DBNull.Value)
            return default;

        if (value is T directValue)
            return directValue;

        Func<object, object> converter = _converterCache.GetOrAdd((value.GetType(), typeof(T)), CreateConverter);
        return (T)converter(value);
    }

    /// <summary>
    /// Intenta convertir un valor al tipo especificado, devolviendo un valor predeterminado si falla.
    /// </summary>
    /// <typeparam name="T">Tipo de destino.</typeparam>
    /// <param name="value">Valor a convertir.</param>
    /// <param name="defaultValue">Valor predeterminado si la conversión falla.</param>
    /// <returns>Valor convertido o valor predeterminado.</returns>
    public static T TryConvertToType<T>(this object value, T defaultValue = default)
    {
        try
        {
            return value.ConvertToType<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    private static Func<object, object> CreateConverter((Type FromType, Type ToType) key)
    {
        Type fromType = key.FromType;
        Type toType = key.ToType;

        // Para tipos nullable, usar el tipo subyacente
        Type underlyingToType = Nullable.GetUnderlyingType(toType) ?? toType;

        // Si es una conversión simple, usar Convert.ChangeType
        if (IsSimpleConversion(fromType, underlyingToType))
        {
            return value => Convert.ChangeType(value, underlyingToType);
        }

        // Intentar usar TypeConverter
        TypeConverter converter = TypeDescriptor.GetConverter(underlyingToType);
        if (converter.CanConvertFrom(fromType))
        {
            return value => converter.ConvertFrom(value);
        }

        // Para enums, usar conversión especial
        if (underlyingToType.IsEnum)
        {
            return value => Enum.ToObject(underlyingToType, value);
        }

        // Para tipos complejos, intentar con el constructor
        ConstructorInfo constructor = underlyingToType.GetConstructor(new[] { fromType });
        if (constructor != null)
        {
            ParameterExpression param = Expression.Parameter(typeof(object), "value");
            UnaryExpression cast = Expression.Convert(param, fromType);
            NewExpression newExpr = Expression.New(constructor, cast);
            var lambda = Expression.Lambda<Func<object, object>>(newExpr, param);
            return lambda.Compile();
        }

        // Último recurso: Convert.ChangeType
        return value => Convert.ChangeType(value, toType);
    }

    private static bool IsSimpleConversion(Type fromType, Type toType)
    {
        // Conversiones numéricas
        if (IsNumericType(fromType) && IsNumericType(toType))
            return true;

        // Conversiones a/desde string
        if (fromType == typeof(string) || toType == typeof(string))
            return true;

        // Conversiones a/desde DateTime
        if (fromType == typeof(DateTime) || toType == typeof(DateTime))
            return true;

        // Conversiones a/desde bool
        if (fromType == typeof(bool) || toType == typeof(bool))
            return true;

        return false;
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(decimal);
    }
}
