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
                return (T)reader[columnName];
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"No fue posible convertir la columna '{columnName}' al tipo {typeof(T).Name}.", ex);
            }
        }

        /// <summary>Obtiene el valor de forma segura o devuelve el valor predeterminado del tipo si la columna no existe o es nula.</summary>
        public T TryGetValue<T>(string columnName)
        {
            try
            {
                object value = reader[columnName];
                return value == DBNull.Value ? DefaultValueProvider.GetDefaultValue<T>() : (T)value;
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"No fue posible convertir la columna '{columnName}' al tipo {typeof(T).Name}.", ex);
            }
        }

        /// <summary>Mapea el registro actual hacia una instancia del tipo indicado.</summary>
        public T GetItem<T>() where T : new()
        {
            T item = new T();
            Type type = typeof(T);

            foreach (int i in Enumerable.Range(0, reader.FieldCount))
            {
                if (reader.IsDBNull(i)) continue;

                PropertyInfo propertyInfo = type.GetProperty(reader.GetName(i),
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase);

                if (propertyInfo?.CanWrite == true)
                {
                    try
                    {
                        object value = reader[i];
                        Type targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                        object converted = targetType.IsInstanceOfType(value) ? value : Convert.ChangeType(value, targetType);
                        propertyInfo.SetValue(item, converted);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidCastException($"No fue posible convertir la columna '{reader.GetName(i)}' al tipo {typeof(T).Name}.", ex);
                    }
                }
            }
            return item;
        }

        /// <summary>Mapea el registro actual hacia una instancia del tipo indicado, asignando valores predeterminados si son nulos.</summary>
        public T TryGetItem<T>() where T : new()
        {
            T item = new T();
            Type type = typeof(T);

            foreach (int i in Enumerable.Range(0, reader.FieldCount))
            {
                PropertyInfo propertyInfo = type.GetProperty(reader.GetName(i),
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase);

                if (propertyInfo?.CanWrite == true)
                {
                    try
                    {
                        object converted;
                        if (reader.IsDBNull(i))
                        {
                            converted = DefaultValueProvider.GetDefaultValue(propertyInfo.PropertyType);
                        }
                        else
                        {
                            object value = reader[i];
                            Type targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                            converted = targetType.IsInstanceOfType(value) ? value : Convert.ChangeType(value, targetType);
                        }

                        propertyInfo.SetValue(item, converted);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error setting {propertyInfo?.Name}: {ex.Message}");
                        // On error, set default
                        object defaultValue = DefaultValueProvider.GetDefaultValue(propertyInfo.PropertyType);
                        propertyInfo.SetValue(item, defaultValue);
                    }
                }
            }
            return item;
        }

        /// <summary>Indica si la columna especificada existe en el lector.</summary>
        public bool ColumnExists(string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Convierte el registro actual en un diccionario <c>nombre-valor</c>.</summary>
        public Dictionary<string, object> ToDictionary()
        {
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