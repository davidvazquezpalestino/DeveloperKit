namespace DevKit.ExecutionEngine.Redis;

/// <summary>
/// Extrae condiciones de expresiones lambda utilizadas en consultas, almacenándolas como tuplas propiedad-operador-valor.
/// Esta clase es útil para analizar y transformar expresiones LINQ en listas de condiciones estructuradas.
/// </summary>
public class ExpressionConditionExtractor : ExpressionVisitor
{
    /// <summary>
    /// Lista de condiciones extraídas: propiedad, operador y valor.
    /// </summary>
    public List<(string Property, string Operator, object Value)> Conditions { get; } = new();

    /// <summary>
    /// Construye una clave de Redis a partir de una expresion Lambda.
    /// Genera una cadena formateada legible con las condiciones extraídas.
    /// </summary>
    /// <param name="expression">Expresión Lambda a procesar.</param>
    /// <param name="pageNumber">Número de página opcional para incluir en la clave.</param>
    /// <returns>Una cadena formateada como clave de Redis.</returns>
    public static string BuildRedisKey(LambdaExpression expression, int pageNumber = 0)
    {
        List<string> keyParts = expression.ReturnType == typeof(bool)
            ? GetPredicateParts(expression)
            : GetMethodCallParts(expression);

        if (pageNumber > 0)
        {
            keyParts.Add($"Page:{pageNumber}");
        }

        return string.Join(":", keyParts);
    }

    /// <summary>
    /// Extrae condiciones de expresiones binarias (ej. x.Prop == valor).
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        if (binaryExpression.Left is MemberExpression memberExpression)
        {
            object extractedValue = GetValue(binaryExpression.Right);
            string operatorName = binaryExpression.NodeType.ToString();
            string propertyName = memberExpression.Member.Name;

            Conditions.Add((propertyName, operatorName, extractedValue));
        }
        return base.VisitBinary(binaryExpression);
    }

    /// <summary>
    /// Extrae condiciones de llamadas a métodos (ej. x.Prop.Contains(valor)).
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.Object is MemberExpression memberExpression)
        {
            string propertyName = memberExpression.Member.Name;
            string methodName = methodCallExpression.Method.Name;

            if (methodCallExpression.Arguments.Count > 0)
            {
                object argumentValue = GetValue(methodCallExpression.Arguments[0]);
                Conditions.Add((propertyName, methodName, argumentValue));
            }
            else
            {
                Conditions.Add((propertyName, methodName, null));
            }
        }
        return base.VisitMethodCall(methodCallExpression);
    }

    #region Private Methods

    /// <summary>
    /// Procesa una expresión de predicado (bool) y extrae sus componentes para la clave.
    /// </summary>
    private static List<string> GetPredicateParts(LambdaExpression expression)
    {
        ExpressionConditionExtractor conditionExtractor = new();
        conditionExtractor.Visit(expression.Body);

        List<string> keyParts = new();
        List<string> conditionParts = BuildConditionParts(conditionExtractor.Conditions);

        return conditionParts.Any()
            ? keyParts.Concat(conditionParts).ToList()
            : new List<string> { "ALL" };
    }

    /// <summary>
    /// Procesa una expresión de llamada a método y extrae sus componentes para la clave.
    /// </summary>
    private static List<string> GetMethodCallParts(LambdaExpression expression)
    {
        MethodCallExpression methodCallExpression = ExtractMethodCallExpression(expression.Body);

        List<string> keyParts = new()
        {
            GetDeclaringTypeName(methodCallExpression.Method),
            methodCallExpression.Method.Name,
            GetCleanTypeName(methodCallExpression.Method.ReturnType)
        };

        List<string> argumentParts = ExtractArgumentParts(methodCallExpression.Arguments);
        keyParts.AddRange(argumentParts);

        return keyParts;
    }

    /// <summary>
    /// Extrae la expresión de llamada a método del cuerpo de la expresión.
    /// </summary>
    private static MethodCallExpression ExtractMethodCallExpression(Expression expressionBody)
    {
        return expressionBody switch
        {
            MethodCallExpression methodCall => methodCall,
            UnaryExpression { Operand: MethodCallExpression methodCall } => methodCall,
            _ => throw new ArgumentException("La expresión debe ser una llamada a un método o un predicado.")
        };
    }

    /// <summary>
    /// Extrae y formatea los argumentos de una llamada a método.
    /// </summary>
    private static List<string> ExtractArgumentParts(IEnumerable<Expression> arguments)
    {
        return arguments
            .Select(argument => FormatValue(GetValue(argument)))
            .ToList();
    }

    /// <summary>
    /// Convierte todas las condiciones en fragmentos de clave, ordenadas para consistencia.
    /// </summary>
    private static List<string> BuildConditionParts(IEnumerable<(string Property, string Operator, object Value)> conditions)
    {
        return conditions.OrderBy(c => c.Property)
                        .ThenBy(c => c.Operator)
                        .Select(c => BuildConditionPart(c.Property, c.Operator, c.Value))
                        .ToList();
    }

    /// <summary>
    /// Convierte una condición en un fragmento de clave.
    /// </summary>
    private static string BuildConditionPart(string propertyName, string operatorName, object propertyValue)
    {
        string normalizedOperator = NormalizeOperator(operatorName);
        string formattedValue = FormatValue(propertyValue);

        return string.IsNullOrEmpty(propertyName)
            ? $"{normalizedOperator}{formattedValue}"
            : $"{propertyName}{normalizedOperator}{formattedValue}";
    }

    /// <summary>
    /// Normaliza el operador para que sea consistente en la clave.
    /// </summary>
    private static string NormalizeOperator(string operatorName) => operatorName switch
    {
        "Equal" => "=",
        "NotEqual" => "!=",
        "GreaterThan" => ">",
        "GreaterThanOrEqual" => ">=",
        "LessThan" => "<",
        "LessThanOrEqual" => "<=",
        "StartsWith" => "^=",
        "EndsWith" => "=$",
        "Contains" => "*=",
        "In" => "IN",
        "NotIn" => "NOT IN",
        "IsNull" => "IS NULL",
        "IsNotNull" => "IS NOT NULL",
        "Between" => "BETWEEN",
        _ => operatorName
    };

    /// <summary>
    /// Obtiene el nombre limpio del tipo que declara el método.
    /// </summary>
    private static string GetDeclaringTypeName(MethodInfo method)
    {
        Type declaringType = method.DeclaringType;
        if (declaringType == null)
        {
            return "Unknown";
        }

        return GetCleanTypeName(declaringType);
    }

    /// <summary>
    /// Obtiene un nombre limpio para el tipo, manejando Task y Genéricos de forma básica.
    /// </summary>
    private static string GetCleanTypeName(Type type)
    {
        if (IsTaskType(type))
        {
            return ExtractTaskTypeName(type);
        }

        if (type.IsGenericType)
        {
            return FormatGenericTypeName(type);
        }

        return type.Name;
    }

    /// <summary>
    /// Determina si el tipo es un Task o Task&lt;T&gt;.
    /// </summary>
    private static bool IsTaskType(Type type)
    {
        return type == typeof(Task) ||
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>));
    }

    /// <summary>
    /// Extrae el nombre del tipo Task o Task&lt;T&gt;.
    /// </summary>
    private static string ExtractTaskTypeName(Type type)
    {
        if (type == typeof(Task))
        {
            return "Void";
        }

        Type genericArgument = type.GetGenericArguments()[0];
        return GetCleanTypeName(genericArgument);
    }

    /// <summary>
    /// Formatea el nombre de un tipo genérico.
    /// </summary>
    private static string FormatGenericTypeName(Type type)
    {
        Type[] genericArguments = type.GetGenericArguments();
        string typeName = ExtractTypeNameWithoutGenericTick(type.Name);
        string formattedGenericArguments = string.Join("|", genericArguments.Select(GetCleanTypeName));

        return $"{typeName}|{formattedGenericArguments}|";
    }

    /// <summary>
    /// Extrae el nombre del tipo sin el carácter genérico `.
    /// </summary>
    private static string ExtractTypeNameWithoutGenericTick(string typeName)
    {
        int tickIndex = typeName.IndexOf('`');
        return tickIndex > 0 ? typeName.Substring(0, tickIndex) : typeName;
    }

    /// <summary>
    /// Formatea el valor de la condición para que sea seguro y consistente.
    /// </summary>
    private static string FormatValue(object value)
    {
        if (value == null)
        {
            return "NULL";
        }

        return value switch
        {
            DateTime dateTimeValue => FormatDateTimeValue(dateTimeValue),
            bool booleanValue => FormatBooleanValue(booleanValue),
            string stringValue => stringValue,
            LambdaExpression lambdaExpression => BuildRedisKey(lambdaExpression),
            Dictionary<string, object> dictionaryValue => FormatDictionaryValue(dictionaryValue),
            System.Collections.IEnumerable enumerableValue => FormatEnumerableValue(enumerableValue),
            _ => value.ToString()?.Trim()
        };
    }

    /// <summary>
    /// Formatea un valor DateTime en formato ISO 8601.
    /// </summary>
    private static string FormatDateTimeValue(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
    }

    /// <summary>
    /// Formatea un valor booleano como '1' o '0'.
    /// </summary>
    private static string FormatBooleanValue(bool booleanValue)
    {
        return booleanValue ? "1" : "0";
    }

    /// <summary>
    /// Formatea un valor Dictionary como una cadena de pares clave-valor.
    /// </summary>
    private static string FormatDictionaryValue(Dictionary<string, object> dictionaryValue)
    {
        IEnumerable<string> dictionaryParts = dictionaryValue
            .Select(keyValuePair => $"{keyValuePair.Key}|{FormatValue(keyValuePair.Value)}");
        return string.Join(",", dictionaryParts);
    }

    /// <summary>
    /// Formatea un valor IEnumerable como una cadena de elementos.
    /// </summary>
    private static string FormatEnumerableValue(System.Collections.IEnumerable enumerableValue)
    {
        IEnumerable<string> collectionItems = enumerableValue
            .Cast<object>()
            .Select(item => item?.ToString() ?? "NULL");
        return $"[{string.Join(",", collectionItems)}]";
    }

    /// <summary>
    /// Evalúa un <see cref="Expression"/> y devuelve su valor.
    /// Soporta constantes, miembros, conversiones, llamadas a métodos y parámetros.
    /// </summary>
    /// <param name="expression">Expresión a evaluar.</param>
    /// <returns>El valor evaluado, o un diccionario si es un tipo complejo.</returns>
    private static object GetValue(Expression expression)
    {
        return expression switch
        {
            ConstantExpression constantExpression => constantExpression.Value,
            MemberExpression memberExpression => EvaluateMemberExpression(memberExpression),
            UnaryExpression unaryExpression => EvaluateUnaryExpression(unaryExpression),
            MethodCallExpression methodCallExpression => EvaluateMethodCallExpression(methodCallExpression),
            ParameterExpression parameterExpression => parameterExpression.Name,
            BinaryExpression binaryExpression => EvaluateBinaryExpression(binaryExpression),
            _ => null
        };
    }

    /// <summary>
    /// Evalúa una expresión de miembro y extrae su valor.
    /// </summary>
    private static object EvaluateMemberExpression(MemberExpression memberExpression)
    {
        LambdaExpression lambdaExpression = Expression.Lambda(memberExpression);
        Delegate compiledLambda = lambdaExpression.Compile();
        object evaluationResult = compiledLambda.DynamicInvoke();

        if (evaluationResult is LambdaExpression nestedLambdaExpression)
        {
            return nestedLambdaExpression;
        }

        return evaluationResult != null && IsComplexType(evaluationResult.GetType())
            ? ExtractProperties(evaluationResult)
            : evaluationResult;
    }

    /// <summary>
    /// Evalúa una expresión unaria de forma segura.
    /// </summary>
    private static object EvaluateUnaryExpression(UnaryExpression unaryExpression)
    {
        try
        {
            LambdaExpression lambdaExpression = Expression.Lambda(unaryExpression);
            Delegate compiledLambda = lambdaExpression.Compile();
            return compiledLambda.DynamicInvoke();
        }
        catch
        {
            return GetValue(unaryExpression.Operand);
        }
    }

    /// <summary>
    /// Evalúa una expresión de llamada a método de forma segura.
    /// </summary>
    private static object EvaluateMethodCallExpression(MethodCallExpression methodCallExpression)
    {
        try
        {
            LambdaExpression lambdaExpression = Expression.Lambda(methodCallExpression);
            Delegate compiledLambda = lambdaExpression.Compile();
            return compiledLambda.DynamicInvoke();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Evalúa una expresión binaria de forma segura.
    /// </summary>
    private static object EvaluateBinaryExpression(BinaryExpression binaryExpression)
    {
        try
        {
            LambdaExpression lambdaExpression = Expression.Lambda(binaryExpression);
            Delegate compiledLambda = lambdaExpression.Compile();
            return compiledLambda.DynamicInvoke();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determina si un tipo es complejo (no primitivo, string, DateTime, o colección).
    /// </summary>
    private static bool IsComplexType(Type type)
    {
        return IsSimpleType(type) == false && IsCollectionType(type) == false;
    }

    /// <summary>
    /// Determina si un tipo es simple (primitivo, string, DateTime, etc.).
    /// </summary>
    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid) ||
               type.IsEnum;
    }

    /// <summary>
    /// Determina si un tipo es una colección (pero no string).
    /// </summary>
    private static bool IsCollectionType(Type type)
    {
        return typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string);
    }

    /// <summary>
    /// Extrae las propiedades públicas de un objeto y las devuelve como un diccionario.
    /// </summary>
    private static Dictionary<string, object> ExtractProperties(object sourceObject)
    {
        Dictionary<string, object> extractedProperties = new();
        Type objectType = sourceObject.GetType();

        foreach (PropertyInfo propertyInfo in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                object propertyValue = propertyInfo.GetValue(sourceObject);
                extractedProperties[propertyInfo.Name] = propertyValue;
            }
            catch
            {
                extractedProperties[propertyInfo.Name] = null;
            }
        }

        return extractedProperties;
    }

    #endregion
}
