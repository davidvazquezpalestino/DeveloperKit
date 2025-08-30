using System.Linq.Expressions;

namespace DevKit.ExecutionEngine.PostgreSQL.QueryBuilder
{
    /// <summary>
    /// Visits expression trees and generates PostgreSQL SQL fragments.
    /// </summary>
    public class PostgreSqlExpressionVisitor : ExpressionVisitor
    {
        private readonly Stack<string> _stack = new Stack<string>();
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly string _tableAlias;
        private int _parameterIndex;

        /// <summary>
        /// Gets the parameters collected during expression visiting.
        /// </summary>
        public IReadOnlyDictionary<string, object> Parameters => _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlExpressionVisitor"/> class.
        /// </summary>
        /// <param name="tableAlias">Optional table alias to use for column references.</param>
        public PostgreSqlExpressionVisitor(string tableAlias = "")
        {
            _tableAlias = tableAlias;
        }

        /// <summary>
        /// Gets the SQL fragment resulting from visiting the expression.
        /// </summary>
        public string GetResult()
        {
            if (_stack.Count == 0)
            {
                return string.Empty;
            }

            return _stack.Pop();
        }

        /// <summary>
        /// Visits a member access expression.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is ParameterExpression)
            {
                // This is a property access like x => x.Property
                string columnName = $"\"{node.Member.Name}\"";
                if (!string.IsNullOrEmpty(_tableAlias))
                {
                    columnName = $"\"{_tableAlias}\".{columnName}";
                }
                _stack.Push(columnName);
            }
            else if (node.Expression is ConstantExpression constantExpression)
            {
                // This handles captured variables in the closure
                object container = constantExpression.Value;
                MemberInfo member = node.Member;
                object value = GetMemberValue(container, member);
                string paramName = $"@p{_parameterIndex++}";
                _parameters[paramName] = value ?? DBNull.Value;
                _stack.Push(paramName);
            }
            else
            {
                // For nested properties, evaluate them
                Expression expression = Visit(node.Expression);
                object value = Expression.Lambda(Expression.PropertyOrField(expression, node.Member.Name)).Compile().DynamicInvoke();
                string paramName = $"@p{_parameterIndex++}";
                _parameters[paramName] = value ?? DBNull.Value;
                _stack.Push(paramName);
            }

            return node;
        }

        /// <summary>
        /// Visits a constant expression.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                _stack.Push("NULL");
            }
            else if (node.Value is string || node.Value is DateTime || node.Value is DateTime?)
            {
                string paramName = $"@p{_parameterIndex++}";
                _parameters[paramName] = node.Value;
                _stack.Push(paramName);
            }
            else if (node.Value.GetType().IsValueType)
            {
                string paramName = $"@p{_parameterIndex++}";
                _parameters[paramName] = node.Value;
                _stack.Push(paramName);
            }
            else
            {
                _stack.Push(node.Value.ToString());
            }

            return node;
        }

        /// <summary>
        /// Visits a binary expression.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);
            Visit(node.Right);

            string right = _stack.Pop();
            string left = _stack.Pop();

            string op;
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    op = right == "NULL" || right == "NOT NULL" ? "IS" : "=";
                    break;
                case ExpressionType.NotEqual:
                    op = right == "NULL" || right == "NOT NULL" ? "IS NOT" : "<>";
                    break;
                case ExpressionType.GreaterThan:
                    op = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    op = ">=";
                    break;
                case ExpressionType.LessThan:
                    op = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    op = "<=";
                    break;
                case ExpressionType.AndAlso:
                    op = "AND";
                    break;
                case ExpressionType.OrElse:
                    op = "OR";
                    break;
                case ExpressionType.Add:
                    op = "+";
                    break;
                case ExpressionType.Subtract:
                    op = "-";
                    break;
                case ExpressionType.Multiply:
                    op = "*";
                    break;
                case ExpressionType.Divide:
                    op = "/";
                    break;
                case ExpressionType.Modulo:
                    op = "%";
                    break;
                default:
                    throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported");
            }

            _stack.Push($"({left} {op} {right})");
            return node;
        }

        /// <summary>
        /// Visits a method call expression.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Contains")
            {
                if (node.Object is { NodeType: ExpressionType.MemberAccess })
                {
                    // Handle string.Contains
                    Visit(node.Object);
                    Visit(node.Arguments[0]);
                    string str = _stack.Pop();
                    _stack.Push($"({str} LIKE '%' || {_stack.Pop()} || '%')");
                }
                else if (node.Method.IsStatic && node.Method.DeclaringType == typeof(string) && node.Method.Name == "Contains")
                {
                    // Handle string.Contains static method
                    Visit(node.Arguments[0]);
                    string value = _stack.Pop();
                    Visit(node.Arguments[1]);
                    string str = _stack.Pop();
                    _stack.Push($"({str} LIKE '%' || {value} || '%')");
                }
                else if (node.Method.IsGenericMethod && node.Method.GetGenericMethodDefinition().Name == "Contains")
                {
                    // Handle Enumerable.Contains
                    object collection = Expression.Lambda(node.Arguments[0]).Compile().DynamicInvoke();
                    List<string> values = new List<string>();

                    if (collection != null)
                        foreach (object item in (System.Collections.IEnumerable)collection)
                        {
                            string paramName = $"@p{_parameterIndex++}";
                            _parameters[paramName] = item;
                            values.Add(paramName);
                        }

                    Visit(node.Arguments[1]);
                    string column = _stack.Pop();
                    _stack.Push($"{column} IN ({string.Join(", ", values)})");
                }
            }
            else if (node.Method.Name == "StartsWith")
            {
                Visit(node.Object);
                Visit(node.Arguments[0]);
                string str = _stack.Pop();
                _stack.Push($"({str} LIKE {_stack.Pop()} || '%')");
            }
            else if (node.Method.Name == "EndsWith")
            {
                Visit(node.Object);
                Visit(node.Arguments[0]);
                string str = _stack.Pop();
                _stack.Push($"({str} LIKE '%' || {_stack.Pop()})");
            }
            else if (node.Method.Name == "ToUpper")
            {
                Visit(node.Object);
                _stack.Push($"UPPER({_stack.Pop()})");
            }
            else if (node.Method.Name == "ToLower")
            {
                Visit(node.Object);
                _stack.Push($"LOWER({_stack.Pop()})");
            }
            else if (node.Method.Name == "Trim")
            {
                Visit(node.Object);
                _stack.Push($"TRIM({_stack.Pop()})");
            }
            else
            {
                throw new NotSupportedException($"The method '{node.Method.Name}' is not supported");
            }

            return node;
        }

        /// <summary>
        /// Visits a unary expression.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
            {
                Visit(node.Operand);
                _stack.Push($"NOT ({_stack.Pop()})");
                return node;
            }

            if (node.NodeType == ExpressionType.Convert)
            {
                Visit(node.Operand);
                // For now, we'll just pass through the conversion
                return node;
            }

            return base.VisitUnary(node);
        }

        private static object GetMemberValue(object container, System.Reflection.MemberInfo member)
        {
            if (member is System.Reflection.PropertyInfo property)
            {
                return property.GetValue(container, null);
            }

            if (member is System.Reflection.FieldInfo field)
            {
                return field.GetValue(container);
            }

            throw new NotSupportedException($"Member type {member.MemberType} is not supported");
        }
    }
}
