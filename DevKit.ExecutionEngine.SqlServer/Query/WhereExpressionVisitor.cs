namespace DevKit.ExecutionEngine.SQLServer.Query;

/// <summary>
/// Visits and processes expressions in a LINQ query to generate SQL WHERE clauses.
/// </summary>
public class WhereExpressionVisitor(Dictionary<string, object> parametersField, ref int paramIndex) : ExpressionVisitor
{
    private int ParamIndex = paramIndex;

    /// <summary>
    /// Obtiene el diccionario de parámetros generados durante la construcción de la consulta.
    /// </summary>
    /// <value>Diccionario que asocia nombres de parámetros con sus valores</value>
    public Dictionary<string, object> Parameters => parametersField;

    /// <summary>
    /// Genera un nombre de parámetro único para consultas SQL en el formato @p{n} donde n es un número secuencial.
    /// </summary>
    /// <returns>Un nombre de parámetro único en formato @p{n}</returns>
    public string GetNextParameterName() => $"@p{ParamIndex++}";

    /// <summary>
    /// Visits the children of the <see cref="BinaryExpression"/>
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        Expression left = Visit(node.Left);
        Expression right = Visit(node.Right);

        if (node.NodeType == ExpressionType.Equal)
        {
            if (right is ConstantExpression { Value: null })
            {
                return Expression.MakeBinary(ExpressionType.Equal, left, right, false, null);
            }

            // Manejar parámetros para igualdades
            if (right is ConstantExpression constant)
            {
                string paramName = GetNextParameterName();
                Parameters[paramName] = constant.Value;
                return Expression.MakeBinary(
                    ExpressionType.Equal,
                    left,
                    Expression.Constant(paramName),
                    false,
                    null);
            }
        }

        return node.Update(left, node.Conversion, right);
    }

    /// <summary>
    /// Visits the children of the <see cref="MemberExpression"/>
    /// </summary>
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is ParameterExpression)
        {
            return node; // Return the member expression as-is for parameter access
        }

        // For static members or other complex expressions, evaluate them
        try
        {
            object constant = Expression.Lambda<Func<object>>(node).Compile()();
            return Expression.Constant(constant, node.Type);
        }
        catch
        {
            // If we can't evaluate, try to visit the expression first
            Expression visited = Visit(node.Expression);
            if (visited != node.Expression)
            {
                return Expression.MakeMemberAccess(visited, node.Member);
            }
            throw;
        }
    }

    /// <summary>
    /// Visits the <see cref="ParameterExpression"/>
    /// </summary>
    protected override Expression VisitParameter(ParameterExpression node) => node;

    /// <summary>
    /// Visits the <see cref="ConstantExpression"/>
    /// </summary>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        // For null values, return as-is for proper null handling
        if (node.Value == null)
        {
            return node;
        }

        string paramName = $"@p{ParamIndex++}";
        Parameters[paramName] = node.Value;
        return Expression.Parameter(node.Type, paramName);
    }
}