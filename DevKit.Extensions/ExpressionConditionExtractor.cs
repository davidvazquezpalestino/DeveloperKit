namespace DevKit.Extensions;

/// <summary>
/// Extracts conditions from lambda expressions used in queries, storing them as property-operator-value tuples.
/// This class is useful for analyzing and transforming LINQ expressions into structured condition lists.
/// </summary>
public class ExpressionConditionExtractor : ExpressionVisitor
{
    /// <summary>
    /// Lista de condiciones extraídas: propiedad, operador y valor.
    /// </summary>
    public List<(string Property, string Operator, object Value)> Conditions { get; } = new();

    /// <summary>
    /// Construye una clave de Redis a partir de una expresion Lambda.
    /// Soporta tanto llamadas a métodos (ej. () => repo.GetAsync(id)) 
    /// como predicados (ej. x => x.Prop == valor).
    /// </summary>
    /// <param name="expression">Expresión Lambda a procesar.</param>
    /// <param name="pagina">Número de página opcional para incluir en la clave.</param>
    /// <returns>Una cadena formateada para ser usada como clave de Redis.</returns>
    public static string BuildRedisKey(LambdaExpression expression, int pagina = 0)
    {
        List<string> parts = expression.ReturnType == typeof(bool)
            ? GetPredicateParts(expression)
            : GetMethodCallParts(expression);

        if (pagina > 0) parts.Add($"Page:{pagina}");

        return string.Join(":", parts);
    }

    /// <summary>
    /// Extrae condiciones de expresiones binarias (ej. x.Prop == valor).
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (node.Left is MemberExpression member)
        {
            object value = GetValue(node.Right);
            Conditions.Add((member.Member.Name, node.NodeType.ToString(), value));
        }
        return base.VisitBinary(node);
    }

    /// <summary>
    /// Extrae condiciones de llamadas a métodos (ej. x.Prop.Contains(valor)).
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Object is MemberExpression member)
        {
            if (node.Arguments.Count > 0)
            {
                object argValue = GetValue(node.Arguments[0]);
                Conditions.Add((member.Member.Name, node.Method.Name, argValue));
            }
            else
            {
                Conditions.Add((member.Member.Name, node.Method.Name, null));
            }
        }
        return base.VisitMethodCall(node);
    }

    #region Private Methods

    /// <summary>
    /// Evalúa un <see cref="Expression"/> y devuelve su valor.
    /// Soporta constantes, miembros, conversiones, llamadas a métodos y parámetros.
    /// </summary>
    /// <param name="expr">Expresión a evaluar.</param>
    /// <returns>El valor evaluado, o un diccionario si es un tipo complejo.</returns>
    private static object GetValue(Expression expr)
    {
        // Caso 1: expresión constante (ej. x => 5)
        if (expr is ConstantExpression constant)
        {
            return constant.Value;
        }

        // Caso 2: expresión de miembro (ej. x => objeto.Propiedad)
        if (expr is MemberExpression member)
        {
            LambdaExpression lambda = Expression.Lambda(member);
            Delegate compiled = lambda.Compile();
            object result = compiled.DynamicInvoke();

            // Si el resultado es un tipo complejo, extraer sus propiedades
            if (result != null && IsComplexType(result.GetType()))
            {
                return ExtractProperties(result);
            }

            return result;
        }

        // Caso 3: expresión unaria (ej. x => (int)otroValor, x => !flag)
        if (expr is UnaryExpression unary)
        {
            try
            {
                LambdaExpression lambda = Expression.Lambda(unary);
                Delegate compiled = lambda.Compile();
                return compiled.DynamicInvoke();
            }
            catch
            {
                // Si no se puede compilar, intentamos obtener el valor del operando
                return GetValue(unary.Operand);
            }
        }

        // Caso 4: llamada a método (ej. x => DateTime.Now, x => Guid.NewGuid())
        if (expr is MethodCallExpression methodCall)
        {
            try
            {
                LambdaExpression lambda = Expression.Lambda(methodCall);
                Delegate compiled = lambda.Compile();
                return compiled.DynamicInvoke();
            }
            catch
            {
                return null;
            }
        }

        // Caso 5: expresión de parámetro (ej. x => x.Propiedad)
        if (expr is ParameterExpression parameter)
        {
            // No tiene valor directo, devolvemos el nombre como referencia
            return parameter.Name;
        }

        // Caso 6: expresión binaria (ej. x => a + b)
        if (expr is BinaryExpression binary)
        {
            try
            {
                LambdaExpression lambda = Expression.Lambda(binary);
                Delegate compiled = lambda.Compile();
                return compiled.DynamicInvoke();
            }
            catch
            {
                return null;
            }
        }

        // Caso 7: cualquier otro tipo de expresión no soportada
        return null;
    }

    /// <summary>
    /// Procesa una expresión de predicado (bool) y extrae sus componentes para la clave.
    /// </summary>
    private static List<string> GetPredicateParts(LambdaExpression expression)
    {
        ExpressionConditionExtractor extractor = new();
        extractor.Visit(expression.Body);

        string typeName = expression.Parameters.Count > 0
            ? GetCleanTypeName(expression.Parameters[0].Type)
            : "Predicate";

        List<string> parts = new List<string> { typeName };
        List<string> conditions = BuildConditionParts(extractor.Conditions);

        if (conditions.Any()) parts.AddRange(conditions);
        else parts.Add("ALL");

        return parts;
    }

    /// <summary>
    /// Procesa una expresión de llamada a método y extrae sus componentes para la clave.
    /// </summary>
    private static List<string> GetMethodCallParts(LambdaExpression expression)
    {
        MethodCallExpression methodCall = expression.Body switch
        {
            MethodCallExpression mc => mc,
            UnaryExpression { Operand: MethodCallExpression mc } => mc,
            _ => throw new ArgumentException("La expresión debe ser una llamada a un método o un predicado.")
        };

        List<string> parts = new List<string>
        {
            GetCleanTypeName(methodCall.Method.ReturnType),
            methodCall.Method.Name
        };

        parts.AddRange(methodCall.Arguments.Select(arg => FormatValue(GetValue(arg))));

        return parts;
    }

    /// <summary>
    /// Obtiene un nombre limpio para el tipo, manejando Task y Genéricos de forma básica.
    /// </summary>
    private static string GetCleanTypeName(Type type)
    {
        // Desempaquetar Task<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            type = type.GetGenericArguments()[0];
        }
        else if (type == typeof(Task))
        {
            return "Void";
        }

        // Manejar List<T>, IEnumerable<T>, etc.
        if (type.IsGenericType)
        {
            Type[] genArgs = type.GetGenericArguments();
            string name = type.Name;
            int tickIndex = name.IndexOf('`');
            if (tickIndex > 0) name = name.Substring(0, tickIndex);

            return $"{name}|{string.Join("|", genArgs.Select(GetCleanTypeName))}|";
        }

        return type.Name;
    }

    /// <summary>
    /// Convierte todas las condiciones en fragmentos de clave, ordenadas para consistencia.
    /// </summary>
    private static List<string> BuildConditionParts(IEnumerable<(string Property, string Operator, object Value)> conditions)
    {
        return conditions
            .OrderBy(c => c.Property)
            .ThenBy(c => c.Operator)
            .Select(c => BuildConditionPart(c.Property, c.Operator, c.Value))
            .ToList();
    }

    /// <summary>
    /// Convierte una condición en un fragmento de clave.
    /// </summary>
    private static string BuildConditionPart(string property, string op, object value)
    {
        string normalizedOperator = NormalizeOperator(op);
        string formattedValue = FormatValue(value);

        return string.IsNullOrEmpty(property)
            ? $"{normalizedOperator}{formattedValue}"
            : $"{property}{normalizedOperator}{formattedValue}";
    }

    /// <summary>
    /// Normaliza el operador para que sea consistente en la clave.
    /// </summary>
    private static string NormalizeOperator(string op) => op switch
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
        _ => op
    };

    /// <summary>
    /// Formatea el valor de la condición para que sea seguro y consistente.
    /// </summary>
    private static string FormatValue(object value)
    {
        if (value == null)
            return "NULL";

        if (value is DateTime dt)
            return dt.ToString("yyyy-MM-ddTHH:mm:ss");

        if (value is bool b)
            return b ? "1" : "0";

        if (value is string s)
        {
            return s;
        }

        if (value is Dictionary<string, object> dict)
        {
            IEnumerable<string> parts = dict.Select(kvp => $"{kvp.Key}|{FormatValue(kvp.Value)}");
            return string.Join(",", parts);
        }

        if (value is System.Collections.IEnumerable enumerable)
        {
            IEnumerable<string> items = enumerable
                .Cast<object>()
                .Select(item => item?.ToString() ?? "NULL");
            return $"[{string.Join(",", items)}]";
        }

        return value.ToString()?.Trim();
    }

    /// <summary>
    /// Determina si un tipo es complejo (no primitivo, string, DateTime, o colección).
    /// </summary>
    private static bool IsComplexType(Type type)
    {
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) || type == typeof(Guid) || type.IsEnum)
        {
            return false;
        }

        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            return false;
        }

        return type.IsClass || type.IsValueType;
    }

    /// <summary>
    /// Extrae las propiedades públicas de un objeto y las devuelve como un diccionario.
    /// </summary>
    private static Dictionary<string, object> ExtractProperties(object obj)
    {
        Dictionary<string, object> properties = new();
        Type type = obj.GetType();

        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                object value = prop.GetValue(obj);
                properties[prop.Name] = value;
            }
            catch
            {
                properties[prop.Name] = null;
            }
        }

        return properties;
    }

    #endregion
}
