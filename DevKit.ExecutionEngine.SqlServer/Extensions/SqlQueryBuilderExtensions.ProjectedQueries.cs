using DevKit.ExecutionEngine.SQLServer.Logging;

namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Métodos de extensión para consultas proyectadas
/// </summary>
public static partial class SqlQueryBuilderExtensions
{
    /// <param name="queryState">The projected query state</param>
    /// <typeparam name="T">The source entity type</typeparam>
    /// <typeparam name="TResult">The projected result type</typeparam>
    extension<T, TResult>(ProjectedQueryState<T, TResult> queryState) where T : class, new()
    {
        /// <summary>
        /// Executes a projected query and returns the results as a list.
        /// </summary>
        /// <returns>A list of projected results</returns>
        public List<TResult> ToList()
        {
            QueryResult queryResult = BuildQuery(queryState);

            // Registrar la consulta que se va a ejecutar
            QueryLogger.LogQuery(
                queryResult.SQL,
                queryResult.Parameters,
                IQueryLogger.LogLevel.Debug,
                $"Ejecutando consulta proyectada ToList para {typeof(T).Name} -> {typeof(TResult).Name}");

            // For anonymous types or complex projections, we need to use dynamic mapping
            if (typeof(TResult).IsAnonymousType() || typeof(TResult) != typeof(T))
            {
                // Use dynamic reader that can handle projections
                ICollection<TResult> result = queryState.DbProvider.ExecuteQueryAsList(queryResult.SQL,
                    reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
                    collection => collection.AddSqlParameters(queryResult.Parameters));

                List<TResult> resultList = result.ToList();

                // Registrar el resultado
                QueryLogger.LogQuery(
                    queryResult.SQL,
                    queryResult.Parameters,
                    IQueryLogger.LogLevel.Debug,
                    $"Consulta proyectada ToList completada. Se encontraron {resultList.Count} registros proyectados");

                return resultList;
            }
            else
            {
                // For same-type projections, use the standard mapping
                ICollection<TResult> result = queryState.DbProvider.ExecuteQueryAsList(queryResult.SQL,
                    reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
                    collection => collection.AddSqlParameters(queryResult.Parameters));

                List<TResult> resultList = result.ToList();

                // Registrar el resultado
                QueryLogger.LogQuery(
                    queryResult.SQL,
                    queryResult.Parameters,
                    IQueryLogger.LogLevel.Debug,
                    $"Consulta proyectada ToList completada. Se encontraron {resultList.Count} registros");

                return resultList;
            }
        }

        /// <summary>
        /// Ejecuta la consulta proyectada y devuelve el primer resultado, o null si no se encuentran resultados.
        /// </summary>
        /// <returns>El primer resultado proyectado o null</returns>
        public TResult FirstOrDefault()
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
        /// <returns>El primer resultado proyectado</returns>
        /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos</exception>
        public TResult First()
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
        /// <returns>El número total de elementos</returns>
        public int Count()
        {
            // Guardar el estado original
            int? originalTake = queryState.TakeField;
            int? originalSkip = queryState.SkipField;

            try
            {
                // Modificar la consulta para contar
                queryState.TakeField = null;
                queryState.SkipField = 0;

                QueryResult queryResult = BuildQuery(queryState);
                string countQuery = $"SELECT COUNT(*) FROM ({queryResult.SQL}) AS CountQuery";

                return queryState.DbProvider.ExecuteScalar<int>(countQuery,
                    collection => collection.AddSqlParameters(queryResult.Parameters));
            }
            finally
            {
                // Restaurar el estado original
                queryState.TakeField = originalTake;
                queryState.SkipField = originalSkip;
            }
        }

        /// <summary>
        /// Determina si la consulta proyectada contiene elementos.
        /// </summary>
        /// <returns>true si la secuencia contiene elementos; de lo contrario, false</returns>
        public bool Any()
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
        /// <returns>Un arreglo de resultados proyectados</returns>
        public TResult[] ToArray()
        {
            return queryState.ToList().ToArray();
        }

        /// <summary>
        /// Filtra los resultados de la consulta proyectada según un predicado.
        /// </summary>
        /// <param name="predicate">Una función para probar cada elemento para una condición</param>
        /// <returns>El estado de la consulta proyectada con el filtro aplicado</returns>
        public ProjectedQueryState<T, TResult> Where(Expression<Func<T, bool>> predicate)
        {
            queryState.Where.Add(predicate);
            return queryState;
        }

        /// <summary>
        /// Ordena los elementos de la consulta proyectada en orden ascendente según una clave.
        /// </summary>
        /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector</typeparam>
        /// <param name="expression">Una función para extraer una clave de un elemento</param>
        /// <returns>El estado de la consulta proyectada con el ordenamiento aplicado</returns>
        public ProjectedQueryState<T, TResult> OrderBy<TKey>(Expression<Func<T, TKey>> expression)
        {
            queryState.OrderBy.Add((GetMemberName(expression.Body), true));
            return queryState;
        }

        /// <summary>
        /// Ordena los elementos de la consulta proyectada en orden descendente según una clave.
        /// </summary>
        /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector</typeparam>
        /// <param name="keySelector">Una función para extraer una clave de un elemento</param>
        /// <returns>El estado de la consulta proyectada con el ordenamiento aplicado</returns>
        public ProjectedQueryState<T, TResult> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            queryState.OrderBy.Add((GetMemberName(keySelector.Body), false));
            return queryState;
        }

        /// <summary>
        /// Omite un número especificado de elementos en los resultados de la consulta proyectada.
        /// </summary>
        /// <param name="count">El número de elementos a omitir</param>
        /// <returns>El estado de la consulta proyectada con la omisión aplicada</returns>
        public ProjectedQueryState<T, TResult> Skip(int count)
        {
            queryState.SkipField = count;
            return queryState;
        }

        /// <summary>
        /// Devuelve un número específico de elementos contiguos desde el inicio de los resultados de la consulta proyectada.
        /// </summary>
        /// <param name="count">El número de elementos a devolver</param>
        /// <returns>El estado de la consulta proyectada con la limitación aplicada</returns>
        public ProjectedQueryState<T, TResult> Take(int count)
        {
            queryState.TakeField = count;
            return queryState;
        }
    }
}
