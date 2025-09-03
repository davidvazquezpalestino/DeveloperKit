namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Métodos de extensión para construir consultas SQL de manera fluida.
/// </summary>
public static partial class SqlQueryBuilderExtensions
{
    /// <summary>
    /// Inicia una nueva consulta para el tipo de entidad especificado.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad a consultar</typeparam>
    /// <param name="dbProvider">La instancia del proveedor de base de datos</param>
    /// <returns>Una nueva instancia del constructor de consultas</returns>
    public static QueryState<T> From<T>(this ISQLServerProvider dbProvider) where T : class, new()
    {
        return new QueryState<T>(dbProvider);
    }

    /// <summary>
    /// Inicia una nueva consulta para la tabla especificada con un esquema personalizado.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad a consultar</typeparam>
    /// <param name="dbProvider">La instancia del proveedor de base de datos</param>
    /// <param name="schema">El nombre del esquema de la base de datos (ej. "dbo")</param>
    /// <param name="tableName">El nombre de la tabla (opcional, usa el nombre de la clase si no se proporciona)</param>
    /// <returns>Una nueva instancia del constructor de consultas</returns>
    public static QueryState<T> From<T>(this ISQLServerProvider dbProvider, string schema, string tableName = null) where T : class, new()
    {
        return new QueryState<T>(dbProvider, schema, tableName);
    }

    /// <summary>
    /// Ejecuta la consulta y devuelve los resultados como una lista.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <returns>Una lista de entidades que coinciden con la consulta</returns>
    public static List<T> ToList<T>(this QueryState<T> queryState) where T : class, new()
    {
        QueryResult queryResult = BuildQuery(queryState);
        ICollection<T> result = queryState.DbProvider.ExecuteQueryAsList(queryResult.SQL,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters));
        return result.ToList();
    }

    /// <summary>
    /// Especifica qué propiedades incluir en los resultados de la consulta.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <typeparam name="TResult">El tipo del resultado</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="selector">Una expresión lambda que especifica qué propiedades incluir</param>
    /// <returns>El estado de la consulta con la expresión de selección aplicada</returns>
    public static QueryState<T> Select<T, TResult>(this QueryState<T> queryState, Expression<Func<T, TResult>> selector) where T : class, new()
    {
        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        // Create a new state to maintain immutability
        QueryState<T> newState = new(queryState.DbProvider, queryState.Schema, queryState.TableName)
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
                Func<T, TResult> compiled = selector.Compile();
                TResult result = compiled(default);
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

    // Los métodos asíncronos han sido movidos a SqlQueryBuilderExtensions.Async.cs

    /// <summary>
    /// Ejecuta la consulta y devuelve el primer resultado, o null si no se encuentran resultados.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <returns>La primera entidad que coincide con la consulta, o null</returns>
    public static T FirstOrDefault<T>(this QueryState<T> queryState) where T : class, new()
    {
        // Limitar a un solo resultado
        queryState.TakeField = 1;

        // Construir la consulta
        QueryResult queryResult = BuildQuery(queryState);

        // Registrar la consulta que se va a ejecutar
        QueryLogger.LogQuery(
            queryResult.SQL,
            queryResult.Parameters,
            IQueryLogger.LogLevel.Debug,
            $"Ejecutando consulta FirstOrDefault para {typeof(T).Name}");

        // Ejecutar la consulta
        ICollection<T> results = queryState.DbProvider.ExecuteQueryAsList(
            queryResult.SQL,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters));

        // Obtener el primer resultado (o null)
        T result = results.FirstOrDefault();

        // Registrar el resultado
        QueryLogger.LogQuery(
            queryResult.SQL,
            queryResult.Parameters,
            IQueryLogger.LogLevel.Debug,
            $"Consulta FirstOrDefault completada. Se encontró {(result != null ? "1 registro" : "ningún registro")} de {typeof(T).Name}");

        return result;
    }

    // Los métodos asíncronos han sido movidos a SqlQueryBuilderExtensions.Async.cs

    /// <summary>
    /// Devuelve el primer elemento de la secuencia que satisface una condición o lanza una excepción si no se encuentra ningún elemento.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <returns>El primer elemento que coincide con la consulta</returns>
    /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos</exception>
    public static T First<T>(this QueryState<T> queryState) where T : class, new()
    {
        queryState.TakeField = 1;
        QueryResult queryResult = BuildQuery(queryState);

        ICollection<T> results = queryState.DbProvider.ExecuteQueryAsList(queryResult.SQL,
            reader => reader.GetItem<T>(),
            collection => collection.AddSqlParameters(queryResult.Parameters));

        return results.First();
    }

    /// <summary>
    /// Devuelve el primer elemento de la secuencia que satisface una condición o lanza una excepción si no se encuentra ningún elemento.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="predicate">Función para probar cada elemento para una condición</param>
    /// <returns>El primer elemento que coincide con la condición</returns>
    /// <exception cref="InvalidOperationException">Se lanza cuando la secuencia no contiene elementos que cumplan la condición</exception>
    public static T First<T>(this QueryState<T> queryState, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        return queryState.Where(predicate).First();
    }

    // Los métodos asíncronos han sido movidos a SqlQueryBuilderExtensions.Async.cs

    // Los métodos asíncronos han sido movidos a SqlQueryBuilderExtensions.Async.cs

    /// <summary>
    /// Devuelve el número total de elementos en la secuencia.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <returns>El número total de elementos en la secuencia</returns>
    public static int Count<T>(this QueryState<T> queryState) where T : class, new()
    {
        // Guardar el estado original para no afectar a la consulta original
        int? originalTake = queryState.TakeField;
        int? originalSkip = queryState.SkipField;
        Expression<Func<T, object>> originalSelect = queryState.SelectExpression;

        try
        {
            // Modificar la consulta para contar
            queryState.TakeField = null;
            queryState.SkipField = 0;
            queryState.SelectExpression = null;

            QueryResult queryResult = BuildQuery(queryState);
            string countQuery = $"SELECT COUNT(*) FROM ({queryResult.SQL}) AS CountQuery";

            return queryState.DbProvider.ExecuteScalar<int>(countQuery,
                collection => collection.AddSqlParameters(queryResult.Parameters));
        }
        finally
        {
            // Restaurar el estado original
            queryState.TakeField = originalTake;
            queryState.SkipField = originalSkip;
            queryState.SelectExpression = originalSelect;
        }
    }

    /// <summary>
    /// Devuelve el número de elementos de la secuencia que satisfacen una condición.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="predicate">Función para probar cada elemento para una condición</param>
    /// <returns>El número de elementos de la secuencia que satisfacen la condición</returns>
    public static int Count<T>(this QueryState<T> queryState, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        return queryState.Where(predicate).Count();
    }

    // Los métodos asíncronos han sido movidos a SqlQueryBuilderExtensions.Async.cs

    // Los métodos asíncronos han sido movidos a SqlQueryBuilderExtensions.Async.cs

    /// <summary>
    /// Determina si una secuencia contiene elementos.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <returns>true si la secuencia contiene elementos; de lo contrario, false</returns>
    public static bool Any<T>(this QueryState<T> queryState) where T : class, new()
    {
        // Optimización: Usamos TOP 1 en lugar de COUNT para mayor eficiencia
        int? originalTake = queryState.TakeField;
        try
        {
            queryState.TakeField = 1;
            QueryResult queryResult = BuildQuery(queryState);
            string existsQuery = $"SELECT CASE WHEN EXISTS ({queryResult.SQL}) THEN 1 ELSE 0 END";

            return queryState.DbProvider.ExecuteScalar<int>(existsQuery,
                collection => collection.AddSqlParameters(queryResult.Parameters)) == 1;
        }
        finally
        {
            queryState.TakeField = originalTake;
        }
    }

    /// <summary>
    /// Determina si algún elemento de una secuencia satisface una condición.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="predicate">Función para probar cada elemento para una condición</param>
    /// <returns>true si algún elemento de la secuencia supera la prueba en el predicado especificado; de lo contrario, false</returns>
    public static bool Any<T>(this QueryState<T> queryState, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        return queryState.Where(predicate).Any();
    }

    // Los métodos asíncronos han sido movidos a SqlQueryBuilderExtensions.Async.cs

    // Los métodos asíncronos han sido movidos a SqlQueryBuilderExtensions.Async.cs

    /// <summary>
    /// Ejecuta la consulta y devuelve los resultados como un arreglo.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <returns>Un arreglo de entidades que coinciden con la consulta</returns>
    public static T[] ToArray<T>(this QueryState<T> queryState) where T : class, new()
    {
        return queryState.ToList().ToArray();
    }

    // Los métodos asíncronos han sido movidos a SqlQueryBuilderExtensions.Async.cs

    /// <summary>
    /// Filtra los resultados de la consulta según un predicado.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="predicate">Una función para probar cada elemento para una condición</param>
    /// <returns>El estado de la consulta con el filtro aplicado</returns>
    public static QueryState<T> Where<T>(this QueryState<T> queryState, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        queryState.WhereExpressions.Add(predicate);
        return queryState;
    }

    /// <summary>
    /// Ordena los elementos de la consulta en orden ascendente según una clave.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="keySelector">Una función para extraer una clave de un elemento</param>
    /// <returns>El estado de la consulta con el ordenamiento aplicado</returns>
    public static QueryState<T> OrderBy<T, TKey>(this QueryState<T> queryState, Expression<Func<T, TKey>> keySelector) where T : class, new()
    {
        queryState.OrderByField.Add((GetMemberName(keySelector.Body), true));
        return queryState;
    }

    /// <summary>
    /// Ordena los elementos de la consulta en orden descendente según una clave.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="keySelector">Una función para extraer una clave de un elemento</param>
    /// <returns>El estado de la consulta con el ordenamiento aplicado</returns>
    public static QueryState<T> OrderByDescending<T, TKey>(this QueryState<T> queryState, Expression<Func<T, TKey>> keySelector) where T : class, new()
    {
        queryState.OrderByField.Add((GetMemberName(keySelector.Body), false));
        return queryState;
    }

    /// <summary>
    /// Realiza una ordenación posterior de los elementos en una secuencia en orden descendente.
    /// </summary>
    /// <typeparam name="T">El tipo de los elementos</typeparam>
    /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="keySelector">Una función para extraer una clave de un elemento</param>
    /// <returns>El estado de la consulta con la ordenación posterior aplicada</returns>
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
    /// Omite un número especificado de elementos en los resultados de la consulta y luego devuelve los elementos restantes.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="count">El número de elementos a omitir</param>
    /// <returns>El estado de la consulta con la omisión aplicada</returns>
    public static QueryState<T> Skip<T>(this QueryState<T> queryState, int count) where T : class, new()
    {
        queryState.SkipField = count;
        return queryState;
    }

    /// <summary>
    /// Devuelve un número específico de elementos contiguos desde el inicio de los resultados de la consulta.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <param name="queryState">El estado de la consulta</param>
    /// <param name="count">El número de elementos a devolver</param>
    /// <returns>El estado de la consulta con la limitación aplicada</returns>
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

    internal static QueryResult BuildQuery<T>(QueryState<T> queryState) where T : class, new()
    {
        QueryResult result = new();
        string tableName = GetTableName(queryState);
        string whereClause = "";
        string orderByClause = "";
        int paramIndex = 0;

        // Build WHERE clause
        if (queryState.WhereExpressions.Count > 0)
        {
            List<string> whereConditions = new();
            // Crear el visitador con el diccionario de parámetros compartido
            WhereExpressionVisitor visitor = new(result.Parameters, ref paramIndex);

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
                        List<string> properties = new();
                        foreach (Expression arg in newExpression.Arguments)
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
        result.SQL = $"SELECT {selectClause} FROM {tableName}";

        if (!string.IsNullOrEmpty(whereClause))
        {
            result.SQL += whereClause;
        }

        if (!string.IsNullOrEmpty(orderByClause))
        {
            result.SQL += $" {orderByClause}";
        }

        // Add pagination if needed
        if (queryState.TakeField.HasValue && queryState.TakeField > 0)
        {
            if (queryState.SkipField > 0)
            {
                result.SQL += $" OFFSET {queryState.SkipField} ROWS FETCH NEXT {queryState.TakeField} ROWS ONLY";
            }
            else
            {
                result.SQL += $" TOP {queryState.TakeField}";
            }
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