namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Métodos de utilidad y privados para SqlQueryBuilderExtensions
/// </summary>
public static partial class SqlQueryBuilderExtensions
{
    private static string GetTableName<T>(QueryState<T> query) where T : class, new()
    {
        // Use explicitly provided table name, then check TableAttribute, then use type name
        string tableName = query.TableName ?? typeof(T).Name;

        // Check for TableAttribute to override table name if not explicitly provided
        if (query.TableName == null)
        {
            TableAttribute tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
            if (tableAttr?.Name != null)
            {
                tableName = tableAttr.Name;
            }
        }

        // Get schema from query state, then from TableAttribute, then default to 'dbo'
        string schema = query.Schema ??
                       typeof(T).GetCustomAttribute<TableAttribute>()?.Schema ??
                       "dbo";

        // Return fully qualified table name with schema
        return $"[{schema}].[{tableName}]";
    }

    /// <summary>
    /// Gets the table name for a projected query state
    /// </summary>
    private static string GetTableName<T, TResult>(ProjectedQueryState<T, TResult> query) where T : class, new()
    {
        return string.IsNullOrEmpty(query.Schema)
            ? $"[{query.TableName}]"
            : $"[{query.Schema}].[{query.TableName}]";
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
            case BinaryExpression binaryExpression:
                string operatorStr = binaryExpression.NodeType switch
                {
                    ExpressionType.Equal => "=",
                    ExpressionType.NotEqual => "!=",
                    ExpressionType.GreaterThan => ">",
                    ExpressionType.GreaterThanOrEqual => ">=",
                    ExpressionType.LessThan => "<",
                    ExpressionType.LessThanOrEqual => "<=",
                    ExpressionType.AndAlso => "AND",
                    ExpressionType.OrElse => "OR",
                    _ => throw new NotSupportedException($"Binary operator {binaryExpression.NodeType} is not supported")
                };

                string left = ProcessExpression(visitor, binaryExpression.Left);
                string right = ProcessExpression(visitor, binaryExpression.Right);

                // For logical operators, wrap in parentheses for proper precedence
                if (binaryExpression.NodeType == ExpressionType.AndAlso || binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    return $"({left} {operatorStr} {right})";
                }
                return $"{left} {operatorStr} {right}";

            case ConstantExpression constantExpression:
                if (constantExpression.Type == typeof(string))
                {
                    return $"'{constantExpression.Value}'";
                }
                return constantExpression.Value?.ToString() ?? "NULL";

            case ParameterExpression paramExpr:
                return paramExpr.Name;

            case MemberExpression memberExpression:
                if (memberExpression.Expression is ParameterExpression)
                {
                    return memberExpression.Member.Name;
                }

                if (memberExpression.Expression is ConstantExpression)
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

            case UnaryExpression unaryExpression:
                if (unaryExpression.NodeType == ExpressionType.Not)
                {
                    string operand = ProcessExpression(visitor, unaryExpression.Operand);
                    // Only wrap in parentheses if the operand is a complex expression
                    if (unaryExpression.Operand is BinaryExpression || unaryExpression.Operand is MethodCallExpression)
                    {
                        return $"NOT ({operand})";
                    }
                    return $"NOT {operand}";
                }
                return ProcessExpression(visitor, unaryExpression.Operand);

            case MethodCallExpression methodCall:
                // Handle common method calls like string.StartsWith, Contains, etc.
                if (methodCall.Method.DeclaringType == typeof(string))
                {
                    string instance = methodCall.Object != null ? ProcessExpression(visitor, methodCall.Object) : null;

                    string[] arguments = methodCall.Arguments
                        .Select(arg => ProcessExpression(visitor, arg))
                        .ToArray();

                    // Obtener el valor del argumento
                    object value;
                    string searchParam = visitor.GetNextParameterName();

                    // Si el argumento es una constante
                    if (methodCall.Arguments[0] is ConstantExpression constantExpression)
                    {
                        value = constantExpression.Value?.ToString() ?? string.Empty;
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

    /// <summary>
    /// Maps a data reader to a projected type using the select expression
    /// </summary>
    private static TResult MapToProjectedType<TResult>(IDataReader reader, Expression expression)
    {
        if (typeof(TResult).IsAnonymousType())
        {
            // Handle anonymous types by creating them dynamically
            ConstructorInfo constructor = typeof(TResult).GetConstructors().First();
            ParameterInfo[] parameters = constructor.GetParameters();
            object[] values = new object[parameters.Length];

            for (int i = 0; i < parameters.Length && i < reader.FieldCount; i++)
            {
                values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            return (TResult)constructor.Invoke(values);
        }

        TResult item = (TResult)Activator.CreateInstance(typeof(TResult));
        Type type = typeof(TResult);

        Dictionary<string, PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);

            if (properties.TryGetValue(columnName, out PropertyInfo prop) && !reader.IsDBNull(i))
            {
                try
                {
                    object value = reader.GetValue(i);
                    if (value != DBNull.Value)
                    {
                        // Handle type conversion
                        Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        if (prop.PropertyType.IsInstanceOfType(value))
                        {
                            prop.SetValue(item, value);
                        }
                        else
                        {
                            object convertedValue = Convert.ChangeType(value, targetType);
                            prop.SetValue(item, convertedValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting property {prop.Name}: {ex.Message}");
                }
            }
        }

        return item;
    }

    /// <summary>
    /// Extension method to check if a type is an anonymous type
    /// </summary>
    private static bool IsAnonymousType(this Type type)
    {
        return type.Name.Contains("AnonymousType") &&
               type.IsGenericType &&
               type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }
}
