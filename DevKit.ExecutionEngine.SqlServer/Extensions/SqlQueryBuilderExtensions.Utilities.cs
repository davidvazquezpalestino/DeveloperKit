
namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Métodos de utilidad y privados para SqlQueryBuilderExtensions.
/// </summary>
public static partial class SqlQueryBuilderExtensions
{
    /// <summary>
    /// Obtiene el nombre completo de la tabla (incluyendo esquema) para un estado de consulta.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad.</typeparam>
    /// <param name="query">El estado de la consulta.</param>
    /// <returns>El nombre de la tabla formateado como [Esquema].[Tabla].</returns>
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
    /// Obtiene el nombre completo de la tabla para un estado de consulta proyectada.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad original.</typeparam>
    /// <typeparam name="TResult">El tipo del resultado proyectado.</typeparam>
    /// <param name="query">El estado de la consulta proyectada.</param>
    /// <returns>El nombre de la tabla formateado con esquema si está disponible.</returns>
    private static string GetTableName<T, TResult>(ProjectedQueryState<T, TResult> query) where T : class, new()
    {
        return string.IsNullOrEmpty(query.Schema)
            ? $"[{query.TableName}]"
            : $"[{query.Schema}].[{query.TableName}]";
    }

    /// <summary>
    /// Procesa una expresión de filtrado para generar la cadena SQL correspondiente.
    /// </summary>
    /// <param name="visitor">El visitador de expresiones.</param>
    /// <param name="expression">La expresión a procesar.</param>
    /// <returns>La representación SQL de la expresión.</returns>
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

                    Expression arg = methodCall.Arguments[0];
                    //string paramName;
                    string baseValue;

                    // If the argument was already converted to a parameter (e.g. @p0),
                    // reuse that same parameter and transform its value into the LIKE pattern
                    if (arg is ParameterExpression paramExpr && visitor.Parameters.TryGetValue(paramExpr.Name, out object existingValue))
                    {
                        baseValue = existingValue?.ToString() ?? string.Empty;
                        paramName = paramExpr.Name;
                    }
                    else
                    {
                        // Otherwise, evaluate the argument (constant or closure variable)
                        if (arg is ConstantExpression constExpr)
                        {
                            baseValue = constExpr.Value?.ToString() ?? string.Empty;
                        }
                        else
                        {
                            baseValue = Expression.Lambda(arg).Compile().DynamicInvoke()?.ToString() ?? string.Empty;
                        }

                        paramName = visitor.GetNextParameterName();
                    }

                    // Apply the corresponding pattern according to the method
                    visitor.Parameters[paramName] = methodCall.Method.Name switch
                    {
                        nameof(string.StartsWith) => $"{baseValue}%",
                        nameof(string.EndsWith) => $"%{baseValue}",
                        nameof(string.Contains) => $"%{baseValue}%",
                        _ => baseValue
                    };

                    return $"{instance} LIKE {paramName}";
                }
                throw new NotSupportedException($"Method calls on type {methodCall.Method.DeclaringType?.Name} are not supported");

            default:
                throw new NotSupportedException($"Expression of type {expression.NodeType} is not supported");
        }
    }

    /// <summary>
    /// Obtiene el nombre del miembro a partir de una expresión.
    /// </summary>
    /// <param name="expression">La expresión a analizar.</param>
    /// <returns>El nombre del miembro.</returns>
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
    /// Maps a data reader to a projected type using the select expression.
    /// </summary>
    /// <typeparam name="TResult">El tipo del resultado.</typeparam>
    /// <param name="reader">El lector de datos.</param>
    /// <param name="expression">La expresión de selección.</param>
    /// <returns>Una instancia de TResult con los datos mapeados.</returns>
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
    /// Determina si un tipo es un tipo anónimo.
    /// </summary>
    /// <param name="type">El tipo a verificar.</param>
    /// <returns>True si es un tipo anónimo; de lo contrario, false.</returns>
    private static bool IsAnonymousType(this Type type)
    {
        return type.Name.Contains("AnonymousType") &&
               type.IsGenericType &&
               type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }
}
