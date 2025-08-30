namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtensions
{
    /// <summary>Determina si un tipo es considerado simple.</summary>
    public static bool IsSimpleType(this Type type)
    {
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        if (underlyingType.IsEnum)
        {
            return true;
        }

        return underlyingType.IsPrimitive ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(TimeSpan) ||
               underlyingType == typeof(double) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(float) ||
               underlyingType == typeof(bool) ||
               underlyingType == typeof(Guid);
    }

    /// <summary>Valida que el tipo genérico no sea primitivo.</summary>
    public static void GuardNotPrimitiveType<T>()
    {
        Type underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        if (underlyingType.IsPrimitive || underlyingType == typeof(string) || underlyingType == typeof(DateTime) || underlyingType.IsEnum)
        {
            throw new InvalidOperationException("This method does not support primitive types, enums, or strings.");
        }
    }

    /// <summary>Convierte una secuencia de DataRow en DataTable, devolviendo una tabla vacía si no hay filas.</summary>
    public static DataTable ToDataTable(this IEnumerable<DataRow> rows, DataTable schema)
    {
        if (rows == null)
        {
            return schema?.Clone() ?? new DataTable();
        }

        using (IEnumerator<DataRow> enumerator = rows.GetEnumerator())
        {
            if (!enumerator.MoveNext())
            {
                return schema?.Clone() ?? new DataTable();
            }
        }
        return rows.CopyToDataTable();
    }

    /// <summary>Alias de ToDataTable para escenarios que esperan una tabla vacía cuando no hay filas.</summary>
    public static DataTable ToDataTableOrEmpty(this IEnumerable<DataRow> rows, DataTable schema)
    {
        return ToDataTable(rows, schema);
    }

    /// <summary>Normaliza DBNull a null.</summary>
    public static object DbNullToNull(object value)
    {
        return value == DBNull.Value ? null : value;
    }

    /// <summary>Compara objetos manejando DBNull y null como equivalentes.</summary>
    public static bool ObjectsEqual(object first, object second)
    {
        object left = DbNullToNull(first);
        object right = DbNullToNull(second);
        return Equals(left, right);
    }

    /// <summary>Normaliza valores JSON convirtiendo JsonElement a tipos .NET apropiados.</summary>
    internal static object NormalizeJsonElement(object value)
    {
        if (value is JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Number:
                    if (jsonElement.TryGetInt64(out long longValue))
                    {
                        return longValue;
                    }

                    if (jsonElement.TryGetDecimal(out decimal decimalValue))
                    {
                        return decimalValue;
                    }

                    return jsonElement.GetDouble();
                case JsonValueKind.String:
                    string stringValue = jsonElement.GetString();
                    if (!string.IsNullOrWhiteSpace(stringValue))
                    {
                        if (DateTimeOffset.TryParse(stringValue, out DateTimeOffset dateTimeOffset))
                        {
                            return dateTimeOffset;
                        }

                        if (DateTime.TryParse(stringValue, out DateTime dateTime))
                        {
                            return dateTime;
                        }
                    }
                    return stringValue;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    return jsonElement.GetRawText();
            }
        }
        return value;
    }

    /// <summary>Determina el tipo .NET más adecuado para una columna a partir de sus valores.</summary>
    internal static Type DetermineColumnType(IEnumerable<object> values)
    {
        List<object> nonNullValues = values.Where(value => value != null).ToList();
        if (nonNullValues.Count == 0)
        {
            return typeof(object);
        }

        bool hasString = nonNullValues.Any(value => value is string);
        bool hasBool = nonNullValues.Any(value => value is bool);
        bool hasInteger = nonNullValues.Any(value => value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong);
        bool hasDecimal = nonNullValues.Any(value => value is decimal);
        bool hasFloating = nonNullValues.Any(value => value is double || value is float);
        bool hasDateTimeOffset = nonNullValues.Any(value => value is DateTimeOffset);
        bool hasDateTime = nonNullValues.Any(value => value is DateTime);

        if (hasString)
        {
            return typeof(string);
        }

        if ((hasBool && (hasInteger || hasDecimal || hasFloating)) || ((hasDateTime || hasDateTimeOffset) && (hasInteger || hasDecimal || hasFloating || hasBool)))
        {
            return typeof(string);
        }

        if (hasDateTimeOffset)
        {
            return typeof(DateTimeOffset);
        }

        if (hasDateTime)
        {
            return typeof(DateTime);
        }

        if (hasDecimal || (hasInteger && hasFloating))
        {
            return typeof(decimal);
        }

        if (hasFloating)
        {
            return typeof(double);
        }

        if (hasInteger)
        {
            return typeof(long);
        }

        if (hasBool)
        {
            return typeof(bool);
        }

        return typeof(object);
    }
}
