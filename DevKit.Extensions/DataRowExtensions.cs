namespace DevKit.Extensions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

/// <summary>Proporciona métodos de extensión para DataRow que facilitan el acceso seguro a los datos.</summary>
public static class DataRowExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyCache = new();

    /// <param name="row">DataRow de origen</param>
    extension(DataRow row)
    {
        /// <summary>
        /// Obtiene el valor tipado de la columna. Si es DBNull retorna default(T).
        /// </summary>
        /// <typeparam name="T">Tipo de dato esperado</typeparam>
        /// <param name="columnName">Nombre de la columna</param>
        /// <param name="defaultValue">Valor por defecto opcional en caso de DBNull</param>
        /// <returns>Valor convertido al tipo especificado</returns>
        /// <exception cref="ArgumentNullException">Cuando el DataRow es nulo</exception>
        /// <exception cref="ArgumentException">Cuando el nombre de la columna no existe</exception>
        /// <exception cref="InvalidCastException">Cuando la conversión de tipo falla</exception>
        public T GetValue<T>(string columnName, T defaultValue = default)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("El nombre de la columna no puede estar vacío", nameof(columnName));
            }

            if (!row.Table.Columns.Contains(columnName))
            {
                throw new ArgumentException($"La columna '{columnName}' no existe en la tabla", nameof(columnName));
            }

            try
            {
                object value = row[columnName];

                if (value == DBNull.Value)
                {
                    return defaultValue;
                }

                // Si el tipo ya es el correcto, retornar directamente
                if (value is T typedValue)
                {
                    return typedValue;
                }

                // Conversión especial para tipos comunes
                return ConvertValue<T>(value);
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException)
            {
                object columnValue = row[columnName];
                throw CreateConversionException<T>(columnName, columnValue, ex);
            }
        }

        /// <summary>
        /// Obtiene el valor de la columna de forma segura, sin lanzar excepciones
        /// </summary>
        public bool TryGetValue<T>(string columnName, out T result, T defaultValue = default)
        {
            result = defaultValue;

            try
            {
                if (row == null || !row.Table.Columns.Contains(columnName))
                {
                    return false;
                }

                object value = row[columnName];
                if (value == DBNull.Value)
                {
                    return false;
                }

                result = ConvertValue<T>(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Mapea los datos del DataRow a un objeto del tipo especificado
        /// </summary>
        public T GetItem<T>() where T : new()
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            T item = new T();
            Type type = typeof(T);

            Dictionary<string, PropertyInfo> properties = GetCachedProperties(type);

            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;
                object value = row[columnName];

                if (value == DBNull.Value)
                {
                    continue;
                }

                // Intentar mapear a propiedad
                if (properties.TryGetValue(columnName, out PropertyInfo property))
                {
                    SetPropertyValue(item, property, value);
                }

            }

            return item;
        }
    }

    /// <summary>
    /// Mapea una DataTable a una lista de objetos
    /// </summary>
    extension(DataTable table)
    {
        /// <summary>
        /// Mapea una DataTable a una lista de objetos
        /// </summary>
        public List<T> GetItems<T>() where T : new()
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            return table.AsEnumerable().GetItems<T>();
        }
    }
    /// <summary>
    /// Mapea una colección de DataRows a una lista de objetos
    /// </summary>
    extension(IEnumerable<DataRow> rows)
    {
        /// <summary>
        /// Mapea una colección de DataRows a una lista de objetos
        /// </summary>
        public List<T> GetItems<T>() where T : new()
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            return rows.Select(row => row.GetItem<T>()).ToList();
        }
    }

    private static Dictionary<string, PropertyInfo> GetCachedProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(propertyInfo => propertyInfo.CanWrite && propertyInfo.GetSetMethod(false) != null)
             .ToDictionary(propertyInfo => propertyInfo.Name, p => p, StringComparer.OrdinalIgnoreCase));
    }


    private static void SetPropertyValue<T>(T item, PropertyInfo property, object value)
    {
        try
        {
            object convertedValue = ConvertToType(value, property.PropertyType);
            property.SetValue(item, convertedValue);
        }
        catch (Exception ex)
        {
            HandleMappingError(property.Name, value, ex);
        }
    }

    private static object ConvertToType(object value, Type targetType)
    {
        if (value == null || value == DBNull.Value)
        {
            return GetDefaultValue(targetType);
        }

        Type sourceType = value.GetType();

        // Si los tipos son compatibles, retornar directamente
        if (targetType.IsAssignableFrom(sourceType))
        {
            return value;
        }

        // Manejar tipos nullable
        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Usar TypeConverter para conversiones más flexibles
        TypeConverter converter = TypeDescriptor.GetConverter(underlyingType);
        if (converter.CanConvertFrom(sourceType))
        {
            return converter.ConvertFrom(value);
        }

        // Conversión estándar
        return Convert.ChangeType(value, underlyingType);
    }

    private static T ConvertValue<T>(object value)
    {
        if (value is T typedValue)
        {
            return typedValue;
        }

        Type targetType = typeof(T);
        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Conversiones especiales para tipos comunes
        if (underlyingType == typeof(Guid) && value is string stringGuid)
        {
            return (T)(object)Guid.Parse(stringGuid);
        }

        if (underlyingType == typeof(DateTimeOffset) && value is DateTime dateTime)
        {
            return (T)(object)new DateTimeOffset(dateTime);
        }

        if (underlyingType.IsEnum && value is string stringEnum)
        {
            return (T)Enum.Parse(underlyingType, stringEnum, true);
        }

        // Usar TypeConverter
        TypeConverter converter = TypeDescriptor.GetConverter(underlyingType);
        if (converter.CanConvertFrom(value.GetType()))
        {
            return (T)converter.ConvertFrom(value);
        }

        return (T)Convert.ChangeType(value, underlyingType);
    }

    private static object GetDefaultValue(Type type)
    {
        return type.IsValueType && Nullable.GetUnderlyingType(type) == null
            ? Activator.CreateInstance(type)
            : null;
    }

    private static InvalidCastException CreateConversionException<T>(string columnName, object value, Exception innerException)
    {
        string valueType = value?.GetType().Name ?? "null";
        string valueString = value?.ToString() ?? "null";

        return new InvalidCastException(
            $"Error al convertir el valor de la columna '{columnName}' al tipo {typeof(T).Name}. " +
            $"Tipo del valor: {valueType}, Valor: {valueString}. " +
            $"Asegúrese de que los tipos son compatibles.", innerException);
    }

    private static void HandleMappingError(string memberName, object value, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error al mapear el valor '{value}' al miembro '{memberName}': {ex.Message}");
    }
}