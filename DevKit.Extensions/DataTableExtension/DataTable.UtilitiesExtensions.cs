namespace DevKit.Extensions.DataTableExtension;

public static partial class DataTableExtension
{
    /// <summary>Determina si un tipo es considerado simple.</summary>
    /// <param name="type">El tipo a verificar.</param>
    /// <returns>Verdadero si el tipo es simple; de lo contrario, falso.</returns>
    public static bool IsSimpleType(this Type type)
    {
        if (type == null)
        {
            return false;
        }

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
    /// <typeparam name="T">El tipo a validar.</typeparam>
    public static void GuardNotPrimitiveType<T>()
    {
        Type underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        if (underlyingType.IsPrimitive || underlyingType == typeof(string) || underlyingType == typeof(DateTime) || underlyingType.IsEnum)
        {
            throw new InvalidOperationException("This method does not support primitive types, enums, or strings.");
        }
    }

    /// <summary>Convierte una secuencia de DataRow en DataTable, devolviendo una tabla vacía si no hay filas.</summary>
    /// <param name="rows">La secuencia de filas.</param>
    /// <param name="schema">Esquema opcional para la tabla resultante.</param>
    /// <returns>Un <see cref="DataTable"/> con las filas especificadas.</returns>
    public static DataTable ToDataTable(this IEnumerable<DataRow> rows, DataTable schema)
    {
        if (rows == null)
        {
            return schema?.Clone() ?? new DataTable();
        }

        List<DataRow> dataRows = rows.ToList();
        if (!dataRows.Any())
        {
            return schema?.Clone() ?? new DataTable();
        }

        return dataRows.CopyToDataTable();
    }

    /// <summary>Alias de ToDataTable para escenarios que esperan una tabla vacía cuando no hay filas.</summary>
    /// <param name="rows">La secuencia de filas.</param>
    /// <param name="schema">Esquema opcional.</param>
    /// <returns>Un <see cref="DataTable"/> con las filas o una tabla vacía.</returns>
    public static DataTable ToDataTableOrEmpty(this IEnumerable<DataRow> rows, DataTable schema)
    {
        return rows.ToDataTable(schema);
    }

    /// <summary>Normaliza DBNull a null.</summary>
    /// <param name="value">El objeto a normalizar.</param>
    /// <returns>El valor original o null si era DBNull.</returns>
    public static object DbNullToNull(object value)
    {
        return value == DBNull.Value ? null : value;
    }

    /// <summary>Compara objetos manejando DBNull y null como equivalentes.</summary>
    /// <param name="first">Primer objeto.</param>
    /// <param name="second">Segundo objeto.</param>
    /// <returns>Verdadero si son equivalentes; de lo contrario, falso.</returns>
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
