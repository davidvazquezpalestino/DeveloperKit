namespace DevKit.ExecutionEngine.Oracle.QueryBuilder
{
    /// <summary>
    /// A query builder for Oracle databases
    /// </summary>
    /// <typeparam name="T">The entity type this query builder works with</typeparam>
    public class OracleQueryBuilder<T> where T : class, new()
    {
        private readonly List<string> SelectColumns = new();
        private readonly List<string> WhereClauses = new();
        private readonly Dictionary<string, object> Parameters = new();
        private string TableName;
        private string OrderByClause;
        private int? LimitField;
        private int? Offset;
        private bool IsDistinct;

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleQueryBuilder{T}"/> class.
        /// </summary>
        public OracleQueryBuilder()
        {
            TableName = typeof(T).Name;
        }

        /// <summary>
        /// Specifies the table name to query from
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>The query builder instance</returns>
        public OracleQueryBuilder<T> From(string tableName)
        {
            TableName = tableName;
            return this;
        }

        /// <summary>
        /// Adds a WHERE clause to the query
        /// </summary>
        /// <param name="condition">The condition (e.g., "ColumnName = :paramName")</param>
        /// <param name="parameters">Optional parameters for the condition</param>
        /// <returns>The query builder instance</returns>
        public OracleQueryBuilder<T> Where(string condition, IDictionary<string, object> parameters = null)
        {
            WhereClauses.Add(condition);
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    Parameters[param.Key] = param.Value;
                }
            }
            return this;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the query
        /// </summary>
        /// <param name="column">The column to order by</param>
        /// <param name="descending">Whether to sort in descending order</param>
        /// <returns>The query builder instance</returns>
        public OracleQueryBuilder<T> OrderBy(string column, bool descending = false)
        {
            OrderByClause = $"ORDER BY {column} {(descending ? "DESC" : "ASC")}";
            return this;
        }

        /// <summary>
        /// Limits the number of rows returned
        /// </summary>
        /// <param name="count">Maximum number of rows to return</param>
        /// <param name="offset">Number of rows to skip</param>
        /// <returns>The query builder instance</returns>
        public OracleQueryBuilder<T> Limit(int count, int? offset = null)
        {
            LimitField = count;
            Offset = offset;
            return this;
        }

        /// <summary>
        /// Sets the SELECT DISTINCT flag
        /// </summary>
        /// <returns>The query builder instance</returns>
        public OracleQueryBuilder<T> Distinct()
        {
            IsDistinct = true;
            return this;
        }

        /// <summary>
        /// Builds the SQL query and parameters
        /// </summary>
        /// <returns>A tuple containing the SQL query and parameters</returns>
        public (string Sql, Dictionary<string, object> Parameters) Build()
        {
            StringBuilder sql = new StringBuilder();

            // Build SELECT clause
            sql.Append("SELECT ");
            if (IsDistinct)
            {
                sql.Append("DISTINCT ");
            }
            sql.Append(SelectColumns.Any() ? string.Join(", ", SelectColumns) : "*");
            sql.Append(" FROM ").Append(TableName);

            // Build WHERE clause
            if (WhereClauses.Any())
            {
                sql.Append(" WHERE ").Append(string.Join(" AND ", WhereClauses));
            }

            // Add ORDER BY if specified
            if (!string.IsNullOrEmpty(OrderByClause))
            {
                sql.Append(" ").Append(OrderByClause);
            }

            // Handle pagination for Oracle
            if (LimitField.HasValue)
            {
                if (Offset.HasValue)
                {
                    sql.Insert(0, $"SELECT * FROM (SELECT a.*, ROWNUM rnum FROM ({sql}) a WHERE ROWNUM <= {Offset + LimitField}) WHERE rnum > {Offset}");
                }
                else
                {
                    sql.Insert(0, $"SELECT * FROM ({sql}) WHERE ROWNUM <= {LimitField}");
                }
            }

            return (sql.ToString(), Parameters);
        }
    }
}
