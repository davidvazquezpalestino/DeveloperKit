namespace DevKit.ExecutionEngine.SQLServer.Extensions
{
    /// <summary>
    /// Métodos de extensión asíncronos para construir consultas SQL de manera fluida.
    /// </summary>
    public static partial class SqlQueryBuilderExtensions
    {
        /// <param name="queryState">El estado de la consulta</param>
        /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
        extension<T>(QueryState<T> queryState) where T : class, new()
        {
            /// <summary>
            /// Ejecuta la consulta de forma asíncrona y devuelve los resultados como una lista.
            /// </summary>
            /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
            /// <returns>Una tarea que representa la operación asíncrona, conteniendo la lista de entidades</returns>
            public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
            {
                // Construir la consulta
                QueryResult queryResult = BuildQuery(queryState);

                // Registrar la consulta que se va a ejecutar
                QueryLogger.LogQuery(
                    queryResult.SQL,
                    queryResult.Parameters,
                    IQueryLogger.LogLevel.Debug,
                    $"Iniciando consulta asíncrona ToList para {typeof(T).Name}");

                try
                {
                    // Ejecutar la consulta de forma asíncrona
                    ICollection<T> result = await queryState.DbProvider.ExecuteQueryAsListAsync(
                        queryResult.SQL,
                        reader => reader.GetItem<T>(),
                        collection => collection.AddSqlParameters(queryResult.Parameters),
                        cancellationToken);

                    // Convertir a lista
                    List<T> resultList = result.ToList();

                    // Registrar el resultado
                    QueryLogger.LogQuery(
                        queryResult.SQL,
                        queryResult.Parameters,
                        IQueryLogger.LogLevel.Debug,
                        $"Consulta asíncrona ToList completada. Se encontraron {resultList.Count} registros de {typeof(T).Name}");

                    return resultList;
                }
                catch (Exception ex)
                {
                    // Registrar el error
                    QueryLogger.LogQuery(
                        queryResult?.SQL ?? string.Empty,
                        queryResult?.Parameters,
                        IQueryLogger.LogLevel.Error,
                        $"Error en consulta asíncrona ToList para {typeof(T).Name}: {ex.Message}");

                    throw; // Relanzar la excepción para que el llamador la maneje
                }
            }

            /// <summary>
            /// Ejecuta la consulta de forma asíncrona y devuelve el primer resultado, o null si no se encuentran resultados.
            /// </summary>
            /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
            /// <returns>Una tarea que representa la operación asíncrona, conteniendo la primera entidad o null</returns>
            public async Task<T> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
            {
                // Limitar a un solo resultado
                queryState.TakeField = 1;

                // Construir la consulta
                QueryResult queryResult = BuildQuery(queryState);

                // Registrar la consulta que se va a ejecutar
                QueryLogger.LogQuery(queryResult.SQL, queryResult.Parameters,
                  message: $"Iniciando consulta asíncrona FirstOrDefault para {typeof(T).Name}");

                try
                {
                    // Ejecutar la consulta de forma asíncrona
                    ICollection<T> results = await queryState.DbProvider.ExecuteQueryAsListAsync(
                        queryResult.SQL,
                        reader => reader.GetItem<T>(),
                        collection => collection.AddSqlParameters(queryResult.Parameters),
                        cancellationToken);

                    // Obtener el primer resultado (o null)
                    T result = results.FirstOrDefault();

                    // Registrar el resultado
                    QueryLogger.LogQuery(queryResult.SQL, queryResult.Parameters,
                        IQueryLogger.LogLevel.Debug,
                        $"Consulta asíncrona FirstOrDefault completada. Se encontró {(result != null ? "1 registro" : "ningún registro")} de {typeof(T).Name}");

                    return result;
                }
                catch (Exception ex)
                {
                    // Registrar el error
                    QueryLogger.LogQuery(
                        queryResult?.SQL ?? string.Empty,
                        queryResult?.Parameters,
                        IQueryLogger.LogLevel.Error,
                        $"Error en consulta asíncrona FirstOrDefault para {typeof(T).Name}: {ex.Message}");

                    throw; // Relanzar la excepción para que el llamador la maneje
                }
            }

            /// <summary>
            /// Devuelve de forma asíncrona el primer elemento de la secuencia que satisface una condición o lanza una excepción si no se encuentra ningún elemento.
            /// </summary>
            /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
            /// <returns>Una tarea que representa la operación asíncrona, conteniendo el primer elemento</returns>
            /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos</exception>
            public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
            {
                queryState.TakeField = 1;
                QueryResult queryResult = BuildQuery(queryState);

                ICollection<T> results = await queryState.DbProvider.ExecuteQueryAsListAsync<T>(queryResult.SQL,
                    reader => reader.GetItem<T>(),
                    collection => collection.AddSqlParameters(queryResult.Parameters),
                    cancellationToken);

                return results.First();
            }

            /// <summary>
            /// Devuelve de forma asíncrona el primer elemento de la secuencia que satisface una condición o lanza una excepción si no se encuentra ningún elemento.
            /// </summary>
            /// <param name="predicate">Función para probar cada elemento para una condición</param>
            /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
            /// <returns>Una tarea que representa la operación asíncrona, conteniendo el primer elemento que cumple la condición</returns>
            /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos que cumplan la condición</exception>
            public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            {
                return await queryState.Where(predicate).FirstAsync(cancellationToken);
            }

            /// <summary>
            /// Devuelve de forma asíncrona el número total de elementos en la secuencia.
            /// </summary>
            /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
            /// <returns>Una tarea que representa la operación asíncrona, conteniendo el número total de elementos</returns>
            public async Task<int> CountAsync(CancellationToken cancellationToken = default)
            {
                // Guardar el estado original para no afectar a la consulta original
                int? originalTake = queryState.TakeField;
                int? originalSkip = queryState.SkipField;
                Expression<Func<T, object>> originalSelect = queryState.SelectExpression;

                try
                {
                    // Modificar la consulta para contar
                    queryState.TakeField = null;
                    queryState.SkipField = 0;
                    queryState.SelectExpression = null;

                    QueryResult queryResult = BuildQuery(queryState);
                    string countQuery = $"SELECT COUNT(*) FROM ({queryResult.SQL}) AS CountQuery";

                    return await queryState.DbProvider.ExecuteScalarAsync<int>(countQuery,
                        collection => collection.AddSqlParameters(queryResult.Parameters),
                        cancellationToken);
                }
                finally
                {
                    // Restaurar el estado original
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
            public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            {
                return await queryState.Where(predicate).CountAsync(cancellationToken);
            }

            /// <summary>
            /// Determina de forma asíncrona si una secuencia contiene elementos.
            /// </summary>
            /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
            /// <returns>Una tarea que representa la operación asíncrona, conteniendo true si la secuencia contiene elementos; de lo contrario, false</returns>
            public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
            {
                // Optimización: Usamos TOP 1 en lugar de COUNT para mayor eficiencia
                int? originalTake = queryState.TakeField;
                try
                {
                    queryState.TakeField = 1;
                    QueryResult queryResult = BuildQuery(queryState);
                    string existsQuery = $"SELECT CASE WHEN EXISTS ({queryResult.SQL}) THEN 1 ELSE 0 END";

                    return await queryState.DbProvider.ExecuteScalarAsync<int>(existsQuery,
                        collection => collection.AddSqlParameters(queryResult.Parameters),
                        cancellationToken) == 1;
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
            public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            {
                return await queryState.Where(predicate).AnyAsync(cancellationToken);
            }

            /// <summary>
            /// Ejecuta la consulta de forma asíncrona y devuelve los resultados como un arreglo.
            /// </summary>
            /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
            /// <returns>Una tarea que representa la operación asíncrona, conteniendo un arreglo de entidades</returns>
            public async Task<T[]> ToArrayAsync(CancellationToken cancellationToken = default)
            {
                List<T> list = await queryState.ToListAsync(cancellationToken);
                return list.ToArray();
            }
        }

        /// <param name="queryState">El estado de la consulta proyectada</param>
        /// <typeparam name="T">El tipo de entidad fuente</typeparam>
        /// <typeparam name="TResult">El tipo del resultado proyectado</typeparam>
        extension<T, TResult>(ProjectedQueryState<T, TResult> queryState) where T : class, new()
        {
            /// <summary>
            /// Ejecuta una consulta proyectada de forma asíncrona y devuelve los resultados como una lista.
            /// </summary>
            /// <param name="cancellationToken">Un token para cancelar la operación asíncrona</param>
            /// <returns>Una tarea que representa la operación asíncrona, conteniendo la lista de resultados proyectados</returns>
            public async Task<List<TResult>> ToListAsync(CancellationToken cancellationToken = default)
            {
                QueryResult queryResult = BuildQuery(queryState);

                // Registrar la consulta que se va a ejecutar
                QueryLogger.LogQuery(
                    queryResult.SQL,
                    queryResult.Parameters,
                    IQueryLogger.LogLevel.Debug,
                    $"Iniciando consulta asíncrona proyectada ToList para {typeof(T).Name} -> {typeof(TResult).Name}");

                try
                {
                    // For anonymous types or complex projections, we need to use dynamic mapping
                    if (typeof(TResult).IsAnonymousType() || typeof(TResult) != typeof(T))
                    {
                        // Use dynamic reader that can handle projections
                        ICollection<TResult> result = await queryState.DbProvider.ExecuteQueryAsListAsync(
                            queryResult.SQL,
                            reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
                            collection => collection.AddSqlParameters(queryResult.Parameters),
                            cancellationToken);

                        List<TResult> resultList = result.ToList();

                        // Registrar el resultado
                        QueryLogger.LogQuery(queryResult.SQL, queryResult.Parameters, IQueryLogger.LogLevel.Debug,
                            $"Consulta asíncrona proyectada ToList completada. Se encontraron {resultList.Count} registros proyectados");

                        return resultList;
                    }
                    else
                    {
                        // For same-type projections, use the standard mapping
                        ICollection<TResult> result = await queryState.DbProvider.ExecuteQueryAsListAsync(
                            queryResult.SQL,
                            reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
                            collection => collection.AddSqlParameters(queryResult.Parameters),
                            cancellationToken);

                        List<TResult> resultList = result.ToList();

                        // Registrar el resultado
                        QueryLogger.LogQuery(
                            queryResult.SQL,
                            queryResult.Parameters,
                            IQueryLogger.LogLevel.Debug,
                            $"Consulta asíncrona proyectada ToList completada. Se encontraron {resultList.Count} registros");

                        return resultList;
                    }
                }
                catch (Exception ex)
                {
                    // Registrar el error
                    QueryLogger.LogQuery(
                        queryResult?.SQL ?? string.Empty,
                        queryResult?.Parameters,
                        IQueryLogger.LogLevel.Error,
                        $"Error en consulta asíncrona proyectada ToList para {typeof(T).Name} -> {typeof(TResult).Name}: {ex.Message}");

                    throw; // Relanzar la excepción para que el llamador la maneje
                }
            }
        }

    }
}
