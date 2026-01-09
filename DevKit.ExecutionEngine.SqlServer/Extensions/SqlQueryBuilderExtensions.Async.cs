namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Métodos de extensión asíncronos para construir consultas SQL de manera fluida.
/// </summary>
public static partial class SqlQueryBuilderExtensions
{
    /// <summary>
    /// Ejecuta la consulta de forma asíncrona y devuelve los resultados como una lista.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
    /// <returns>Una tarea que representa la operación asíncrona, conteniendo la lista de entidades</returns>
    public static async Task<List<T>> ToListAsync<T>(this QueryState<T> queryState, CancellationToken cancellationToken = default) where T : class, new()
    {
        QueryResult queryResult = BuildQuery(queryState);
        ICollection<T> result = await queryState.DbProvider.ExecuteQueryAsListAsync(
            queryResult.SQL,
            reader => reader.GetItem<T>(),
            dbParameters: param => param.AddSqlParameters(queryResult.Parameters),
            cancellationToken).ConfigureAwait(false);

        return result.ToList();
    }

    /// <summary>
    /// Ejecuta la consulta de forma asíncrona y devuelve el primer resultado, o null si no se encuentran resultados.
    /// </summary>
    public static async Task<T> FirstOrDefaultAsync<T>(this QueryState<T> queryState, CancellationToken cancellationToken = default) where T : class, new()
    {
        queryState.TakeField = 1;
        QueryResult queryResult = BuildQuery(queryState);

        return await queryState.DbProvider.ExecuteQueryAsSingleAsync(
            queryResult.SQL,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Devuelve de forma asíncrona el primer elemento de la secuencia que satisface una condición o lanza una excepción si no se encuentra ningún elemento.
    /// </summary>
    /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
    /// <returns>Una tarea que representa la operación asíncrona, conteniendo el primer elemento</returns>
    /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos</exception>
    public static async Task<T> FirstAsync<T>(this QueryState<T> queryState, CancellationToken cancellationToken = default) where T : class, new()
    {
        queryState.TakeField = 1;
        QueryResult queryResult = BuildQuery(queryState);

        return await queryState.DbProvider.ExecuteQueryAsSingleAsync<T>(queryResult.SQL,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Devuelve de forma asíncrona el primer elemento de la secuencia que satisface una condición o lanza una excepción si no se encuentra ningún elemento.
    /// </summary>
    /// <param name="predicate">Función para probar cada elemento para una condición</param>
    /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
    /// <returns>Una tarea que representa la operación asíncrona, conteniendo el primer elemento que cumple la condición</returns>
    /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos que cumplan la condición</exception>
    public static async Task<T> FirstAsync<T>(this QueryState<T> queryState, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class, new()
    {
        return await queryState.Where(predicate).FirstAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Devuelve de forma asíncrona el número total de elementos en la secuencia.
    /// </summary>
    /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
    /// <returns>Una tarea que representa la operación asíncrona, conteniendo el número total de elementos</returns>
    public static async Task<int> CountAsync<T>(this QueryState<T> queryState, CancellationToken cancellationToken = default) where T : class, new()
    {
        int? originalTake = queryState.TakeField;
        int? originalSkip = queryState.SkipField;
        Expression<Func<T, object>> originalSelect = queryState.SelectExpression;

        try
        {
            queryState.TakeField = null;
            queryState.SkipField = 0;
            queryState.SelectExpression = null;

            QueryResult queryResult = BuildQuery(queryState);
            string countQuery = $"SELECT COUNT(*) FROM ({queryResult.SQL}) AS CountQuery";

            return await queryState.DbProvider.ExecuteScalarAsync<int>(countQuery,
                collection => collection.AddSqlParameters(queryResult.Parameters),
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            queryState.TakeField = originalTake;
            queryState.SkipField = originalSkip;
            queryState.SelectExpression = originalSelect;
        }
    }

    /// <summary>
    /// Devuelve de forma asíncrona el número de elementos de la secuencia que satisfacen una condición.
    /// </summary>
    /// <param name="predicate">Función para probar cada elemento para una condición</param>
    /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
    /// <returns>Una tarea que representa la operación asíncrona, conteniendo el número de elementos que satisfacen la condición</returns>
    public static async Task<int> CountAsync<T>(this QueryState<T> queryState, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class, new()
    {
        return await queryState.Where(predicate).CountAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determina de forma asíncrona si una secuencia contiene elementos.
    /// </summary>
    /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
    /// <returns>Una tarea que representa la operación asíncrona, conteniendo true si la secuencia contiene elementos; de lo contrario, false</returns>
    public static async Task<bool> AnyAsync<T>(this QueryState<T> queryState, CancellationToken cancellationToken = default) where T : class, new()
    {
        int? originalTake = queryState.TakeField;
        try
        {
            queryState.TakeField = 1;
            QueryResult queryResult = BuildQuery(queryState);
            string existsQuery = $"SELECT CASE WHEN EXISTS ({queryResult.SQL}) THEN 1 ELSE 0 END";

            return await queryState.DbProvider.ExecuteScalarAsync<int>(existsQuery,
                collection => collection.AddSqlParameters(queryResult.Parameters),
                cancellationToken).ConfigureAwait(false) == 1;
        }
        finally
        {
            queryState.TakeField = originalTake;
        }
    }

    /// <summary>
    /// Determina de forma asíncrona si algún elemento de una secuencia satisface una condición.
    /// </summary>
    /// <param name="predicate">Función para probar cada elemento para una condición</param>
    /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
    /// <returns>Una tarea que representa la operación asíncrona, conteniendo true si algún elemento de la secuencia supera la prueba en el predicado especificado; de lo contrario, false</returns>
    public static async Task<bool> AnyAsync<T>(this QueryState<T> queryState, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class, new()
    {
        return await queryState.Where(predicate).AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Ejecuta la consulta de forma asíncrona y devuelve los resultados como un arreglo.
    /// </summary>
    /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
    /// <returns>Una tarea que representa la operación asíncrona, conteniendo un arreglo de entidades</returns>
    public static async Task<T[]> ToArrayAsync<T>(this QueryState<T> queryState, CancellationToken cancellationToken = default) where T : class, new()
    {
        List<T> list = await queryState.ToListAsync(cancellationToken).ConfigureAwait(false);
        return list.ToArray();
    }

    /// <summary>
    /// Ejecuta una consulta proyectada de forma asíncrona y devuelve los resultados como una lista.
    /// </summary>
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ProjectedQueryState<T, TResult> queryState, CancellationToken cancellationToken = default) where T : class, new()
    {
        QueryResult queryResult = BuildQuery(queryState);

        try
        {
            ICollection<TResult> result = await queryState.DbProvider.ExecuteQueryAsListAsync(
                queryResult.SQL,
                reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
                collection => collection.AddSqlParameters(queryResult.Parameters),
                cancellationToken).ConfigureAwait(false);

            return result.ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en consulta asíncrona proyectada ToList para {typeof(T).Name} -> {typeof(TResult).Name}: {ex.Message}");
        }
    }
}