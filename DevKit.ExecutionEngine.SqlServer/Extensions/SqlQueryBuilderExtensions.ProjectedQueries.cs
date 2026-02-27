
namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Métodos de extensión para consultas proyectadas.
/// </summary>
public static partial class SqlQueryBuilderExtensions
{
    /// <summary>
    /// Ejecuta una consulta proyectada y devuelve los resultados como una lista.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <returns>Una lista de resultados proyectados.</returns>
    public static List<TResult> ToList<T, TResult>(this ProjectedQueryState<T, TResult> queryState) where T : class, new()
    {
        QueryResult queryResult = BuildQuery(queryState);

        ICollection<TResult> result = queryState.DbProvider.ExecuteQueryAsList(queryResult.SQL,
            reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
            collection => collection.AddSqlParameters(queryResult.Parameters));

        return result.ToList();
    }

    /// <summary>
    /// Ejecuta la consulta proyectada y devuelve el primer resultado, o null si no se encuentran resultados.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <returns>El primer resultado proyectado o null.</returns>
    public static TResult FirstOrDefault<T, TResult>(this ProjectedQueryState<T, TResult> queryState) where T : class, new()
    {
        queryState.TakeField = 1;
        QueryResult queryResult = BuildQuery(queryState);

        ICollection<TResult> results = queryState.DbProvider.ExecuteQueryAsList(queryResult.SQL,
            reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
            collection => collection.AddSqlParameters(queryResult.Parameters));

        return results.FirstOrDefault();
    }

    /// <summary>
    /// Ejecuta la consulta proyectada y devuelve el primer resultado, o lanza una excepción si no se encuentra ningún elemento.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <returns>El primer resultado proyectado.</returns>
    /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos.</exception>
    public static TResult First<T, TResult>(this ProjectedQueryState<T, TResult> queryState) where T : class, new()
    {
        queryState.TakeField = 1;
        QueryResult queryResult = BuildQuery(queryState);

        ICollection<TResult> results = queryState.DbProvider.ExecuteQueryAsList(queryResult.SQL,
            reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
            collection => collection.AddSqlParameters(queryResult.Parameters));

        return results.First();
    }

    /// <summary>
    /// Devuelve el número total de elementos en la consulta proyectada.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <returns>El número total de elementos.</returns>
    public static int Count<T, TResult>(this ProjectedQueryState<T, TResult> queryState) where T : class, new()
    {
        int? originalTake = queryState.TakeField;
        int? originalSkip = queryState.SkipField;

        try
        {
            queryState.TakeField = null;
            queryState.SkipField = 0;

            QueryResult queryResult = BuildQuery(queryState);
            string countQuery = $"SELECT COUNT(*) FROM ({queryResult.SQL}) AS CountQuery";

            return queryState.DbProvider.ExecuteScalar<int>(countQuery,
                collection => collection.AddSqlParameters(queryResult.Parameters));
        }
        finally
        {
            queryState.TakeField = originalTake;
            queryState.SkipField = originalSkip;
        }
    }

    /// <summary>
    /// Determina si la consulta proyectada contiene elementos.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <returns>true si la secuencia contiene elementos; de lo contrario, false.</returns>
    public static bool Any<T, TResult>(this ProjectedQueryState<T, TResult> queryState) where T : class, new()
    {
        int? originalTake = queryState.TakeField;
        try
        {
            queryState.TakeField = 1;
            QueryResult queryResult = BuildQuery(queryState);
            string existsQuery = $"SELECT CASE WHEN EXISTS ({queryResult.SQL}) THEN 1 ELSE 0 END";

            return queryState.DbProvider.ExecuteScalar<int>(existsQuery,
                collection => collection.AddSqlParameters(queryResult.Parameters)) == 1;
        }
        finally
        {
            queryState.TakeField = originalTake;
        }
    }

    /// <summary>
    /// Ejecuta la consulta proyectada y devuelve los resultados como un arreglo.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <returns>Un arreglo de resultados proyectados.</returns>
    public static TResult[] ToArray<T, TResult>(this ProjectedQueryState<T, TResult> queryState) where T : class, new()
    {
        return queryState.ToList().ToArray();
    }

    /// <summary>
    /// Filtra los resultados de la consulta proyectada según un predicado.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <param name="predicate">Una función para probar cada elemento para una condición.</param>
    /// <returns>El estado de la consulta proyectada con el filtro aplicado.</returns>
    public static ProjectedQueryState<T, TResult> Where<T, TResult>(this ProjectedQueryState<T, TResult> queryState, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        queryState.Where.Add(predicate);
        return queryState;
    }

    /// <summary>
    /// Ordena los elementos de la consulta proyectada en orden ascendente según una clave.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <param name="expression">Una función para extraer una clave de un elemento.</param>
    /// <returns>El estado de la consulta proyectada con el ordenamiento aplicado.</returns>
    public static ProjectedQueryState<T, TResult> OrderBy<T, TResult, TKey>(this ProjectedQueryState<T, TResult> queryState, Expression<Func<T, TKey>> expression) where T : class, new()
    {
        queryState.OrderBy.Add((GetMemberName(expression.Body), true));
        return queryState;
    }

    /// <summary>
    /// Ordena los elementos de la consulta proyectada en orden descendente según una clave.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <param name="keySelector">Una función para extraer una clave de un elemento.</param>
    /// <returns>El estado de la consulta proyectada con el ordenamiento aplicado.</returns>
    public static ProjectedQueryState<T, TResult> OrderByDescending<T, TResult, TKey>(this ProjectedQueryState<T, TResult> queryState, Expression<Func<T, TKey>> keySelector) where T : class, new()
    {
        queryState.OrderBy.Add((GetMemberName(keySelector.Body), false));
        return queryState;
    }

    /// <summary>
    /// Omite un número especificado de elementos en los resultados de la consulta proyectada.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <param name="count">El número de elementos a omitir.</param>
    /// <returns>El estado de la consulta proyectada con la omisión aplicada.</returns>
    public static ProjectedQueryState<T, TResult> Skip<T, TResult>(this ProjectedQueryState<T, TResult> queryState, int count) where T : class, new()
    {
        queryState.SkipField = count;
        return queryState;
    }

    /// <summary>
    /// Devuelve un número específico de elementos contiguos desde el inicio de los resultados de la consulta proyectada.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="queryState">El estado de la consulta proyectada.</param>
    /// <param name="count">El número de elementos a devolver.</param>
    /// <returns>El estado de la consulta proyectada con la limitación aplicada.</returns>
    public static ProjectedQueryState<T, TResult> Take<T, TResult>(this ProjectedQueryState<T, TResult> queryState, int count) where T : class, new()
    {
        queryState.TakeField = count;
        return queryState;
    }
}
