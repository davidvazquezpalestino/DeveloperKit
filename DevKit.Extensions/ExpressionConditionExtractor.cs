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
            object argValue = GetValue(node.Arguments[0]);
            Conditions.Add((member.Member.Name, node.Method.Name, argValue));
        }
        return base.VisitMethodCall(node);
    }
    /// <summary>
    /// Evalúa un valor desde una expresión (constante o miembro).
    /// </summary>
    /// <summary>
    /// Evalúa un <see cref="Expression"/> y devuelve su valor.
    /// Soporta constantes y miembros (propiedades/campos).
    /// </summary>
    /// <param name="expr">Expresión a evaluar.</param>
    /// <returns>El valor evaluado, o null si no se puede resolver.</returns>
    /// <summary>
    /// Evalúa un <see cref="Expression"/> y devuelve su valor.
    /// Soporta constantes, miembros, conversiones, llamadas a métodos y parámetros.
    /// </summary>
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
            return compiled.DynamicInvoke();
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
    /// Construye una clave de Redis a partir de una expresión y opcionalmente una página.
    /// </summary>
    public static string BuildRedisKey<T>(Expression<Func<T, bool>> expression, int pagina = 0)
    {
        ExpressionConditionExtractor extractor = new();
        extractor.Visit(expression.Body);

        string baseKey = typeof(T).Name;
        List<string> conditionParts = BuildConditionParts(extractor.Conditions);

        if (pagina > 0)
            conditionParts.Add($"Page:{pagina}");

        return $"{baseKey}:{string.Join(":", conditionParts)}";
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

        if (value is System.Collections.IEnumerable enumerable && value is not string)
        {
            IEnumerable<string> items = enumerable.Cast<object>()
                                  .Select(item => item?.ToString() ?? "NULL");
            return $"[{string.Join(",", items)}]";
        }

        return value.ToString().Trim();
    }
}
