namespace DevKit.ExecutionEngine.SQLServer.Query;

/// <summary>
/// Representa el estado de una consulta en construcción.
/// </summary>
/// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
public class QueryState<T> : ILoggedQuery where T : class, new()
{
    internal ISQLServerProvider DbProvider { get; set; }

    /// <summary>
    /// SQL que se ejecutó (para logging)
    /// </summary>
    public string ExecutedSql { get; private set; }

    /// <summary>
    /// Parámetros de la consulta (para logging)
    /// </summary>
    public IDictionary<string, object> Parameters { get; private set; } = new Dictionary<string, object>();

    /// <summary>
    /// Obtiene o establece el nombre del esquema para la consulta.
    /// </summary>
    internal string Schema { get; private set; }

    /// <summary>
    /// Obtiene o establece el nombre de la tabla para la consulta.
    /// </summary>
    internal string TableName { get; }

    internal List<Expression<Func<T, bool>>> WhereExpressions { get; } = new();
    internal List<(string column, bool isAscending)> OrderByField { get; } = new();
    internal int? TakeField { get; set; }
    internal int? SkipField { get; set; }
    internal bool DistinctField { get; set; }
    internal List<string> SelectFields { get; set; } = new();

    internal Expression<Func<T, object>> SelectExpression { get; set; }

    internal QueryState(ISQLServerProvider dbProvider, string schema = null, string tableName = null)
    {
        DbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        Schema = schema;
        TableName = tableName ?? typeof(T).Name;
    }

    /// <summary>
    /// Construye la consulta SQL
    /// </summary>
    public string Build()
    {
        QueryResult queryInfo = BuildQueryInternal();

        // Registrar la consulta SQL generada
        QueryLogger.LogQuery(
            queryInfo.SQL,
            queryInfo.Parameters,
            IQueryLogger.LogLevel.Debug,
            "Consulta SQL generada");

        return queryInfo.SQL;
    }

    /// <summary>
    /// Obtiene los parámetros de la consulta
    /// </summary>
    public IReadOnlyDictionary<string, object> GetParameters()
    {
        QueryResult queryInfo = BuildQueryInternal();
        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(
            queryInfo.Parameters ?? new Dictionary<string, object>());
    }

    /// <summary>
    /// Construye la consulta SQL y sus parámetros
    /// </summary>
    private QueryResult BuildQueryInternal()
    {
        // Construir la consulta SQL manualmente
        StringBuilder sqlBuilder = new System.Text.StringBuilder();
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        int paramIndex = 0;

        // SELECT
        sqlBuilder.Append("SELECT ");

        // DISTINCT
        if (DistinctField)
        {
            sqlBuilder.Append("DISTINCT ");
        }

        // CAMPOS
        sqlBuilder.Append(SelectFields is { Count: > 0 } ? string.Join(", ", SelectFields) : "*");

        // FROM
        sqlBuilder.Append(" FROM ");
        if (!string.IsNullOrEmpty(Schema))
        {
            sqlBuilder.Append($"[{Schema}].");
        }
        sqlBuilder.Append($"[{TableName}]");

        // WHERE
        if (WhereExpressions.Count > 0)
        {
            sqlBuilder.Append(" WHERE ");
            bool first = true;
            foreach (Expression<Func<T, bool>> whereExpr in WhereExpressions)
            {
                if (!first)
                {
                    sqlBuilder.Append(" AND ");
                }

                // Convertir la expresión a SQL (esto es un ejemplo simplificado)
                string paramName = $"@p{paramIndex++}";
                if (whereExpr.Body is MemberExpression memberExpr)
                {
                    object value = Expression.Lambda(memberExpr).Compile().DynamicInvoke();
                    sqlBuilder.Append($"{memberExpr.Member.Name} = {paramName}");
                    parameters[paramName] = value;
                }
                first = false;
            }
        }

        // ORDER BY
        if (OrderByField.Count > 0)
        {
            sqlBuilder.Append(" ORDER BY ");
            bool first = true;
            foreach ((string column, bool isAscending) in OrderByField)
            {
                if (!first)
                {
                    sqlBuilder.Append(", ");
                }
                sqlBuilder.Append($"{column} {(isAscending ? "ASC" : "DESC")}");
                first = false;
            }
        }

        // OFFSET-FETCH (paginación)
        if (SkipField.HasValue || TakeField.HasValue)
        {
            sqlBuilder.Append(" OFFSET ");
            sqlBuilder.Append(SkipField ?? 0);
            sqlBuilder.Append(" ROWS");

            if (TakeField.HasValue)
            {
                sqlBuilder.Append(" FETCH NEXT ");
                sqlBuilder.Append(TakeField.Value);
                sqlBuilder.Append(" ROWS ONLY");
            }
        }

        return new QueryResult(sqlBuilder.ToString(), parameters);
    }

    /// <summary>
    /// Construye y ejecuta la consulta, devolviendo los resultados como una lista
    /// </summary>
    public List<T> ToList()
    {
        QueryResult queryInfo = BuildQueryInternal();

        // Registrar la consulta que se va a ejecutar
        QueryLogger.LogQuery(
            queryInfo.SQL,
            queryInfo.Parameters,
            IQueryLogger.LogLevel.Debug,
            $"Ejecutando consulta para obtener lista de {typeof(T).Name}");

        ICollection<T> result = DbProvider.ExecuteQueryAsList<T>(
            queryInfo.SQL,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryInfo.Parameters));

        // Registrar el resultado
        QueryLogger.LogQuery(
            queryInfo.SQL,
            queryInfo.Parameters,
            IQueryLogger.LogLevel.Debug,
            $"Consulta completada. Se encontraron {result?.Count ?? 0} registros de {typeof(T).Name}");

        // Almacenar información para logging
        ExecutedSql = queryInfo.SQL;
        Parameters = queryInfo.Parameters ?? new Dictionary<string, object>();

        return result.ToList();
    }

    /// <summary>
    /// Establece el esquema para la consulta.
    /// </summary>
    /// <param name="schema">Nombre del esquema a utilizar</param>
    /// <returns>El estado de la consulta con el esquema establecido</returns>
    public QueryState<T> WithSchema(string schema)
    {
        QueryState<T> queryState = new QueryState<T>(DbProvider, schema, TableName)
        {
            TakeField = TakeField,
            SkipField = SkipField,
            SelectExpression = SelectExpression
        };

        queryState.WhereExpressions.AddRange(WhereExpressions);
        queryState.OrderByField.AddRange(OrderByField);

        return queryState;
    }

    /// <summary>
    /// Sets the table name for the query.
    /// </summary>
    /// <param name="tableName">The table name to use</param>
    /// <returns>The query state with the table name set</returns>
    public QueryState<T> WithTableName(string tableName)
    {
        QueryState<T> queryState = new QueryState<T>(DbProvider, Schema, tableName)
        {
            TakeField = TakeField,
            SkipField = SkipField,
            SelectExpression = SelectExpression
        };

        queryState.WhereExpressions.AddRange(WhereExpressions);
        queryState.OrderByField.AddRange(OrderByField);

        return queryState;
    }
}