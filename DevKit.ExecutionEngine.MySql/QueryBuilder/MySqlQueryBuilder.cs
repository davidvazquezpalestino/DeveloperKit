namespace DevKit.ExecutionEngine.MySql.QueryBuilder
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MySqlQueryBuilder<T>(IMySqlProvider provider)
        where T : class, new()
    {
        private readonly List<string> SelectColumns = new();
        private string FromClause;
        private readonly List<string> JoinClauses = new();
        private readonly List<string> WhereClauses = new();
        private readonly List<string> OrderByClauses = new();
        private readonly Dictionary<string, object> Parameters = new();
        private int? LimitValue;
        private int? OffsetValue;
        private bool IsDistinct;


        public MySqlQueryBuilder<T> Select(params string[] columns)
        {
            SelectColumns.AddRange(columns);
            return this;
        }

        public MySqlQueryBuilder<T> From(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be empty", nameof(tableName));

            FromClause = tableName;
            return this;
        }

        public MySqlQueryBuilder<T> Where(string condition, IDictionary<string, object> parameters = null)
        {
            if (!string.IsNullOrWhiteSpace(condition))
            {
                WhereClauses.Add(condition);
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        Parameters[param.Key] = param.Value;
                    }
                }
            }
            return this;
        }

        public MySqlQueryBuilder<T> OrderBy(string column, bool descending = false)
        {
            if (!string.IsNullOrWhiteSpace(column))
            {
                OrderByClauses.Add($"{column} {(descending ? "DESC" : "ASC")}");
            }
            return this;
        }

        public MySqlQueryBuilder<T> Limit(int limit)
        {
            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than zero");

            LimitValue = limit;
            return this;
        }

        public MySqlQueryBuilder<T> Offset(int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative");

            OffsetValue = offset;
            return this;
        }

        public MySqlQueryBuilder<T> Distinct()
        {
            IsDistinct = true;
            return this;
        }

        public void AddParameter(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Parameter name cannot be empty", nameof(name));

            Parameters[name] = value;
        }

        public (string Sql, Dictionary<string, object> Parameters) Build()
        {
            if (string.IsNullOrEmpty(FromClause))
                throw new InvalidOperationException("Table name must be specified using From() method");

            StringBuilder sql = new StringBuilder();

            // SELECT clause
            sql.Append("SELECT ");
            if (IsDistinct)
                sql.Append("DISTINCT ");

            sql.Append(SelectColumns.Any() ? string.Join(", ", SelectColumns) : "*");

            // FROM clause
            sql.Append(" FROM ").Append(FromClause);

            // JOIN clauses
            if (JoinClauses.Count > 0)
            {
                sql.Append(" ").Append(string.Join(" ", JoinClauses));
            }

            // WHERE clause
            if (WhereClauses.Count > 0)
            {
                sql.Append(" WHERE ").Append(string.Join(" AND ", WhereClauses));
            }

            // ORDER BY clause
            if (OrderByClauses.Count > 0)
            {
                sql.Append(" ORDER BY ").Append(string.Join(", ", OrderByClauses));
            }

            // LIMIT and OFFSET clauses (MySQL syntax)
            if (LimitValue.HasValue)
            {
                sql.Append(" LIMIT ").Append(LimitValue.Value);

                if (OffsetValue.HasValue)
                {
                    sql.Append(" OFFSET ").Append(OffsetValue.Value);
                }
            }

            return (sql.ToString(), new Dictionary<string, object>(Parameters));
        }
    }
}
