using System.Reflection;

namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>
/// Métodos de extensión para construir consultas SQL de manera fluida.
/// </summary>
public static partial class SqlQueryBuilderExtensions
{
    /// <summary>
    /// Inicia una nueva consulta para la tabla especificada con un esquema personalizado.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad a consultar</typeparam>
    /// <param name="dbProvider">La instancia del proveedor de base de datos</param>
    /// <param name="schema">El nombre del esquema de la base de datos (ej. "dbo")</param>
    /// <param name="tableName">El nombre de la tabla (opcional, usa el nombre de la clase si no se proporciona)</param>
    /// <returns>Una nueva instancia del constructor de consultas</returns>
    public static QueryState<T> From<T>(this ISQLServerProvider dbProvider, string schema = "dbo", string tableName = null) where T : class, new()
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
    /// <param name="query">El estado de la consulta</param>
    /// <param name="expression">Una expresión lambda que especifica qué propiedades incluir</param>
    /// <returns>El estado de la consulta con la expresión de selección aplicada</returns>
    public static ProjectedQueryState<T, TResult> Select<T, TResult>(this QueryState<T> query, Expression<Func<T, TResult>> expression) where T : class, new()
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        // Create a new projected state
        ProjectedQueryState<T, TResult> newState = new(query.DbProvider, query.Schema, query.TableName)
        {
            TakeField = query.TakeField,
            SkipField = query.SkipField,
            SelectExpression = expression
        };

        newState.Where.AddRange(query.Where);
        newState.OrderBy.AddRange(query.OrderBy);

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
    /// <param name="query">El estado de la consulta</param>
    /// <param name="predicate">Una función para probar cada elemento para una condición</param>
    /// <returns>El estado de la consulta con el filtro aplicado</returns>
    public static QueryState<T> Where<T>(this QueryState<T> query, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        query.Where.Add(predicate);
        return query;
    }

    /// <summary>
    /// Ordena los elementos de la consulta en orden ascendente según una clave.
    /// </summary>
    /// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
    /// <typeparam name="TKey">El tipo de la clave devuelta por la función representada por keySelector</typeparam>
    /// <param name="query">El estado de la consulta</param>
    /// <param name="expression">Una función para extraer una clave de un elemento</param>
    /// <returns>El estado de la consulta con el ordenamiento aplicado</returns>
    public static QueryState<T> OrderBy<T, TKey>(this QueryState<T> query, Expression<Func<T, TKey>> expression) where T : class, new()
    {
        query.OrderBy.Add((GetMemberName(expression.Body), true));
        return query;
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
        queryState.OrderBy.Add((GetMemberName(keySelector.Body), false));
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
        if (queryState.OrderBy.Count == 0)
        {
            throw new InvalidOperationException("ThenByDescending must be called after OrderBy or OrderByDescending");
        }

        queryState.OrderBy.Add((GetMemberName(keySelector.Body), false));
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
        result.SQL = $"SELECT {selectClause} FROM {tableName}";

        if (!string.IsNullOrEmpty(where))
        {
            result.SQL += where;
        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            result.SQL += $" {orderBy}";
        }

        // Add pagination if needed
        if (query.TakeField.HasValue && query.TakeField > 0)
        {
            if (query.SkipField > 0)
            {
                result.SQL += $" OFFSET {query.SkipField} ROWS FETCH NEXT {query.TakeField} ROWS ONLY";
            }
            else
            {
                result.SQL += $" TOP {query.TakeField}";
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

    /// <summary>
    /// Gets the table name for a projected query state
    /// </summary>
    private static string GetTableName<T, TResult>(ProjectedQueryState<T, TResult> query) where T : class, new()
    {
        return string.IsNullOrEmpty(query.Schema) 
            ? $"[{query.TableName}]" 
            : $"[{query.Schema}].[{query.TableName}]";
    }

    /// <summary>
    /// Executes a projected query and returns the results as a list.
    /// </summary>
    /// <typeparam name="T">The source entity type</typeparam>
    /// <typeparam name="TResult">The projected result type</typeparam>
    /// <param name="queryState">The projected query state</param>
    /// <returns>A list of projected results</returns>
    public static List<TResult> ToList<T, TResult>(this ProjectedQueryState<T, TResult> queryState) where T : class, new()
    {
        QueryResult queryResult = BuildQuery(queryState);
        
        // For anonymous types or complex projections, we need to use dynamic mapping
        if (typeof(TResult).IsAnonymousType() || typeof(TResult) != typeof(T))
        {
            // Use dynamic reader that can handle projections
            ICollection<TResult> result = queryState.DbProvider.ExecuteQueryAsList(queryResult.SQL,
                reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
                collection => collection.AddSqlParameters(queryResult.Parameters));
            return result.ToList();
        }
        else
        {
            // For same-type projections, use the standard mapping
            ICollection<TResult> result = queryState.DbProvider.ExecuteQueryAsList(queryResult.SQL,
                reader => MapToProjectedType<TResult>(reader, queryState.SelectExpression.Body),
                collection => collection.AddSqlParameters(queryResult.Parameters));
            return result.ToList();
        }
    }

    /// <summary>
    /// Maps a data reader to a projected type using the select expression
    /// </summary>
    private static TResult MapToProjectedType<TResult>(IDataReader reader, Expression selectExpression)
    {
        // This is a simplified implementation - in a real scenario, you'd need more sophisticated mapping
        // For now, we'll use reflection to create the result type
        
        if (typeof(TResult).IsAnonymousType())
        {
            // Handle anonymous types by creating them dynamically
            var constructor = typeof(TResult).GetConstructors().First();
            var parameters = constructor.GetParameters();
            var values = new object[parameters.Length];
            
            for (int i = 0; i < parameters.Length && i < reader.FieldCount; i++)
            {
                values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            
            return (TResult)constructor.Invoke(values);
        }
        else
        {
            // Handle regular types using Activator.CreateInstance since TResult may not have new() constraint
            TResult item = (TResult)Activator.CreateInstance(typeof(TResult));
            Type type = typeof(TResult);
            
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
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
                        System.Diagnostics.Debug.WriteLine($"Error setting property {prop.Name}: {ex.Message}");
                    }
                }
            }

            return item;
        }
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