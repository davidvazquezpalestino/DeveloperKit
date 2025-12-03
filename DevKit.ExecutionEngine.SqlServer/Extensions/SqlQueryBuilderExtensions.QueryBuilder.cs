namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Métodos de construcción de consultas para SqlQueryBuilderExtensions
/// </summary>
public static partial class SqlQueryBuilderExtensions
{
    internal static QueryResult BuildQuery<T>(QueryState<T> query) where T : class, new()
    {
        QueryResult result = new();
        string tableName = GetTableName(query);
        string where = "";
        string orderBy = "";
        int paramIndex = 0;

        // Build WHERE clause
        if (query.Where.Count > 0)
        {
            List<string> whereConditions = new();
            // Crear el visitador con el diccionario de parámetros compartido
            WhereExpressionVisitor visitor = new(result.Parameters, ref paramIndex);

            foreach (Expression<Func<T, bool>> expression in query.Where)
            {
                Expression condition = visitor.Visit(expression.Body);

                if (condition is ConstantExpression constantExpression)
                {
                    if (constantExpression.Type == typeof(string))
                    {
                        whereConditions.Add((string)constantExpression.Value);
                    }
                    else if (constantExpression.Type == typeof(bool))
                    {
                        whereConditions.Add((bool)constantExpression.Value ? "1 = 1" : "1 = 0");
                    }
                    else
                    {
                        string paramName = $"@p{paramIndex++}";
                        result.Parameters[paramName] = constantExpression.Value;
                        whereConditions.Add(paramName);
                    }
                }
                else if (condition is BinaryExpression binaryExpression)
                {
                    string operatorStr = binaryExpression.NodeType switch
                    {
                        ExpressionType.Equal => " = ",
                        ExpressionType.NotEqual => " != ",
                        ExpressionType.GreaterThan => " > ",
                        ExpressionType.GreaterThanOrEqual => " >= ",
                        ExpressionType.LessThan => " < ",
                        ExpressionType.LessThanOrEqual => " <= ",
                        ExpressionType.And => " AND ",
                        ExpressionType.AndAlso => " AND ",
                        ExpressionType.Or => " OR ",
                        ExpressionType.OrElse => " OR ",
                        _ => throw new NotSupportedException($"Binary operator {binaryExpression.NodeType} is not supported")
                    };

                    // Process left and right sides of the binary expression
                    string left = ProcessExpression(visitor, binaryExpression.Left);
                    string right = ProcessExpression(visitor, binaryExpression.Right);

                    whereConditions.Add($"{left} {operatorStr} {right}");
                }
                else
                {
                    // For other cases, try to evaluate the expression
                    try
                    {
                        string value = ProcessExpression(visitor, condition);
                        if (string.IsNullOrEmpty(value) == false)
                        {
                            whereConditions.Add(value);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Could not evaluate expression: " + expression, ex);
                    }
                }
            }

            where = whereConditions.Count > 0
                ? " WHERE " + string.Join(" AND ", whereConditions) : string.Empty;
        }

        // Build ORDER BY clause
        if (query.OrderBy.Count > 0)
        {
            orderBy = "ORDER BY " + string.Join(", ", query.OrderBy.Select(x => $"{x.column} {(x.isAscending ? "ASC" : "DESC")}"));
        }

        // Build the SELECT clause based on the SelectExpression
        string selectClause = "*";
        if (query.SelectExpression != null)
        {
            try
            {
                Expression body = query.SelectExpression.Body;

                // Unwrap the Convert expression if present
                while (body.NodeType == ExpressionType.Convert || body.NodeType == ExpressionType.ConvertChecked)
                {
                    body = ((UnaryExpression)body).Operand;
                }

                // Handle different types of expressions in the select
                if (body is MemberInitExpression memberInitExpression)
                {
                    // Handle member initialization: new MyClass { Prop1 = u.Prop1, Prop2 = u.Prop2 }
                    // Like EFC, we use the source property names (u.Prop1) for the SELECT clause
                    List<string> properties = new();
                    foreach (MemberBinding binding in memberInitExpression.Bindings)
                    {
                        if (binding is MemberAssignment assignment)
                        {
                            Expression expression = assignment.Expression;

                            // Unwrap any Convert expressions
                            while (expression is UnaryExpression unaryExpression && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked))
                            {
                                expression = unaryExpression.Operand;
                            }

                            if (expression is MemberExpression memberExpression && memberExpression.Expression is ParameterExpression)
                            {
                                // Use the source property name (u.PropertyName -> PropertyName)
                                properties.Add($"[{memberExpression.Member.Name}]");
                            }
                            else if (expression is ConstantExpression constantExpression)
                            {
                                // Handle constant values in the select
                                properties.Add($"'{constantExpression.Value?.ToString() ?? "NULL"}'");
                            }
                            else
                            {
                                throw new NotSupportedException($"Unsupported expression type in select: {expression.NodeType}. Expression: {expression}");
                            }
                        }
                    }
                    selectClause = string.Join(", ", properties);
                }
                else if (body is NewExpression newExpression)
                {
                    // Handle anonymous type creation: new { Property1 = u.Property1, Property2 = u.Property2 }
                    if (newExpression.Members != null && newExpression.Arguments.Count > 0)
                    {
                        // For anonymous types, use the source property names from arguments, not the member names
                        List<string> properties = new();
                        foreach (Expression argument in newExpression.Arguments)
                        {
                            Expression expression = argument;

                            // Unwrap any Convert expressions
                            while (expression is UnaryExpression unaryExpression && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked))
                            {
                                expression = unaryExpression.Operand;
                            }

                            if (expression is MemberExpression memberExpression && memberExpression.Expression is ParameterExpression)
                            {
                                // Use the source property name (u.PropertyName -> PropertyName)
                                properties.Add($"[{memberExpression.Member.Name}]");
                            }
                            else if (expression is ConstantExpression constantExpression)
                            {
                                // Handle constant values in the select
                                properties.Add($"'{constantExpression.Value?.ToString() ?? "NULL"}'");
                            }
                            else
                            {
                                throw new NotSupportedException($"Unsupported expression type in select: {expression.NodeType}. Expression: {expression}");
                            }
                        }
                        selectClause = string.Join(", ", properties);
                    }
                    else if (newExpression.Arguments.Count > 0)
                    {
                        // For other cases, try to extract member access from arguments
                        List<string> properties = new();
                        foreach (Expression arguments in newExpression.Arguments)
                        {
                            Expression expression = arguments;

                            // Unwrap any Convert expressions
                            while (expression is UnaryExpression unaryExpression && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked))
                            {
                                expression = unaryExpression.Operand;
                            }

                            if (expression is MemberExpression memberExpression)
                            {
                                properties.Add($"[{memberExpression.Member.Name}]");
                            }
                            else if (expression is ConstantExpression constantExpression)
                            {
                                // Handle constant values in the select
                                properties.Add(constantExpression.Value?.ToString() ?? "NULL");
                            }
                            else
                            {
                                throw new NotSupportedException($"Unsupported expression type in select: {expression.NodeType}. Expression: {expression}");
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
                else if (body is MethodCallExpression methodCallExpression && methodCallExpression.Method.Name == "Select" && methodCallExpression.Arguments.Count == 2)
                {
                    // Handle simple .Select() calls
                    selectClause = "*";
                }
                else if (body is UnaryExpression unaryExpression)
                {
                    // Handle unary expressions like !IsDeleted
                    if (unaryExpression.NodeType == ExpressionType.Not && unaryExpression.Operand is MemberExpression unaryMember)
                    {
                        selectClause = $"[{unaryMember.Member.Name}]";
                    }
                }
                else
                {
                    // Try to evaluate the expression
                    try
                    {
                        Delegate compiled = Expression.Lambda(body).Compile();
                        object result2 = compiled.DynamicInvoke();
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
                    $"Error processing select expression: {query.SelectExpression}. " +
                    "Please use a simple property access or anonymous type.", ex);
            }
        }
        else
        {
            // Default to select all columns
            selectClause = "*";
        }

        // Build the final query
        string topClause = (query.TakeField.HasValue && query.TakeField > 0 && (query.SkipField == null || query.SkipField <= 0)) ? $"TOP {query.TakeField} " : "";
        result.SQL = $"SELECT {topClause}{selectClause} FROM {tableName}";

        if (!string.IsNullOrEmpty(where))
        {
            result.SQL += where;
        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            result.SQL += $" {orderBy}";
        }

        // Add pagination if needed
        if (query.SkipField > 0 && query.TakeField.HasValue && query.TakeField > 0)
        {
            result.SQL += $" OFFSET {query.SkipField} ROWS FETCH NEXT {query.TakeField} ROWS ONLY";
        }

        return result;
    }

    /// <summary>
    /// Overloaded BuildQuery method for projected queries
    /// </summary>
    internal static QueryResult BuildQuery<T, TResult>(ProjectedQueryState<T, TResult> query) where T : class, new()
    {
        QueryResult result = new();
        string tableName = GetTableName(query);
        string where = "";
        string orderBy = "";
        int paramIndex = 0;

        // Build WHERE clause (same as regular QueryState)
        if (query.Where.Count > 0)
        {
            List<string> whereConditions = new();
            WhereExpressionVisitor visitor = new(result.Parameters, ref paramIndex);

            foreach (Expression<Func<T, bool>> expression in query.Where)
            {
                Expression condition = visitor.Visit(expression.Body);

                if (condition is ConstantExpression constantExpression)
                {
                    if (constantExpression.Type == typeof(string))
                    {
                        whereConditions.Add((string)constantExpression.Value);
                    }
                    else if (constantExpression.Type == typeof(bool))
                    {
                        whereConditions.Add((bool)constantExpression.Value ? "1 = 1" : "1 = 0");
                    }
                    else
                    {
                        string paramName = $"@p{paramIndex++}";
                        result.Parameters[paramName] = constantExpression.Value;
                        whereConditions.Add(paramName);
                    }
                }
                else if (condition is BinaryExpression binaryExpression)
                {
                    string operatorStr = binaryExpression.NodeType switch
                    {
                        ExpressionType.Equal => " = ",
                        ExpressionType.NotEqual => " != ",
                        ExpressionType.GreaterThan => " > ",
                        ExpressionType.GreaterThanOrEqual => " >= ",
                        ExpressionType.LessThan => " < ",
                        ExpressionType.LessThanOrEqual => " <= ",
                        ExpressionType.And => " AND ",
                        ExpressionType.AndAlso => " AND ",
                        ExpressionType.Or => " OR ",
                        ExpressionType.OrElse => " OR ",
                        _ => throw new NotSupportedException($"Binary operator {binaryExpression.NodeType} is not supported")
                    };

                    string left = ProcessExpression(visitor, binaryExpression.Left);
                    string right = ProcessExpression(visitor, binaryExpression.Right);

                    whereConditions.Add($"{left} {operatorStr} {right}");
                }
                else
                {
                    try
                    {
                        string value = ProcessExpression(visitor, condition);
                        if (string.IsNullOrEmpty(value) == false)
                        {
                            whereConditions.Add(value);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Could not evaluate expression: " + expression, ex);
                    }
                }
            }

            where = whereConditions.Count > 0
                ? " WHERE " + string.Join(" AND ", whereConditions) : string.Empty;
        }

        // Build ORDER BY clause
        if (query.OrderBy.Count > 0)
        {
            orderBy = "ORDER BY " + string.Join(", ", query.OrderBy.Select(x => $"{x.column} {(x.isAscending ? "ASC" : "DESC")}"));
        }

        // Build the SELECT clause based on the SelectExpression
        string selectClause = "*";
        if (query.SelectExpression != null)
        {
            try
            {
                Expression body = query.SelectExpression.Body;

                // Unwrap the Convert expression if present
                while (body.NodeType == ExpressionType.Convert || body.NodeType == ExpressionType.ConvertChecked)
                {
                    body = ((UnaryExpression)body).Operand;
                }

                if (body is MemberInitExpression memberInitExpression)
                {
                    List<string> properties = new();
                    foreach (MemberBinding binding in memberInitExpression.Bindings)
                    {
                        if (binding is MemberAssignment assignment)
                        {
                            Expression expression = assignment.Expression;
                            while (expression is UnaryExpression unaryExpression && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked))
                            {
                                expression = unaryExpression.Operand;
                            }
                            if (expression is MemberExpression memberExpression && memberExpression.Expression is ParameterExpression)
                            {
                                properties.Add($"[{memberExpression.Member.Name}]");
                            }
                            else if (expression is ConstantExpression constantExpression)
                            {
                                properties.Add($"'{constantExpression.Value?.ToString() ?? "NULL"}'");
                            }
                            else
                            {
                                throw new NotSupportedException($"Expression type {expression.GetType().Name} in member assignment is not supported for SQL translation.");
                            }
                        }
                    }
                    selectClause = string.Join(", ", properties);
                }
                else if (body is NewExpression newExpression)
                {
                    List<string> properties = new();
                    for (int i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        Expression argument = newExpression.Arguments[i];
                        while (argument is UnaryExpression unaryExpression && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked))
                        {
                            argument = unaryExpression.Operand;
                        }
                        if (argument is MemberExpression memberExpression && memberExpression.Expression is ParameterExpression)
                        {
                            properties.Add($"[{memberExpression.Member.Name}]");
                        }
                        else if (argument is ConstantExpression constantExpression)
                        {
                            properties.Add($"'{constantExpression.Value?.ToString() ?? "NULL"}'");
                        }
                        else
                        {
                            throw new NotSupportedException($"Expression type {argument.GetType().Name} in new expression is not supported for SQL translation.");
                        }
                    }
                    selectClause = string.Join(", ", properties);
                }
                else if (body is MemberExpression memberExpression && memberExpression.Expression is ParameterExpression)
                {
                    selectClause = $"[{memberExpression.Member.Name}]";
                }
                else if (body is ParameterExpression)
                {
                    selectClause = "*";
                }
                else
                {
                    selectClause = "*";
                }
            }
            catch (Exception)
            {
                selectClause = "*";
            }
        }

        // Build the final SQL
        string sql = $"SELECT {selectClause} FROM {tableName}{where}";

        if (!string.IsNullOrEmpty(orderBy))
        {
            sql += $" {orderBy}";
        }

        // Handle OFFSET and FETCH for pagination
        if (query.SkipField.HasValue || query.TakeField.HasValue)
        {
            if (string.IsNullOrEmpty(orderBy))
            {
                sql += " ORDER BY (SELECT NULL)";
            }
            sql += $" OFFSET {query.SkipField ?? 0} ROWS";
            if (query.TakeField.HasValue)
            {
                sql += $" FETCH NEXT {query.TakeField.Value} ROWS ONLY";
            }
        }

        result.SQL = sql;
        return result;
    }
}
