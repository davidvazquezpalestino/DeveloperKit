namespace DevKit.ExecutionEngine.MySql.QueryBuilder
{
    /// <inheritdoc />
    public class MySqlExpressionVisitor : ExpressionVisitor
    {
        private readonly Stack<string> Stack = new Stack<string>();
        private readonly Dictionary<string, object> Parameters = new Dictionary<string, object>();
        private int ParameterIndex;

        public (string WhereClause, Dictionary<string, object> Parameters) GetWhereClause()
        {
            if (Stack.Count == 0)
                return (string.Empty, new Dictionary<string, object>());

            return (Stack.Pop(), new Dictionary<string, object>(Parameters));
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);
            Visit(node.Right);

            string right = Stack.Pop();
            string left = Stack.Pop();
            string op = GetOperator(node.NodeType);

            Stack.Push($"({left} {op} {right})");
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is ParameterExpression)
            {
                Stack.Push($"`{node.Member.Name}`");
            }
            else if (node.Expression is ConstantExpression constantExpression)
            {
                object value = Expression.Lambda(node).Compile().DynamicInvoke();
                AddParameter(value);
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            AddParameter(node.Value);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Contains")
            {
                if (node.Object != null && node.Object.NodeType == ExpressionType.MemberAccess)
                {
                    // Handle string.Contains
                    Visit(node.Object);
                    Visit(node.Arguments[0]);
                    string value = Stack.Pop();
                    string property = Stack.Pop();
                    Stack.Push($"({property} LIKE CONCAT('%', {value}, '%'))");
                }
                else if (node.Method.DeclaringType == typeof(Enumerable) &&
                         node.Arguments.Count > 1)
                {
                    // Handle Enumerable.Contains
                    Visit(node.Arguments[1]);
                    Visit(node.Arguments[0]);
                    string values = Stack.Pop();
                    string property = Stack.Pop();
                    Stack.Push($"{property} IN ({values})");
                }
            }
            else if (node.Method.Name == "StartsWith")
            {
                Visit(node.Object);
                Visit(node.Arguments[0]);
                string value = Stack.Pop();
                string property = Stack.Pop();
                Stack.Push($"({property} LIKE CONCAT({value}, '%'))");
            }
            else if (node.Method.Name == "EndsWith")
            {
                Visit(node.Object);
                Visit(node.Arguments[0]);
                string value = Stack.Pop();
                string property = Stack.Pop();
                Stack.Push($"({property} LIKE CONCAT('%', {value}))");
            }

            return node;
        }

        private void AddParameter(object value)
        {
            string paramName = $"@p{ParameterIndex++}";
            Parameters[paramName] = value;
            Stack.Push(paramName);
        }

        private static string GetOperator(ExpressionType type)
        {
            return type switch
            {
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "!=",
                _ => throw new NotSupportedException($"The operator '{type}' is not supported")
            };
        }
    }
}
