namespace DevKit.ExecutionEngine.MySql.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para construir y ejecutar consultas MySQL de manera fluida y segura en tipos.
    /// Esta clase simplifica el proceso de creación y ejecución de consultas de base de datos contra una base de datos MySQL.
    /// </summary>
    public static class MySqlQueryBuilderExtensions
    {
        /// <summary>
        /// Crea un nuevo constructor de consultas para la tabla especificada.
        /// </summary>
        /// <param name="provider">La instancia del proveedor de base de datos MySQL.</param>
        /// <param name="tableName">Opcional. El nombre de la tabla a consultar. Si no se proporciona, se usará el nombre del tipo T.</param>
        /// <typeparam name="T">El tipo de entidad a consultar.</typeparam>
        /// <returns>Una nueva instancia de <see cref="MySqlQueryBuilder{T}"/> configurada para la tabla especificada.</returns>
        public static MySqlQueryBuilder<T> From<T>(
            this IMySqlProvider provider,
            string tableName = null) where T : class, new()
        {
            return new MySqlQueryBuilder<T>(provider).From(tableName ?? typeof(T).Name);
        }

        /// <summary>
        /// Ejecuta una consulta construida con el constructor de consultas y devuelve los resultados como una lista.
        /// </summary>
        /// <typeparam name="T">El tipo de entidad a devolver.</typeparam>
        /// <param name="provider">La instancia del proveedor de base de datos MySQL.</param>
        /// <param name="queryBuilder">El constructor de consultas que contiene la consulta a ejecutar.</param>
        /// <param name="connectionString">Opcional. La cadena de conexión a utilizar. Si no se proporciona, se usará la cadena de conexión predeterminada del proveedor.</param>
        /// <param name="cancellationToken">Un token para cancelar la operación asíncrona.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea contiene una lista de entidades.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando <paramref name="queryBuilder"/> es nulo.</exception>
        /// <example>
        /// <code>
        /// var clientes = await proveedor.ListarAsync(constructorConsulta);
        /// </code>
        /// </example>
        public static async Task<List<T>> ToListAsync<T>(
            this IMySqlProvider provider,
            MySqlQueryBuilder<T> queryBuilder,
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
                parameter => parameter.AsMySqlParameters(parameters),
                cancellationToken).ConfigureAwait(false)).ToList();
        }

        /// <summary>
        /// Ejecuta una consulta construida con el constructor de consultas y devuelve el primer resultado, o nulo si no se encuentran resultados.
        /// </summary>
        /// <typeparam name="T">El tipo de entidad a devolver.</typeparam>
        /// <param name="provider">La instancia del proveedor de base de datos MySQL.</param>
        /// <param name="queryBuilder">El constructor de consultas que contiene la consulta a ejecutar.</param>
        /// <param name="connectionString">Opcional. La cadena de conexión a utilizar. Si no se proporciona, se usará la cadena de conexión predeterminada del proveedor.</param>
        /// <param name="cancellationToken">Un token para cancelar la operación asíncrona.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea contiene la primera entidad o nulo si no se encuentran resultados.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando <paramref name="queryBuilder"/> es nulo.</exception>
        /// <example>
        /// <code>
        /// var cliente = await proveedor.PrimeroOPorDefectoAsync(constructorConsulta);
        /// </code>
        /// </example>
        public static async Task<T> FirstOrDefaultAsync<T>(
            this IMySqlProvider provider,
            MySqlQueryBuilder<T> queryBuilder,
            string connectionString = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            queryBuilder.Limit(1);
            (string sql, Dictionary<string, object> parameters) = queryBuilder.Build();

            ICollection<T> results = await provider.ExecuteQueryAsListAsync(
                sql,
                reader => reader.GetItem<T>(),
                parameter => parameter.AsMySqlParameters(parameters),
                cancellationToken).ConfigureAwait(false);

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Ejecuta una consulta construida con el constructor de consultas y devuelve los resultados como un DataTable.
        /// </summary>
        /// <typeparam name="T">El tipo de entidad que se está consultando.</typeparam>
        /// <param name="provider">La instancia del proveedor de base de datos MySQL.</param>
        /// <param name="queryBuilder">El constructor de consultas que contiene la consulta a ejecutar.</param>
        /// <param name="connectionString">Opcional. La cadena de conexión a utilizar. Si no se proporciona, se usará la cadena de conexión predeterminada del proveedor.</param>
        /// <param name="cancellationToken">Un token para cancelar la operación asíncrona.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea contiene un DataTable con los resultados de la consulta.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando <paramref name="queryBuilder"/> es nulo.</exception>
        /// <example>
        /// <code>
        /// DataTable tablaDatos = await proveedor.ATablaDatosAsync(constructorConsulta);
        /// </code>
        /// </example>
        public static async Task<DataTable> ToDataTableAsync<T>(
            this IMySqlProvider provider,
            MySqlQueryBuilder<T> queryBuilder,
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
                parameter => parameter.AsMySqlParameters(parameters),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Ejecuta una operación que no devuelve resultados (INSERT, UPDATE, DELETE) construida con el constructor de consultas y devuelve el número de filas afectadas.
        /// </summary>
        /// <typeparam name="T">El tipo de entidad sobre la que se está operando.</typeparam>
        /// <param name="provider">La instancia del proveedor de base de datos MySQL.</param>
        /// <param name="queryBuilder">El constructor de consultas que contiene el comando a ejecutar.</param>
        /// <param name="connectionString">Opcional. La cadena de conexión a utilizar. Si no se proporciona, se usará la cadena de conexión predeterminada del proveedor.</param>
        /// <param name="cancellationToken">Un token para cancelar la operación asíncrona.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea contiene el número de filas afectadas.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando <paramref name="queryBuilder"/> es nulo.</exception>
        /// <example>
        /// <code>
        /// int filasAfectadas = await proveedor.EjecutarSinResultadoAsync(constructorConsulta);
        /// </code>
        /// </example>
        public static async Task<int> ExecuteNonQueryAsync<T>(
            this IMySqlProvider provider,
            MySqlQueryBuilder<T> queryBuilder,
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
                parameter => parameter.AsMySqlParameters(parameters),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Agrega una cláusula WHERE a la consulta usando una expresión fuertemente tipada.
        /// </summary>
        /// <typeparam name="T">El tipo de entidad que se está consultando.</typeparam>
        /// <param name="queryBuilder">El constructor de consultas al que se le agregará la cláusula WHERE.</param>
        /// <param name="predicate">Una función para probar cada elemento según una condición.</param>
        /// <returns>La instancia del constructor de consultas con la cláusula WHERE agregada.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando <paramref name="predicate"/> es nulo.</exception>
        /// <example>
        /// <code>
        /// var consulta = proveedor.Desde&lt;Cliente&gt;()
        ///     .Donde(c => c.Edad > 18 && c.Activo);
        /// </code>
        /// </example>
        public static MySqlQueryBuilder<T> Where<T>(
            this MySqlQueryBuilder<T> queryBuilder,
            Expression<Func<T, bool>> predicate) where T : class, new()
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            MySqlExpressionVisitor visitor = new MySqlExpressionVisitor();
            visitor.Visit(predicate);
            (string whereClause, Dictionary<string, object> parameters) = visitor.GetWhereClause();

            // Add the parameters to the query builder
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    queryBuilder.AddParameter(param.Key, param.Value);
                }
            }

            // Add the where clause
            if (!string.IsNullOrEmpty(whereClause))
            {
                queryBuilder.Where(whereClause);
            }

            return queryBuilder;
        }

    }
}
