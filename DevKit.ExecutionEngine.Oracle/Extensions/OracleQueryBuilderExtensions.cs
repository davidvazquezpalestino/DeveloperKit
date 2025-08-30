namespace DevKit.ExecutionEngine.Oracle.Extensions
{
    /// <summary>
    /// Extension methods for working with Oracle query builder
    /// </summary>
    public static class OracleQueryBuilderExtensions
    {
        /// <summary>
        /// Executes a query built with the query builder and returns the results as a list.
        /// </summary>
        public static async Task<List<T>> ToListAsync<T>(this IOracleProvider provider,
            OracleQueryBuilder<T> queryBuilder,
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
                parameter => parameter.AsOracleParameters(parameters),
                cancellationToken).ConfigureAwait(false)).ToList();
        }

        /// <summary>
        /// Executes a query built with the query builder and returns the first result.
        /// </summary>
        public static async Task<T> FirstOrDefaultAsync<T>(this IOracleProvider provider,
            OracleQueryBuilder<T> queryBuilder,
            string connectionString = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            queryBuilder.Limit(1);
            (string sql, Dictionary<string, object> parameters) = queryBuilder.Build();

            ICollection<T> results = await provider.ExecuteQueryAsListAsync(
                sql,
                reader => reader.GetItem<T>(),
                parameter => parameter.AsOracleParameters(parameters),
                cancellationToken).ConfigureAwait(false);

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Executes a query built with the query builder and returns the results as a DataTable.
        /// </summary>
        public static async Task<DataTable> ToDataTableAsync<T>(this IOracleProvider provider,
            OracleQueryBuilder<T> queryBuilder,
            string connectionString = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            if (queryBuilder == null)
            {
                throw new ArgumentNullException(nameof(queryBuilder));
            }

            (string sql, Dictionary<string, object> parameters) = queryBuilder.Build();

            return await provider.ExecuteQueryAsTableAsync(
                sql,
                parameter => parameter.AsOracleParameters(parameters),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query built with the query builder and returns the number of affected rows.
        /// </summary>
        public static async Task<int> ExecuteNonQueryAsync<T>(this IOracleProvider provider,
            OracleQueryBuilder<T> queryBuilder,
            string connectionString = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            if (queryBuilder == null)
            {
                throw new ArgumentNullException(nameof(queryBuilder));
            }

            (string sql, Dictionary<string, object> parameters) = queryBuilder.Build();

            return await provider.ExecuteNonQueryAsync(
                sql,
                parameter => parameter.AsOracleParameters(parameters),
                cancellationToken).ConfigureAwait(false);
        }
    }
}
