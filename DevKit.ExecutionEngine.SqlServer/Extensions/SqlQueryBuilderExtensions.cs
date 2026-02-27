
namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Métodos de extensión para construir consultas SQL de manera fluida.
/// </summary>
public static partial class SqlQueryBuilderExtensions
{
    /// <summary>
    /// Inicia la construcción de una consulta SQL para un tipo de entidad específico.
    /// </summary>
    /// <param name="repository">La instancia del proveedor de base de datos.</param>
    /// <typeparam name="T">El tipo de entidad a consultar.</typeparam>
    /// <param name="schema">El nombre del esquema de la base de datos (ej. "dbo").</param>
    /// <param name="tableName">El nombre de la tabla (opcional, usa el nombre de la clase si no se proporciona).</param>
    /// <returns>Una nueva instancia del constructor de consultas <see cref="QueryState{T}"/>.</returns>
    public static QueryState<T> From<T>(this ISQLServerProvider repository, string schema = "dbo", string tableName = null) where T : class, new()
    {
        return new QueryState<T>(repository, schema, tableName);
    }

    /// <summary>
    /// Ejecuta la consulta y devuelve los resultados como una lista de entidades.
    /// </summary>
    /// <param name="query">El estado de la consulta.</param>
    /// <typeparam name="T">El tipo de entidad que se está consultando.</typeparam>
    /// <returns>Una lista de entidades que coinciden con la consulta.</returns>
    public static List<T> ToList<T>(this QueryState<T> query) where T : class, new()
    {
        QueryResult queryResult = BuildQuery(query);

        ICollection<T> result = query.DbProvider.ExecuteQueryAsList(queryResult.SQL,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters));

        List<T> resultList = result.ToList();

        return resultList;
    }

    /// <summary>
    /// Especifica qué propiedades incluir en los resultados de la consulta.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="query">El estado de la consulta original.</param>
    /// <param name="expression">Una expresión lambda que especifica qué propiedades incluir.</param>
    /// <returns>El estado de la consulta con la expresión de selección aplicada.</returns>
    public static ProjectedQueryState<T, TResult> Select<T, TResult>(this QueryState<T> query, Expression<Func<T, TResult>> expression) where T : class, new()
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        ProjectedQueryState<T, TResult> newState = new(query.DbProvider, query.Schema, query.TableName)
        {
            TakeField = query.TakeField,
            SkipField = query.SkipField,
            SelectExpression = expression
        };

        newState.Where.AddRange(query.Where);
        newState.OrderBy.AddRange(query.OrderBy);

        return newState;
    }

    /// <summary>
    /// Ejecuta la consulta y devuelve el primer resultado, o null si no se encuentran resultados.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <returns>La primera entidad que coincide con la consulta, o null.</returns>
    public static T FirstOrDefault<T>(this QueryState<T> query) where T : class, new()
    {
        query.TakeField = 1;

        QueryResult queryResult = BuildQuery(query);

        ICollection<T> results = query.DbProvider.ExecuteQueryAsList(
            queryResult.SQL,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters));

        T result = results.FirstOrDefault();

        return result;
    }

    /// <summary>
    /// Devuelve el primer elemento de la secuencia que satisface una condición o lanza una excepción si no se encuentra ningún elemento.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <returns>El primer elemento que coincide con la consulta.</returns>
    /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos.</exception>
    public static T First<T>(this QueryState<T> query) where T : class, new()
    {
        query.TakeField = 1;
        QueryResult queryResult = BuildQuery(query);

        ICollection<T> results = query.DbProvider.ExecuteQueryAsList(queryResult.SQL,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters));

        T result = results.First();

        return result;
    }

    /// <summary>
    /// Devuelve el primer elemento de la secuencia que satisface una condición o lanza una excepción si no se encuentra ningún elemento.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <param name="predicate">Función para probar cada elemento para una condición.</param>
    /// <returns>El primer elemento que coincide con la condición.</returns>
    /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos que cumplan la condición.</exception>
    public static T First<T>(this QueryState<T> query, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        return query.Where(predicate).First();
    }

    /// <summary>
    /// Devuelve el número total de elementos en la secuencia.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <returns>El número total de elementos en la secuencia.</returns>
    public static int Count<T>(this QueryState<T> query) where T : class, new()
    {
        int? originalTake = query.TakeField;
        int? originalSkip = query.SkipField;
        Expression<Func<T, object>> originalSelect = query.SelectExpression;

        try
        {
            query.TakeField = null;
            query.SkipField = 0;
            query.SelectExpression = null;

            QueryResult queryResult = BuildQuery(query);
            string countQuery = $"SELECT COUNT(*) FROM ({queryResult.SQL}) AS CountQuery";

            int result = query.DbProvider.ExecuteScalar<int>(countQuery,
                collection => collection.AddSqlParameters(queryResult.Parameters));

            return result;
        }
        finally
        {
            query.TakeField = originalTake;
            query.SkipField = originalSkip;
            query.SelectExpression = originalSelect;
        }
    }

    /// <summary>
    /// Devuelve el número de elementos de la secuencia que satisfacen una condición.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <param name="predicate">Función para probar cada elemento para una condición.</param>
    /// <returns>El número de elementos de la secuencia que satisfacen la condición.</returns>
    public static int Count<T>(this QueryState<T> query, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        return query.Where(predicate).Count();
    }

    /// <summary>
    /// Determina si una secuencia contiene elementos.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <returns>true si la secuencia contiene elementos; de lo contrario, false.</returns>
    public static bool Any<T>(this QueryState<T> query) where T : class, new()
    {
        int? originalTake = query.TakeField;
        try
        {
            query.TakeField = 1;
            QueryResult queryResult = BuildQuery(query);
            string existsQuery = $"SELECT CASE WHEN EXISTS ({queryResult.SQL}) THEN 1 ELSE 0 END";

            bool result = query.DbProvider.ExecuteScalar<int>(existsQuery,
                collection => collection.AddSqlParameters(queryResult.Parameters)) == 1;

            return result;
        }
        finally
        {
            query.TakeField = originalTake;
        }
    }

    /// <summary>
    /// Determina si algún elemento de una secuencia satisface una condición.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <param name="predicate">Función para probar cada elemento para una condición.</param>
    /// <returns>true si algún elemento de la secuencia supera la prueba en el predicado especificado; de lo contrario, false.</returns>
    public static bool Any<T>(this QueryState<T> query, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        return query.Where(predicate).Any();
    }

    /// <summary>
    /// Ejecuta la consulta y devuelve los resultados como un arreglo.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <returns>Un arreglo de entidades que coinciden con la consulta.</returns>
    public static T[] ToArray<T>(this QueryState<T> query) where T : class, new()
    {
        return query.ToList().ToArray();
    }

    /// <summary>
    /// Filtra los resultados de la consulta según un predicado.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <param name="predicate">Una función para probar cada elemento para una condición.</param>
    /// <returns>El estado de la consulta con el filtro aplicado.</returns>
    public static QueryState<T> Where<T>(this QueryState<T> query, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        query.Where.Add(predicate);
        return query;
    }

    /// <summary>
    /// Ordena los elementos de la consulta en orden ascendente según una clave.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <param name="expression">Una función para extraer una clave de un elemento.</param>
    /// <returns>El estado de la consulta con el ordenamiento aplicado.</returns>
    public static QueryState<T> OrderBy<T, TKey>(this QueryState<T> query, Expression<Func<T, TKey>> expression) where T : class, new()
    {
        query.OrderBy.Add((GetMemberName(expression.Body), true));
        return query;
    }

    /// <summary>
    /// Ordena los elementos de la consulta en orden descendente según una clave.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <param name="keySelector">Una función para extraer una clave de un elemento.</param>
    /// <returns>El estado de la consulta con el ordenamiento aplicado.</returns>
    public static QueryState<T> OrderByDescending<T, TKey>(this QueryState<T> query, Expression<Func<T, TKey>> keySelector) where T : class, new()
    {
        query.OrderBy.Add((GetMemberName(keySelector.Body), false));
        return query;
    }

    /// <summary>
    /// Realiza una ordenación posterior de los elementos en una secuencia en orden descendente.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <param name="keySelector">Una función para extraer una clave de un elemento.</param>
    /// <returns>El estado de la consulta con la ordenación posterior aplicada.</returns>
    public static QueryState<T> ThenByDescending<T, TKey>(this QueryState<T> query, Expression<Func<T, TKey>> keySelector) where T : class, new()
    {
        if (query.OrderBy.Count == 0)
        {
            throw new InvalidOperationException("ThenByDescending must be called after OrderBy or OrderByDescending");
        }

        query.OrderBy.Add((GetMemberName(keySelector.Body), false));
        return query;
    }

    /// <summary>
    /// Omite un número especificado de elementos en los resultados de la consulta y luego devuelve los elementos restantes.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <param name="count">El número de elementos a omitir.</param>
    /// <returns>El estado de la consulta con la omisión aplicada.</returns>
    public static QueryState<T> Skip<T>(this QueryState<T> query, int count) where T : class, new()
    {
        query.SkipField = count;
        return query;
    }

    /// <summary>
    /// Devuelve un número específico de elementos contiguos desde el inicio de los resultados de la consulta.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <param name="count">El número de elementos a devolver.</param>
    /// <returns>El estado de la consulta con la limitación aplicada.</returns>
    public static QueryState<T> Take<T>(this QueryState<T> query, int count) where T : class, new()
    {
        query.TakeField = count;
        return query;
    }
}