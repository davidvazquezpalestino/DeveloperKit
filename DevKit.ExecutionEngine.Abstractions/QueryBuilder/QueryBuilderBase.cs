using System.Reflection;
using System.Linq.Expressions;

namespace DevKit.ExecutionEngine.Abstractions.QueryBuilder;

/// <summary>
/// Clase base abstracta que proporciona funcionalidad común para query builders tipados.
/// Encapsula la lógica de mapeo de DataTable a objetos y conversión de tipos.
/// </summary>
/// <typeparam name="T">Tipo de entidad para el mapeo automático.</typeparam>
public abstract class QueryBuilderBase<T> : ITypedQueryBuilder<T> where T : class, new()
{
    protected readonly Dictionary<string, PropertyInfo> _propertyMap;

    protected QueryBuilderBase()
    {
        _propertyMap = typeof(T).GetProperties()
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Construye la consulta SQL final.
    /// </summary>
    public abstract string Build();

    /// <summary>
    /// Obtiene los parámetros generados para la consulta.
    /// </summary>
    public abstract IReadOnlyDictionary<string, object> GetParameters();

    /// <summary>
    /// Mapea un DataTable a una lista de objetos del tipo T.
    /// </summary>
    /// <param name="dataTable">DataTable con los datos a mapear.</param>
    /// <returns>Lista de objetos mapeados.</returns>
    public virtual List<T> MapToList(DataTable dataTable)
    {
        List<T> results = new List<T>();

        foreach (DataRow row in dataTable.Rows)
        {
            T obj = new T();

            foreach (DataColumn column in dataTable.Columns)
            {
                if (_propertyMap.TryGetValue(column.ColumnName, out PropertyInfo property))
                {
                    object value = row[column];
                    if (value != DBNull.Value)
                    {
                        object convertedValue = ConvertValue(value, property.PropertyType);
                        property.SetValue(obj, convertedValue);
                    }
                }
            }

            results.Add(obj);
        }

        return results;
    }

    /// <summary>
    /// Mapea la primera fila de un DataTable a un objeto del tipo T.
    /// </summary>
    /// <param name="dataTable">DataTable con los datos a mapear.</param>
    /// <returns>Objeto mapeado o null si no hay datos.</returns>
    public virtual T MapToSingle(DataTable dataTable)
    {
        if (dataTable.Rows.Count == 0)
            return null;

        List<T> results = MapToList(dataTable);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Convierte un valor de base de datos al tipo de propiedad especificado.
    /// </summary>
    /// <param name="value">Valor a convertir.</param>
    /// <param name="targetType">Tipo de destino.</param>
    /// <returns>Valor convertido.</returns>
    protected virtual object ConvertValue(object value, Type targetType)
    {
        if (value == null || value == DBNull.Value)
            return null;

        // Manejar tipos nullable
        Type underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
            targetType = underlyingType;

        // Conversión directa si los tipos coinciden
        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        // Conversiones especiales
        if (targetType == typeof(Guid) && value is string stringValue)
            return Guid.Parse(stringValue);

        if (targetType.IsEnum)
            return Enum.ToObject(targetType, value);

        // Conversión usando Convert.ChangeType
        return Convert.ChangeType(value, targetType);
    }

    /// <summary>
    /// Extrae el nombre de una propiedad de una expresión lambda.
    /// </summary>
    /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
    /// <param name="selector">Expresión lambda que selecciona la propiedad.</param>
    /// <returns>Nombre de la propiedad.</returns>
    protected virtual string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> selector)
    {
        if (selector.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("Selector debe ser una expresión de propiedad", nameof(selector));
    }
}
