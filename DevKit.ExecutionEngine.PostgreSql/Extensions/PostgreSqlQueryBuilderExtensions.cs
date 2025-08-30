namespace DevKit.ExecutionEngine.PostgreSQL.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IPostgreSqlProvider"/> to support query building.
    /// </summary>
    public static class PostgreSqlQueryBuilderExtensions
    {
        /// <summary>
        /// Creates a new query builder for the specified entity type.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="provider">The database provider</param>
        /// <returns>A new query builder instance</returns>
        public static PostgreSqlQueryBuilder<T> Query<T>(this IPostgreSqlProvider provider) where T : class, new()
        {
            return new PostgreSqlQueryBuilder<T>();
        }

        /// <summary>
        /// Executes a query built with the query builder and returns the results as a list.
        /// </summary>
        public static async Task<List<T>> ToListAsync<T>(
            this IPostgreSqlProvider provider,
            PostgreSqlQueryBuilder<T> queryBuilder,
            string connectionString = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            if (queryBuilder == null)
            {
                throw new ArgumentNullException(nameof(queryBuilder));
            }

            (string sql, Dictionary<string, object> parameters) = queryBuilder.Build();

            return (await provider.ExecuteQueryAsListAsync(
                sql,
                reader => reader.GetItem<T>(),
                p =>
                {
                    if (parameters != null)
                    {
                        foreach (KeyValuePair<string, object> param in parameters)
                        {
                            p.AddPosgreParameter(param.Key, param.Value);
                        }
                    }
                },
                cancellationToken).ConfigureAwait(false)).ToList();
        }

        /// <summary>
        /// Executes a query built with the query builder and returns the first result.
        /// </summary>
        public static async Task<T> FirstOrDefaultAsync<T>(
            this IPostgreSqlProvider provider,
            PostgreSqlQueryBuilder<T> queryBuilder,
            string connectionString = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            if (queryBuilder == null)
            {
                throw new ArgumentNullException(nameof(queryBuilder));
            }

            // Apply limit 1 to optimize the query
            queryBuilder.Limit(1);
            (string sql, Dictionary<string, object> parameters) = queryBuilder.Build();

            T result = await provider.ExecuteQueryAsSingleAsync(sql,
                reader => reader.GetItem<T>(),
                p =>
                {
                    if (parameters != null)
                    {
                        foreach (KeyValuePair<string, object> param in parameters)
                        {
                            p.AddPosgreParameter(param.Key, param.Value);
                        }
                    }
                },
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Executes a query built with the query builder and returns the result as a data table.
        /// </summary>
        public static async Task<DataTable> ToDataTableAsync<T>(
            this IPostgreSqlProvider provider,
            PostgreSqlQueryBuilder<T> queryBuilder,
            string connectionString = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            if (queryBuilder == null)
            {
                throw new ArgumentNullException(nameof(queryBuilder));
            }

            (string sql, Dictionary<string, object> parameters) = queryBuilder.Build();
            return await provider.ExecuteQueryAsTableAsync(sql, p =>
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        p.AddPosgreParameter(param.Key, param.Value);
                    }
                }
            }, cancellationToken)
                .ConfigureAwait(false);
        }



        /// <summary>
        /// Executes a query built with the query builder and returns the number of affected rows.
        /// </summary>
        public static async Task<int> ExecuteNonQueryAsync<T>(
            this IPostgreSqlProvider provider,
            PostgreSqlQueryBuilder<T> queryBuilder,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            if (queryBuilder == null)
            {
                throw new ArgumentNullException(nameof(queryBuilder));
            }

            (string sql, Dictionary<string, object> parameters) = queryBuilder.Build();
            return await provider.ExecuteNonQueryAsync(sql, parameter =>
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        parameter.AddPosgreParameter(param.Key, param.Value);
                    }
                }
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
