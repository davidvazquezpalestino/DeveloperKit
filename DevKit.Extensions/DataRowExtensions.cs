namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión sencillos para <see cref="DataRow" /> y sus colecciones.</summary>
public static class DataRowExtensions
{
    /// <summary>Proporciona métodos de extensión sencillos para <see cref="DataRow" /> y sus colecciones.</summary>
    extension(DataRow row)
    {
        /// <summary>Obtiene el valor tipado de la columna; retorna el valor predeterminado cuando el campo es <see cref="DBNull" />.</summary>
        public T GetValue<T>(string columnName, T defaultValue = default)
        {
            ValidateRow(row);
            ValidateColumnName(columnName);
            EnsureColumnExists(row, columnName);

            object value = row[columnName];

            if (value == DBNull.Value)
            {
                return defaultValue;
            }

            if (value is T typedValue)
            {
                return typedValue;
            }

            try
            {
                return (T)ConvertValue(value, typeof(T));
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException)
            {
                throw new InvalidCastException($"No fue posible convertir la columna '{columnName}' al tipo {typeof(T).Name}.", ex);
            }
        }

        /// <summary>Obtiene el valor de la columna de forma segura sin generar excepciones.</summary>
        public bool TryGetValue<T>(string columnName, out T result, T defaultValue = default)
        {
            result = defaultValue;

            if (row == null || string.IsNullOrWhiteSpace(columnName) || !row.Table.Columns.Contains(columnName))
            {
                return false;
            }

            object value = row[columnName];

            if (value == DBNull.Value)
            {
                return false;
            }

            try
            {
                result = value is T typedValue ? typedValue : (T)ConvertValue(value, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Mapea el registro actual hacia una instancia del tipo indicado.</summary>
        public T GetItem<T>() where T : new()
        {
            ValidateRow(row);

            T item = new();
            Type itemType = typeof(T);

            foreach (DataColumn column in row.Table.Columns)
            {
                PropertyInfo property = itemType.GetProperty(column.ColumnName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property == null || !property.CanWrite)
                {
                    continue;
                }

                object value = row[column];

                if (value == DBNull.Value)
                {
                    continue;
                }

                try
                {
                    object convertedValue = property.PropertyType.IsInstanceOfType(value)
                        ? value
                        : ConvertValue(value, property.PropertyType);

                    property.SetValue(item, convertedValue);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error asignando la columna '{column.ColumnName}' a '{property.Name}': {ex.Message}");
                }
            }

            return item;
        }
    }

    /// <summary>Mapea todas las filas de la tabla a una lista del tipo indicado.</summary>
    public static List<T> GetItems<T>(this DataTable table) where T : new()
    {
        if (table == null)
        {
            throw new ArgumentNullException(nameof(table));
        }

        List<T> items = new();

        foreach (DataRow row in table.Rows)
        {
            items.Add(row.GetItem<T>());
        }

        return items;
    }

    /// <summary>Mapea una colección de filas a una lista del tipo indicado.</summary>
    public static List<T> GetItems<T>(this IEnumerable<DataRow> rows) where T : new()
    {
        if (rows == null)
        {
            throw new ArgumentNullException(nameof(rows));
        }

        List<T> items = new();

        foreach (DataRow row in rows)
        {
            items.Add(row.GetItem<T>());
        }

        return items;
    }

    private static void ValidateRow(DataRow row)
    {
        if (row == null)
        {
            throw new ArgumentNullException(nameof(row));
        }

        if (row.Table == null)
        {
            throw new ArgumentException("El DataRow no está asociado a una tabla.", nameof(row));
        }
    }

    private static void ValidateColumnName(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            throw new ArgumentException("El nombre de la columna no puede estar vacío.", nameof(columnName));
        }
    }

    private static void EnsureColumnExists(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
        {
            throw new ArgumentException($"La columna '{columnName}' no existe en la tabla.", nameof(columnName));
        }
    }

    private static object ConvertValue(object value, Type targetType)
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