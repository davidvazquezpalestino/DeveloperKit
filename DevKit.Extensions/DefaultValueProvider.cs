namespace DevKit.Extensions;

internal static class DefaultValueProvider
{
    internal static readonly Dictionary<Type, object> DefaultValues = new()
    {
        { typeof(int), 0 },
        { typeof(int?), 0 },
        { typeof(decimal), 0m },
        { typeof(decimal?), 0m },
        { typeof(double), 0d },
        { typeof(double?), 0d },
        { typeof(DateTime), DateTime.MinValue },
        { typeof(DateTime?), DateTime.MinValue },
        { typeof(string), string.Empty },
        { typeof(bool), false },
        { typeof(bool?), false }
    };

    internal static T GetDefaultValue<T>()
    {
        return DefaultValues.TryGetValue(typeof(T), out object value) ? (T)value : default;
    }

    internal static object GetDefaultValue(Type type)
    {
        return DefaultValues.TryGetValue(type, out var value) ? value : (type.IsValueType ? Activator.CreateInstance(type) : null);
    }
    internal static object ConvertValue(object value, Type targetType)
    {
        Type effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (effectiveType == typeof(Guid))
        {
            return value is Guid guid ? guid : Guid.Parse(value.ToString());
        }

        if (effectiveType.IsEnum)
        {
            return Enum.Parse(effectiveType, value.ToString(), true);
        }

        return Convert.ChangeType(value, effectiveType);
    }
}