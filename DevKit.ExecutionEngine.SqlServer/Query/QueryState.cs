namespace DevKit.ExecutionEngine.SQLServer.Query;

/// <summary>
/// Represents the state of a query being built.
/// </summary>
/// <typeparam name="T">The entity type being queried</typeparam>
public class QueryState<T> where T : class, new()
{
    internal ISQLServerProvider Db { get; set; }
    /// <summary>
    /// Gets or sets the schema name for the query.
    /// </summary>
    internal string Schema { get; private set; }

    /// <summary>
    /// Gets or sets the table name for the query.
    /// </summary>
    internal string TableName { get; }
    internal List<Expression<Func<T, bool>>> WhereExpressions { get; } = new();
    internal List<(string column, bool isAscending)> OrderByField { get; } = new();
    internal int? TakeField { get; set; }
    internal int SkipField { get; set; }
    internal Expression<Func<T, object>> SelectExpression { get; set; }

    internal QueryState(ISQLServerProvider db, string schema = null, string tableName = null)
    {
        Db = db;
        Schema = schema;
        TableName = tableName;
    }

    /// <summary>
    /// Sets the schema for the query.
    /// </summary>
    /// <param name="schema">The schema name to use</param>
    /// <returns>The query state with the schema set</returns>
    public QueryState<T> WithSchema(string schema)
    {
        QueryState<T> newState = new QueryState<T>(Db, schema, TableName)
        {
            TakeField = TakeField,
            SkipField = SkipField,
            SelectExpression = SelectExpression
        };

        newState.WhereExpressions.AddRange(WhereExpressions);
        newState.OrderByField.AddRange(OrderByField);

        return newState;
    }

    /// <summary>
    /// Sets the table name for the query.
    /// </summary>
    /// <param name="tableName">The table name to use</param>
    /// <returns>The query state with the table name set</returns>
    public QueryState<T> WithTableName(string tableName)
    {
        QueryState<T> newState = new QueryState<T>(Db, Schema, tableName)
        {
            TakeField = TakeField,
            SkipField = SkipField,
            SelectExpression = SelectExpression
        };

        newState.WhereExpressions.AddRange(WhereExpressions);
        newState.OrderByField.AddRange(OrderByField);

        return newState;
    }
}