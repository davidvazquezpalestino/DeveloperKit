namespace DevKit.ExecutionEngine.Abstractions.QueryBuilder;

/// <summary>
/// Interface base para query builders que define operaciones comunes de construcción de consultas SQL.
/// </summary>
public interface IQueryBuilder
{
    /// <summary>
    /// Construye la consulta SQL final.
    /// </summary>
    /// <returns>Cadena SQL generada.</returns>
    string Build();

    /// <summary>
    /// Obtiene los parámetros generados para la consulta.
    /// </summary>
    /// <returns>Diccionario de parámetros con sus valores.</returns>
    IReadOnlyDictionary<string, object> GetParameters();
}

/// <summary>
/// Interface para query builders tipados que soportan mapeo automático a objetos.
/// </summary>
/// <typeparam name="T">Tipo de entidad para el mapeo.</typeparam>
public interface ITypedQueryBuilder<T> : IQueryBuilder where T : class, new()
{
    /// <summary>
    /// Mapea un DataTable a una lista de objetos del tipo T.
    /// </summary>
    /// <param name="dataTable">DataTable con los datos a mapear.</param>
    /// <returns>Lista de objetos mapeados.</returns>
    List<T> MapToList(DataTable dataTable);

    /// <summary>
    /// Mapea la primera fila de un DataTable a un objeto del tipo T.
    /// </summary>
    /// <param name="dataTable">DataTable con los datos a mapear.</param>
    /// <returns>Objeto mapeado o null si no hay datos.</returns>
    T MapToSingle(DataTable dataTable);
}
