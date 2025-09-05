namespace DevKit.Extensions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

/// <summary>Proporciona métodos de extensión para IDataReader que facilitan la lectura de datos de forma segura.</summary>
public static class DataReaderExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyCache = new();

    /// <summary>Obtiene el valor tipado de la columna. Si es DBNull retorna default(T).</summary>
    public static T GetValue<T>(this IDataReader reader, string columnName)
    {
        try
        {
            object value = reader[columnName];
            return value == DBNull.Value ? default : (T)value;
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException)
        {
            throw CreateConversionException<T>(reader, columnName, ex);
        }
    }

    /// <summary>Obtiene el valor de forma segura, retornando default(T) si la columna no existe o es nula.</summary>
    public static T GetValueSafe<T>(this IDataReader reader, string columnName, T defaultValue = default)
    {
        try
        {
            if (!ColumnExists(reader, columnName))
                return defaultValue;

            object value = reader[columnName];
            return value == DBNull.Value ? defaultValue : (T)value;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>Verifica si una columna existe en el lector.</summary>
    public static bool ColumnExists(this IDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>Mapea los datos del IDataReader a un objeto del tipo especificado.</summary>
    public static T GetItem<T>(this IDataReader reader) where T : new()
    {
        T item = new T();
        Type type = typeof(T);

        Dictionary<string, PropertyInfo> properties = GetCachedProperties(type);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);

            if (properties.TryGetValue(columnName, out PropertyInfo prop) && !reader.IsDBNull(i))
            {
                SetPropertyValue(item, prop, reader[i]);
            }
        }

        return item;
    }

    /// <summary>Mapea todos los registros del lector a una lista de objetos.</summary>
    public static List<T> GetItems<T>(this IDataReader reader) where T : new()
    {
        List<T> results = new List<T>();

        while (reader.Read())
        {
            results.Add(reader.GetItem<T>());
        }

        return results;
    }

    /// <summary>Obtiene un diccionario con los nombres y valores de todas las columnas del registro actual.</summary>
    public static Dictionary<string, object> ToDictionary(this IDataReader reader)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            object value = reader.IsDBNull(i) ? null : reader[i];
            dict[columnName] = value;
        }

        return dict;
    }

    /// <summary>Convierte todos los registros del lector a una lista de diccionarios.</summary>
    public static List<Dictionary<string, object>> ToDictionaryList(this IDataReader reader)
    {
        List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

        while (reader.Read())
        {
            results.Add(reader.ToDictionary());
        }

        return results;
    }

    /// <summary>Convierte el IDataReader a un DataTable.</summary>
    public static DataTable ToDataTable(this IDataReader reader)
    {
        DataTable table = new DataTable();

        // Agregar columnas al DataTable
        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            Type columnType = reader.GetFieldType(i);

            table.Columns.Add(columnName, columnType);
        }

        // Agregar filas con los datos
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

    /// <summary>Convierte el IDataReader a un DataTable con un nombre específico.</summary>
    public static DataTable ToDataTable(this IDataReader reader, string tableName)
    {
        DataTable table = reader.ToDataTable();
        table.TableName = tableName;
        return table;
    }

    private static Dictionary<string, PropertyInfo> GetCachedProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.CanWrite)
                             .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase));
    }

    private static void SetPropertyValue<T>(T item, PropertyInfo property, object value)
    {
        try
        {
            if (value == DBNull.Value)
                return;

            // Conversión directa si los tipos son compatibles
            if (property.PropertyType.IsInstanceOfType(value))
            {
                property.SetValue(item, value);
                return;
            }

            // Conversión para tipos nullable
            Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            object convertedValue = Convert.ChangeType(value, targetType);
            property.SetValue(item, convertedValue);
        }
        catch (Exception ex)
        {
            // Log silencioso del error
            System.Diagnostics.Debug.WriteLine($"Error setting property {property.Name}: {ex.Message}");
        }
    }

    private static InvalidCastException CreateConversionException<T>(IDataReader reader, string columnName, Exception innerException)
    {
        try
        {
            object columnValue = reader[columnName];
            string valueType = columnValue?.GetType().Name ?? "null";
            string valueString = columnValue?.ToString() ?? "null";

            return new InvalidCastException(
                $"Error converting column '{columnName}' to type {typeof(T).Name}. " +
                $"Value type: {valueType}, Value: {valueString}",
                innerException);
        }
        catch
        {
            return new InvalidCastException(
                $"Error converting column '{columnName}' to type {typeof(T).Name}",
                innerException);
        }
    }
}