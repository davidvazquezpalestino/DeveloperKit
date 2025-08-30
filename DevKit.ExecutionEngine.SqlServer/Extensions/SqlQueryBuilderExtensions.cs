namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Extension methods for building SQL queries in a fluent manner.
/// </summary>
public static class SqlQueryBuilderExtensions
{
    /// <summary>
    /// Starts a new query for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type to query</typeparam>
    /// <param name="dbProvider">The database provider instance</param>
    /// <returns>A new query builder instance</returns>
    public static QueryState<T> From<T>(this ISQLServerProvider dbProvider) where T : class, new()
    {
        return new QueryState<T>(dbProvider);
    }

    /// <summary>
    /// Starts a new query for the specified table with custom schema.
    /// </summary>
    /// <typeparam name="T">The entity type to query</typeparam>
    /// <param name="dbProvider">The database provider instance</param>
    /// <param name="schema">The database schema name (e.g., "dbo")</param>
    /// <param name="tableName">The table name (optional, uses class name if not provided)</param>
    /// <returns>A new query builder instance</returns>
    public static QueryState<T> From<T>(this ISQLServerProvider dbProvider, string schema, string tableName = null) where T : class, new()
    {
        return new QueryState<T>(dbProvider, schema, tableName);
    }

    /// <summary>
    /// Executes the query and returns the results as a list.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <param name="queryState">The query state</param>
    /// <returns>A list of entities matching the query</returns>
    public static List<T> ToList<T>(this QueryState<T> queryState) where T : class, new()
    {
        QueryResult queryResult = BuildQuery(queryState);
        return (List<T>)queryState.Db.ExecuteQueryAsList(queryResult.Sql,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters));
    }

    /// <summary>
    /// Specifies which properties to include in the query results.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <typeparam name="TResult">The type of the result</typeparam>
    /// <param name="queryState">The query state</param>
    /// <param name="selector">A lambda expression that specifies which properties to include</param>
    /// <returns>The query state with the select expression applied</returns>
    public static QueryState<T> Select<T, TResult>(this QueryState<T> queryState, Expression<Func<T, TResult>> selector) where T : class, new()
    {
        if (selector == null)
            throw new ArgumentNullException(nameof(selector));

        // Create a new state to maintain immutability
        QueryState<T> newState = new QueryState<T>(queryState.Db, queryState.Schema, queryState.TableName)
        {
            TakeField = queryState.TakeField,
            SkipField = queryState.SkipField
        };

        try
        {
            // Store the original selector expression
            // Check if we can use the selector as is (when TResult is object)
            if (typeof(TResult) == typeof(object))
            {
                // If it's already Func<T, object>, we can use it directly
                newState.SelectExpression = (Expression<Func<T, object>>)(object)selector;
            }
            else if (selector.Body is NewExpression || selector.Body is MemberExpression || selector.Body is ParameterExpression)
            {
                // For NewExpression (anonymous types), MemberExpression (single property), or ParameterExpression (identity selector)
                // we can safely convert to Func<T, object>
                newState.SelectExpression = Expression.Lambda<Func<T, object>>(
                    Expression.Convert(selector.Body, typeof(object)),
                    selector.Parameters
                );
            }
            else
            {
                // For other expressions, try to evaluate them
                var compiled = selector.Compile();
                var result = compiled(default);
                newState.SelectExpression = _ => result;
            }
        }
        catch (Exception ex)
        {
            throw new NotSupportedException("The select expression is not supported. Please use a simple property access or anonymous type. " +
                                         $"Expression: {selector.Body}, Type: {selector.Body.NodeType}", ex);
        }

        newState.WhereExpressions.AddRange(queryState.WhereExpressions);
        newState.OrderByField.AddRange(queryState.OrderByField);

        return newState;
    }

    /// <summary>
    /// Executes the query asynchronously and returns the results as a list.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <param name="queryState">The query state</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation</param>
    /// <returns>A task that represents the asynchronous operation, containing the list of entities</returns>
    public static async Task<List<T>> ToListAsync<T>(this QueryState<T> queryState, CancellationToken cancellationToken = default) where T : class, new()
    {
        QueryResult queryResult = BuildQuery(queryState);

        return (List<T>)await queryState.Db.ExecuteQueryAsListAsync(queryResult.Sql,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters),
            cancellationToken);
    }

    /// <summary>
    /// Executes the query and returns the first result, or null if no results are found.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <param name="queryState">The query state</param>
    /// <returns>The first entity that matches the query, or null</returns>
    public static T FirstOrDefault<T>(this QueryState<T> queryState) where T : class, new()
    {
        queryState.TakeField = 1;
        QueryResult queryResult = BuildQuery(queryState);

        ICollection<T> results = queryState.Db.ExecuteQueryAsList(queryResult.Sql,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters));
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Executes the query asynchronously and returns the first result, or null if no results are found.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <param name="queryState">The query state</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation</param>
    /// <returns>A task that represents the asynchronous operation, containing the first entity or null</returns>
    public static async Task<T> FirstOrDefaultAsync<T>(this QueryState<T> queryState, CancellationToken cancellationToken = default) where T : class, new()
    {
        queryState.TakeField = 1;
        (string sql, Dictionary<string, object> parameters) = BuildQuery(queryState);

        ICollection<T> results = await queryState.Db.ExecuteQueryAsListAsync<T>(sql,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(parameters),
            cancellationToken);

        return results.FirstOrDefault();
    }

    /// <summary>
    /// Filters the query results based on a predicate.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <param name="queryState">The query state</param>
    /// <param name="predicate">A function to test each element for a condition</param>
    /// <returns>The query state with the filter applied</returns>
    public static QueryState<T> Where<T>(this QueryState<T> queryState, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        queryState.WhereExpressions.Add(predicate);
        return queryState;
    }

    /// <summary>
    /// Sorts the elements of the query in ascending order according to a key.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the function represented by keySelector</typeparam>
    /// <param name="queryState">The query state</param>
    /// <param name="keySelector">A function to extract a key from an element</param>
    /// <returns>The query state with the ordering applied</returns>
    public static QueryState<T> OrderBy<T, TKey>(this QueryState<T> queryState, Expression<Func<T, TKey>> keySelector) where T : class, new()
    {
        queryState.OrderByField.Add((GetMemberName(keySelector.Body), true));
        return queryState;
    }

    /// <summary>
    /// Sorts the elements of the query in descending order according to a key.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the function represented by keySelector</typeparam>
    /// <param name="queryState">The query state</param>
    /// <param name="keySelector">A function to extract a key from an element</param>
    /// <returns>The query state with the ordering applied</returns>
    public static QueryState<T> OrderByDescending<T, TKey>(this QueryState<T> queryState, Expression<Func<T, TKey>> keySelector) where T : class, new()
    {
        queryState.OrderByField.Add((GetMemberName(keySelector.Body), false));
        return queryState;
    }

    /// <summary>
    /// Performs a subsequent ordering of the elements in a sequence in descending order.
    /// </summary>
    /// <typeparam name="T">The type of the elements</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the function represented by keySelector</typeparam>
    /// <param name="queryState">The query state</param>
    /// <param name="keySelector">A function to extract a key from an element</param>
    /// <returns>The query state with the subsequent ordering applied</returns>
    public static QueryState<T> ThenByDescending<T, TKey>(this QueryState<T> queryState, Expression<Func<T, TKey>> keySelector) where T : class, new()
    {
        if (queryState.OrderByField.Count == 0)
        {
            throw new InvalidOperationException("ThenByDescending must be called after OrderBy or OrderByDescending");
        }

        queryState.OrderByField.Add((GetMemberName(keySelector.Body), false));
        return queryState;
    }

    /// <summary>
    /// Bypasses a specified number of elements in the query results and then returns the remaining elements.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <param name="queryState">The query state</param>
    /// <param name="count">The number of elements to skip</param>
    /// <returns>The query state with the skip applied</returns>
    public static QueryState<T> Skip<T>(this QueryState<T> queryState, int count) where T : class, new()
    {
        queryState.SkipField = count;
        return queryState;
    }

    /// <summary>
    /// Returns a specified number of contiguous elements from the start of the query results.
    /// </summary>
    /// <typeparam name="T">The entity type being queried</typeparam>
    /// <param name="queryState">The query state</param>
    /// <param name="count">The number of elements to return</param>
    /// <returns>The query state with the take applied</returns>
    public static QueryState<T> Take<T>(this QueryState<T> queryState, int count) where T : class, new()
    {
        queryState.TakeField = count;
        return queryState;
    }
    private static string GetSchema<T>(QueryState<T> queryState) where T : class, new()
    {
        // Use explicitly provided schema, then check TableAttribute, then default to "dbo"
        return queryState.Schema ?? typeof(T).GetCustomAttribute<TableAttribute>()?.Schema ?? "dbo";
    }

    private static string GetTableName<T>(QueryState<T> queryState) where T : class, new()
    {
        // Use explicitly provided table name, then check TableAttribute, then use type name
        string tableName = queryState.TableName ?? typeof(T).Name;

        // Check for TableAttribute to override table name if not explicitly provided
        if (queryState.TableName == null)
        {
            TableAttribute tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
            if (tableAttr?.Name != null)
            {
                tableName = tableAttr.Name;
            }
        }

        // Get schema from query state, then from TableAttribute, then default to 'dbo'
        string schema = queryState.Schema ??
                       typeof(T).GetCustomAttribute<TableAttribute>()?.Schema ??
                       "dbo";

        // Return fully qualified table name with schema
        return $"[{schema}].[{tableName}]";
    }

    private static string ProcessExpression(WhereExpressionVisitor visitor, Expression expression)
    {
        expression = visitor.Visit(expression);

        switch (expression)
        {
            case ConstantExpression constExpr when constExpr.Type == typeof(string) && constExpr.Value is string strValue:
                // Si es un parámetro ya procesado (comienza con @p)
                if (strValue.StartsWith("@p") && visitor.Parameters.ContainsKey(strValue))
                {
                    return strValue;
                }
                // Si es un valor de cadena normal
                string paramName = visitor.GetNextParameterName();
                visitor.Parameters[paramName] = strValue;
                return paramName;
            case BinaryExpression binaryExpr:
                string operatorStr = binaryExpr.NodeType switch
                {
                    ExpressionType.Equal => "=",
                    ExpressionType.NotEqual => "!=",
                    ExpressionType.GreaterThan => ">",
                    ExpressionType.GreaterThanOrEqual => ">=",
                    ExpressionType.LessThan => "<",
                    ExpressionType.LessThanOrEqual => "<=",
                    ExpressionType.AndAlso => "AND",
                    ExpressionType.OrElse => "OR",
                    _ => throw new NotSupportedException($"Binary operator {binaryExpr.NodeType} is not supported")
                };

                string left = ProcessExpression(visitor, binaryExpr.Left);
                string right = ProcessExpression(visitor, binaryExpr.Right);

                // For logical operators, wrap in parentheses for proper precedence
                if (binaryExpr.NodeType == ExpressionType.AndAlso ||
                    binaryExpr.NodeType == ExpressionType.OrElse)
                {
                    return $"({left} {operatorStr} {right})";
                }
                return $"{left} {operatorStr} {right}";

            case ConstantExpression constExpr:
                if (constExpr.Type == typeof(string))
                {
                    return $"'{constExpr.Value}'";
                }
                return constExpr.Value?.ToString() ?? "NULL";

            case ParameterExpression paramExpr:
                return paramExpr.Name;

            case MemberExpression memberExpr:
                if (memberExpr.Expression is ParameterExpression)
                {
                    return memberExpr.Member.Name;
                }

                if (memberExpr.Expression is ConstantExpression)
                {
                    // Handle constant member access (e.g., DateTime.Now)
                    object value = Expression.Lambda(expression).Compile().DynamicInvoke();
                    if (value is string str)
                    {
                        string constParam = visitor.GetNextParameterName();
                        visitor.Parameters[constParam] = str;
                        return constParam;
                    }
                    return value?.ToString() ?? "NULL";
                }

                // For other cases, try to evaluate the expression
                try
                {
                    object value = Expression.Lambda(expression).Compile().DynamicInvoke();
                    if (value is string str)
                    {
                        string constParam = visitor.GetNextParameterName();
                        visitor.Parameters[constParam] = str;
                        return constParam;
                    }
                    return value?.ToString() ?? "NULL";
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException($"Could not evaluate expression: {expression}", ex);
                }

            case UnaryExpression unaryExpr:
                if (unaryExpr.NodeType == ExpressionType.Not)
                {
                    string operand = ProcessExpression(visitor, unaryExpr.Operand);
                    // Only wrap in parentheses if the operand is a complex expression
                    if (unaryExpr.Operand is BinaryExpression || unaryExpr.Operand is MethodCallExpression)
                    {
                        return $"NOT ({operand})";
                    }
                    return $"NOT {operand}";
                }
                return ProcessExpression(visitor, unaryExpr.Operand);

            case MethodCallExpression methodCall:
                // Handle common method calls like string.StartsWith, Contains, etc.
                if (methodCall.Method.DeclaringType == typeof(string))
                {
                    string instance = methodCall.Object != null
                        ? ProcessExpression(visitor, methodCall.Object)
                        : null;

                    string[] arguments = methodCall.Arguments
                        .Select(arg => ProcessExpression(visitor, arg))
                        .ToArray();

                    // Obtener el valor del argumento
                    object value;
                    string searchParam = visitor.GetNextParameterName();

                    // Si el argumento es una constante
                    if (methodCall.Arguments[0] is ConstantExpression constantExpr)
                    {
                        value = constantExpr.Value?.ToString() ?? string.Empty;
                    }
                    // Si el argumento es un parámetro existente
                    else if (arguments[0].StartsWith("@p") && visitor.Parameters.TryGetValue(arguments[0], out object paramValue))
                    {
                        value = paramValue?.ToString() ?? string.Empty;
                    }
                    // Si es un valor literal entre comillas
                    else if (arguments[0].StartsWith("'"))
                    {
                        value = arguments[0].Trim('\'');
                    }
                    // Cualquier otro caso
                    else
                    {
                        value = arguments[0];
                    }

                    // Aplicar el patrón correspondiente según el método
                    visitor.Parameters[searchParam] = methodCall.Method.Name switch
                    {
                        nameof(string.StartsWith) => $"{value}%",
                        nameof(string.EndsWith) => $"%{value}",
                        nameof(string.Contains) => $"%{value}%",
                        _ => value
                    };

                    return $"{instance} LIKE {searchParam}";
                }
                throw new NotSupportedException($"Method calls on type {methodCall.Method.DeclaringType?.Name} are not supported");

            default:
                throw new NotSupportedException($"Expression of type {expression.NodeType} is not supported");
        }
    }

    private static QueryResult BuildQuery<T>(QueryState<T> queryState) where T : class, new()
    {
        QueryResult result = new QueryResult();
        string tableName = GetTableName(queryState);
        string whereClause = "";
        string orderByClause = "";
        int paramIndex = 0;

        // Build WHERE clause
        if (queryState.WhereExpressions.Count > 0)
        {
            List<string> whereConditions = new List<string>();
            // Crear el visitador con el diccionario de parámetros compartido
            WhereExpressionVisitor visitor = new WhereExpressionVisitor(result.Parameters, ref paramIndex);

            foreach (Expression<Func<T, bool>> expr in queryState.WhereExpressions)
            {
                Expression condition = visitor.Visit(expr.Body);

                if (condition is ConstantExpression constant)
                {
                    if (constant.Type == typeof(string))
                    {
                        whereConditions.Add((string)constant.Value);
                    }
                    else if (constant.Type == typeof(bool))
                    {
                        whereConditions.Add((bool)constant.Value ? "1=1" : "1=0");
                    }
                    else
                    {
                        string paramName = $"@p{paramIndex++}";
                        result.Parameters[paramName] = constant.Value;
                        whereConditions.Add(paramName);
                    }
                }
                else if (condition is BinaryExpression expression)
                {
                    // Handle binary expressions like x == y, x > y, etc.
                    string operatorStr = expression.NodeType switch
                    {
                        ExpressionType.Equal => "=",
                        ExpressionType.NotEqual => "!=",
                        ExpressionType.GreaterThan => ">",
                        ExpressionType.GreaterThanOrEqual => ">=",
                        ExpressionType.LessThan => "<",
                        ExpressionType.LessThanOrEqual => "<=",
                        ExpressionType.And => "AND",
                        ExpressionType.AndAlso => "AND",
                        ExpressionType.Or => "OR",
                        ExpressionType.OrElse => "OR",
                        _ => throw new NotSupportedException($"Binary operator {expression.NodeType} is not supported")
                    };

                    // Process left and right sides of the binary expression
                    string leftStr = ProcessExpression(visitor, expression.Left);
                    string rightStr = ProcessExpression(visitor, expression.Right);

                    whereConditions.Add($"{leftStr} {operatorStr} {rightStr}");
                }
                else
                {
                    // For other cases, try to evaluate the expression
                    try
                    {
                        string value = ProcessExpression(visitor, condition);
                        if (!string.IsNullOrEmpty(value))
                        {
                            whereConditions.Add(value);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Could not evaluate expression: " + expr, ex);
                    }
                }
            }

            whereClause = whereConditions.Count > 0
                ? " WHERE " + string.Join(" AND ", whereConditions)
                : string.Empty;
        }

        // Build ORDER BY clause
        if (queryState.OrderByField.Count > 0)
        {
            orderByClause = "ORDER BY " + string.Join(", ",
                queryState.OrderByField.Select(x => $"{x.column} {(x.isAscending ? "ASC" : "DESC")}"));
        }

        // Build the SELECT clause based on the SelectExpression
        string selectClause = "*";
        if (queryState.SelectExpression != null)
        {
            try
            {
                Expression body = queryState.SelectExpression.Body;

                // Unwrap the Convert expression if present
                while (body.NodeType == ExpressionType.Convert || body.NodeType == ExpressionType.ConvertChecked)
                {
                    body = ((UnaryExpression)body).Operand;
                }

                // Handle different types of expressions in the select
                if (body is NewExpression newExpression)
                {
                    // Handle anonymous type creation: new { p.Property1, p.Property2 }
                    if (newExpression.Members != null)
                    {
                        // If we have members (anonymous type), use them
                        selectClause = string.Join(", ", newExpression.Members.Select(m => $"[{m.Name}]"));
                    }
                    else if (newExpression.Arguments.Count > 0)
                    {
                        // For other cases, try to extract member access from arguments
                        var properties = new List<string>();
                        foreach (var arg in newExpression.Arguments)
                        {
                            Expression expr = arg;

                            // Unwrap any Convert expressions
                            while (expr is UnaryExpression unary && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
                            {
                                expr = unary.Operand;
                            }

                            if (expr is MemberExpression memberExpr)
                            {
                                properties.Add($"[{memberExpr.Member.Name}]");
                            }
                            else if (expr is ConstantExpression constantExpr)
                            {
                                // Handle constant values in the select
                                properties.Add(constantExpr.Value?.ToString() ?? "NULL");
                            }
                            else
                            {
                                throw new NotSupportedException($"Unsupported expression type in select: {expr.NodeType}. Expression: {expr}");
                            }
                        }
                        selectClause = string.Join(", ", properties);
                    }
                    else
                    {
                        // Empty new expression, use default
                        selectClause = "*";
                    }
                }
                else if (body is MemberExpression memberExpression)
                {
                    // Handle single property: p => p.Property
                    selectClause = $"[{memberExpression.Member.Name}]";
                }
                else if (body is ParameterExpression)
                {
                    // Handle identity selector: p => p
                    selectClause = "*";
                }
                else if (body is MethodCallExpression methodCall &&
                        methodCall.Method.Name == "Select" &&
                        methodCall.Arguments.Count == 2)
                {
                    // Handle simple .Select() calls
                    selectClause = "*";
                }
                else if (body is UnaryExpression unaryExpression)
                {
                    // Handle unary expressions like !IsDeleted
                    if (unaryExpression.NodeType == ExpressionType.Not &&
                        unaryExpression.Operand is MemberExpression unaryMember)
                    {
                        selectClause = $"[{unaryMember.Member.Name}]";
                    }
                }
                else
                {
                    // Try to evaluate the expression
                    try
                    {
                        var compiled = Expression.Lambda(body).Compile();
                        var result2 = compiled.DynamicInvoke();
                        selectClause = result2?.ToString() ?? "NULL";
                    }
                    catch (Exception ex)
                    {
                        throw new NotSupportedException(
                            $"The select expression is not supported. Expression: {body}, Type: {body.NodeType}. " +
                            "Please use a simple property access, anonymous type, or a supported method call.", ex);
                    }
                }
            }
            catch (Exception ex) when (!(ex is NotSupportedException))
            {
                throw new NotSupportedException(
                    $"Error processing select expression: {queryState.SelectExpression}. " +
                    "Please use a simple property access or anonymous type.", ex);
            }
        }
        else
        {
            // Default to select all columns
            selectClause = "*";
        }

        // Build the final query
        result.Sql = $"SELECT {selectClause} FROM {tableName}";

        if (!string.IsNullOrEmpty(whereClause))
        {
            result.Sql += whereClause;
        }

        if (!string.IsNullOrEmpty(orderByClause))
        {
            result.Sql += $" {orderByClause}";
        }

        // Add pagination if needed
        if (queryState.TakeField.HasValue && queryState.TakeField > 0)
        {
            if (queryState.SkipField > 0)
                result.Sql += $" OFFSET {queryState.SkipField} ROWS FETCH NEXT {queryState.TakeField} ROWS ONLY";
            else
                result.Sql += $" TOP {queryState.TakeField}";
        }

        return result;
    }
    private static string GetMemberName(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
        {
            return GetMemberName(unaryExpression.Operand);
        }

        throw new ArgumentException("Unsupported expression type", nameof(expression));
    }
}