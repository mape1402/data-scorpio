namespace DataScorpio.Profiles;

using System.Linq.Expressions;

internal sealed class QueryMemberPath
{
    private QueryMemberPath(string path, string leafName, Type type)
    {
        Path = path;
        LeafName = leafName;
        Type = type;
    }

    public string Path { get; }

    public string LeafName { get; }

    public Type Type { get; }

    public static QueryMemberPath From<TEntity>(Expression<Func<TEntity, object>> field)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var expression = Unwrap(field.Body);
        var members = new Stack<string>();
        var current = expression;

        while (current is MemberExpression member)
        {
            members.Push(member.Member.Name);
            current = member.Expression;
        }

        if (members.Count == 0 || current is not ParameterExpression)
            throw new ArgumentException("Query fields must be simple member access expressions.", nameof(field));

        var pathMembers = members.ToArray();

        return new QueryMemberPath(
            string.Join(".", pathMembers),
            pathMembers[^1],
            expression.Type);
    }

    private static Expression Unwrap(Expression expression)
    {
        while (expression is UnaryExpression unary &&
               (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
        {
            expression = unary.Operand;
        }

        return expression;
    }
}
