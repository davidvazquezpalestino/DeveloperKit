namespace DevKit.ExecutionEngine.SQLServer.Query
{
    /// <summary>
    /// Constructor de consultas SQL para SQL Server
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    public class SqlQueryBuilder<T> where T : class, new()
    {
        private readonly string _schema;
        private readonly string _tableName;
        private readonly List<Expression<Func<T, bool>>> _whereExpressions = new();
        private readonly List<(string column, bool isAscending)> _orderByFields = new();
        private int? _take;
        private int? _skip;
        private bool _distinct;

        public SqlQueryBuilder(string schema, string tableName)
        {
            _schema = schema;
            _tableName = tableName;
        }

        /// <summary>
        /// Agrega una condición WHERE a la consulta
        /// </summary>
        public SqlQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                _whereExpressions.Add(predicate);
            }
            return this;
        }

        /// <summary>
        /// Agrega un ordenamiento a la consulta
        /// </summary>
        public SqlQueryBuilder<T> OrderBy(string column, bool ascending = true)
        {
            if (!string.IsNullOrWhiteSpace(column))
            {
                _orderByFields.Add((column, ascending));
            }

            return this;
        }

        /// <summary>
        /// Limita el número de resultados
        /// </summary>
        public SqlQueryBuilder<T> Take(int count)
        {
            _take = count > 0 ? count : _take;
            return this;
        }

        /// <summary>
        /// Salta un número específico de resultados
        /// </summary>
        public SqlQueryBuilder<T> Skip(int count)
        {
            _skip = count >= 0 ? count : _skip;
            return this;
        }

        /// <summary>
        /// Especifica que la consulta debe devolver resultados distintos
        /// </summary>
        public SqlQueryBuilder<T> Distinct()
        {
            _distinct = true;
            return this;
        }

        // Agrega más métodos según sea necesario para construir la consulta
    }
}
