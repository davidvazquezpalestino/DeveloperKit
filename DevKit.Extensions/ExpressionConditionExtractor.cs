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
    /// Construye una clave de Redis a partir de una expresion Lambda (ej. () => repo.Metodo(args)).
    /// </summary>
    public static string BuildRedisKey(LambdaExpression expression, int pagina = 0)
    {
        Expression body = expression.Body;

        // Si es una llamada asíncrona que no ha sido esperada (Task<T>), el cuerpo es la llamada al método.
        // Si usamos await, el compilador genera una máquina de estados, pero en una expresión lambda expression tree,
        // normalmente tenemos la invocación directa.

        MethodCallExpression methodCall = null;
        if (body is MethodCallExpression mc)
        {
            methodCall = mc;
        }
        else if (body is UnaryExpression unary)
        {
            if (unary.Operand is MethodCallExpression mcFromUnary)
            {
                // Conversiones implícitas o explicatas
                methodCall = mcFromUnary;
            }
        }

        if (methodCall == null)
        {
            throw new ArgumentException("La expresión debe ser una llamada a un método.");
        }

        // Obtener el nombre del tipo de retorno limpio
        string typeName = GetCleanTypeName(methodCall.Method.ReturnType);

        // Obtener el nombre del método
        string methodName = methodCall.Method.Name;

        // Extraer argumentos
        List<string> arguments = new();
        foreach (Expression arg in methodCall.Arguments)
        {
            object val = GetValue(arg);
            arguments.Add(FormatValue(val));
        }

        if (pagina > 0)
        {
            arguments.Add($"Page:{pagina}");
        }

        // Formato: TipoRetorno:Metodo:Arg1:Arg2...
        // Ejemplo: Impuesto:GetImpuestoAsync:1
        return $"{typeName}:{methodName}{(arguments.Any() ? ":" + string.Join(":", arguments) : "")}";
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
            // Tomamos el nombre del tipo sin el `1
            string name = type.Name;
            int tickIndex = name.IndexOf('`');
            if (tickIndex > 0) name = name.Substring(0, tickIndex);

            // Concatenamos argumentos genéricos. Ej: List<Impuesto> -> List<Impuesto> 
            // Ojo: Redis keys no deben tener caracteres muy raros, pero <> suele pasar o se puede simplificar.
            // Usuario prefiere: "Impuesto" para lista? El usuario dijo "collection or list... <repository>PGetImpuestosAsyncNULL"
            // Vamos a intentar devolver el nombre del genérico principal si es una colección, o algo legible.
            // Preferencia personal: "ImpuestoList" o "ListOfImpuesto".
            // Para simplificar y seguir el estilo del usuario "Impuesto", si es una colección de T, podríamos usar T.
            // Pero si retorna List<int>, "Int32" sería confuso. 
            // Dejemoslo como "List<Impuesto>" limpiando.

            return $"{name}<{string.Join(",", genArgs.Select(GetCleanTypeName))}>";
        }

        return type.Name;
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

        // Manejar diccionarios (tipos complejos extraídos)
        if (value is Dictionary<string, object> dict)
        {
            var parts = dict.Select(kvp => $"{kvp.Key}|{FormatValue(kvp.Value)}");
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
        // Tipos simples que no consideramos complejos
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) || type == typeof(Guid) || type.IsEnum)
        {
            return false;
        }

        // Si es una colección, no es un tipo complejo en este contexto
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            return false;
        }

        // Es un tipo complejo (clase personalizada)
        return type.IsClass || type.IsValueType;
    }

    /// <summary>
    /// Extrae las propiedades públicas de un objeto y las devuelve como un diccionario.
    /// </summary>
    private static Dictionary<string, object> ExtractProperties(object obj)
    {
        Dictionary<string, object> properties = new();
        Type type = obj.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                object value = prop.GetValue(obj);
                properties[prop.Name] = value;
            }
            catch
            {
                // Si no se puede leer la propiedad, la ignoramos
                properties[prop.Name] = null;
            }
        }

        return properties;
    }
}
