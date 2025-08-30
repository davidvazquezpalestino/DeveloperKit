using System.Linq.Expressions;
using System.Text;

namespace DevKit.ExecutionEngine.PostgreSQL.QueryBuilder
{
    /// <summary>
    /// Fluent query builder for generating PostgreSQL queries in a type-safe manner using expression trees.
    /// </summary>
    /// <typeparam name="T">The entity type this query operates on</typeparam>
    public class PostgreSqlQueryBuilder<T> where T : class
    {
        private readonly List<string> SelectFields = new();
        private string FromClause;
        private readonly List<string> JoinClauses = new();
        private readonly List<string> WhereClauses = new();
        private readonly List<string> OrderByClauses = new();
        private readonly List<string> GroupByClauses = new();
        private readonly List<string> HavingClauses = new();
        private readonly Dictionary<string, object> Parameters = new();
        private int? LimitCount;
        private int? OffsetCount;
        private bool DistinctField;
        private string Alias = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlQueryBuilder{T}"/> class.
        /// </summary>
        public PostgreSqlQueryBuilder()
        {
            FromClause = $"\"{typeof(T).Name}\"";
        }

        /// <summary>
        /// Specifies the columns to select.
        /// </summary>
        public PostgreSqlQueryBuilder<T> Select(params Expression<Func<T, object>>[] fields)
        {
            if (fields == null || fields.Length == 0)
            {
                SelectFields.Add("*");
                return this;
            }

            PostgreSqlExpressionVisitor visitor = new PostgreSqlExpressionVisitor();
            foreach (Expression<Func<T, object>> field in fields)
            {
                visitor.Visit(field);
                SelectFields.Add(visitor.GetResult());
            }

            return this;
        }

        /// <summary>
        /// Specifies that the query should return distinct results.
        /// </summary>
        public PostgreSqlQueryBuilder<T> Distinct()
        {
            DistinctField = true;
            return this;
        }

        /// <summary>
        /// Specifies the table to query from.
        /// </summary>
        public PostgreSqlQueryBuilder<T> From(string tableName, string alias = "")
        {
            FromClause = string.IsNullOrEmpty(alias)
                ? $"\"{tableName}\""
                : $"\"{tableName}\" AS \"{alias}\"";
            Alias = alias;
            return this;
        }

        /// <summary>
        /// Adds a WHERE condition to the query.
        /// </summary>
        public PostgreSqlQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                return this;
            }

            PostgreSqlExpressionVisitor visitor = new PostgreSqlExpressionVisitor(Alias);
            visitor.Visit(predicate);
            WhereClauses.Add(visitor.GetResult());

            // Add parameters
            foreach (KeyValuePair<string, object> param in visitor.Parameters)
            {
                Parameters[param.Key] = param.Value;
            }

            return this;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the query.
        /// </summary>
        public PostgreSqlQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool descending = false)
        {
            if (keySelector == null)
            {
                return this;
            }

            PostgreSqlExpressionVisitor visitor = new PostgreSqlExpressionVisitor(Alias);
            visitor.Visit(keySelector);
            string orderByClause = $"{visitor.GetResult()} {(descending ? "DESC" : "ASC")}";
            OrderByClauses.Add(orderByClause);

            return this;
        }

        /// <summary>
        /// Limits the number of rows returned by the query.
        /// </summary>
        public PostgreSqlQueryBuilder<T> Limit(int count)
        {
            LimitCount = count;
            return this;
        }

        /// <summary>
        /// Skips a specified number of rows.
        /// </summary>
        public PostgreSqlQueryBuilder<T> Offset(int count)
        {
            OffsetCount = count;
            return this;
        }

        /// <summary>
        /// Builds the final SQL query.
        /// </summary>
        public (string Sql, Dictionary<string, object> Parameters) Build()
        {
            StringBuilder sql = new StringBuilder();

            // SELECT [DISTINCT] columns
            sql.Append("SELECT ");
            if (DistinctField)
            {
                sql.Append("DISTINCT ");
            }

            sql.AppendLine(SelectFields.Count > 0 ? string.Join(", ", SelectFields) : "*");

            // FROM table
            sql.AppendLine($"FROM {FromClause}");

            // JOIN clauses
            foreach (string join in JoinClauses)
            {
                sql.AppendLine(join);
            }

            // WHERE conditions
            if (WhereClauses.Count > 0)
            {
                sql.Append("WHERE ");
                sql.AppendLine(string.Join(" AND ", WhereClauses));
            }

            // GROUP BY
            if (GroupByClauses.Count > 0)
            {
                sql.Append("GROUP BY ");
                sql.AppendLine(string.Join(", ", GroupByClauses));

                // HAVING
                if (HavingClauses.Count > 0)
                {
                    sql.Append("HAVING ");
                    sql.AppendLine(string.Join(" AND ", HavingClauses));
                }
            }

            // ORDER BY
            if (OrderByClauses.Count > 0)
            {
                sql.Append("ORDER BY ");
                sql.AppendLine(string.Join(", ", OrderByClauses));
            }

            // LIMIT and OFFSET
            if (LimitCount.HasValue)
            {
                sql.AppendLine($"LIMIT {LimitCount}");
            }

            if (OffsetCount.HasValue)
            {
                sql.AppendLine($"OFFSET {OffsetCount}");
            }

            return (sql.ToString().Trim(), new Dictionary<string, object>(Parameters));
        }

    }
}
