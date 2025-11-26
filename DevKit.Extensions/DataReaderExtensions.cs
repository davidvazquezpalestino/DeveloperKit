namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión sencillos para <see cref="IDataReader" />.</summary>
public static class DataReaderExtensions
{

    /// <summary>Proporciona métodos de extensión sencillos para <see cref="IDataReader" />.</summary>
    extension(IDataReader reader)
    {
        /// <summary>Obtiene el valor tipado de la columna; retorna <c>default</c> si es <see cref="DBNull" />.</summary>
        public T GetValue<T>(string columnName)
        {
            try
            {
                object value = reader[columnName];
                return value == DBNull.Value ? default : (T)value;
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"No fue posible convertir la columna '{columnName}' al tipo {typeof(T).Name}.", ex);
            }
        }

        /// <summary>Obtiene el valor de forma segura o devuelve el valor predeterminado si la columna no existe o es nula.</summary>
        public T GetValueSafe<T>(string columnName, T defaultValue = default)
        {
            if (reader == null || !reader.ColumnExists(columnName))
            {
                return defaultValue;
            }

            try
            {
                object value = reader[columnName];
                return value == DBNull.Value ? defaultValue : (T)value;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>Indica si la columna especificada existe en el lector.</summary>
        public bool ColumnExists(string columnName)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Mapea el registro actual hacia una instancia del tipo indicado.</summary>
        public T GetItem<T>() where T : new()
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            T item = new();
            Type itemType = typeof(T);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.IsDBNull(i))
                {
                    continue;
                }

                PropertyInfo property = itemType.GetProperty(reader.GetName(i), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property == null || !property.CanWrite)
                {
                    continue;
                }

                object value = reader[i];

                try
                {
                    Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    object convertedValue = targetType.IsInstanceOfType(value) ? value : Convert.ChangeType(value, targetType);
                    property.SetValue(item, convertedValue);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting property {property.Name}: {ex.Message}");
                }
            }

            return item;
        }

        /// <summary>Lee todos los registros restantes y los convierte en una lista del tipo indicado.</summary>
        public List<T> GetItems<T>() where T : new()
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            List<T> items = new();

            while (reader.Read())
            {
                items.Add(reader.GetItem<T>());
            }

            return items;
        }

        /// <summary>Convierte el registro actual en un diccionario <c>nombre-valor</c>.</summary>
        public Dictionary<string, object> ToDictionary()
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            Dictionary<string, object> result = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                result[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[i];
            }

            return result;
        }

        /// <summary>Convierte todos los registros en una lista de diccionarios.</summary>
        public List<Dictionary<string, object>> ToDictionaryList()
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            List<Dictionary<string, object>> items = new();

            while (reader.Read())
            {
                items.Add(reader.ToDictionary());
            }

            return items;
        }

        /// <summary>Convierte el resultado del lector en un <see cref="DataTable" />.</summary>
        public DataTable ToDataTable()
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            DataTable table = new();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            while (reader.Read())
            {
                DataRow row = table.NewRow();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader.IsDBNull(i) ? DBNull.Value : reader[i];
                }

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>Convierte el lector a <see cref="DataTable" /> y asigna el nombre proporcionado.</summary>
        public DataTable ToDataTable(string tableName)
        {
            DataTable table = reader.ToDataTable();
            table.TableName = tableName;
            return table;
        }
    }
}