namespace DataScorpio.Profiles;

using System.Linq.Expressions;
using System.Reflection;

internal static class QueryContractExpressionAdapter
{
    public static LambdaExpression Adapt<TContract, TEntity>(Expression<Func<TContract, object>> expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var parameter = Expression.Parameter(typeof(TEntity), expression.Parameters[0].Name ?? "entity");
        var body = new ContractParameterVisitor(expression.Parameters[0], parameter).Visit(expression.Body);

        return Expression.Lambda(body, parameter);
    }

    public static Expression<Func<TEntity, bool>> Adapt<TContract, TEntity>(Expression<Func<TContract, bool>> expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var parameter = Expression.Parameter(typeof(TEntity), expression.Parameters[0].Name ?? "entity");
        var body = new ContractParameterVisitor(expression.Parameters[0], parameter).Visit(expression.Body);

        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    private sealed class ContractParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression contractParameter;
        private readonly ParameterExpression entityParameter;

        public ContractParameterVisitor(ParameterExpression contractParameter, ParameterExpression entityParameter)
        {
            this.contractParameter = contractParameter;
            this.entityParameter = entityParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == contractParameter ? entityParameter : base.VisitParameter(node);

        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);

            if (expression != null &&
                node.Member.DeclaringType != null &&
                node.Member.DeclaringType != expression.Type &&
                node.Member.DeclaringType.IsAssignableFrom(expression.Type))
            {
                return Expression.MakeMemberAccess(expression, FindConcreteMember(expression.Type, node.Member));
            }

            return node.Update(expression);
        }

        private static MemberInfo FindConcreteMember(Type entityType, MemberInfo contractMember)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            return contractMember.MemberType switch
            {
                MemberTypes.Property => entityType.GetProperty(contractMember.Name, flags)
                    ?? throw new InvalidOperationException($"Type '{entityType.Name}' does not expose property '{contractMember.Name}'."),
                MemberTypes.Field => entityType.GetField(contractMember.Name, flags)
                    ?? throw new InvalidOperationException($"Type '{entityType.Name}' does not expose field '{contractMember.Name}'."),
                _ => throw new NotSupportedException($"Member '{contractMember.Name}' is not supported in contract query expressions.")
            };
        }
    }
}

