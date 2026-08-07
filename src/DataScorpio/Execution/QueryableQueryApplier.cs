namespace DataScorpio.Execution;

using System.Globalization;
using System.Linq.Expressions;
using DataScorpio.Parsing.Sieve;
using DataScorpio.Profiles;
using DataScorpio.Querying;

/// <summary>
/// Applies query descriptors through provider-friendly expression trees.
/// </summary>
public sealed class QueryableQueryApplier : IQueryableQueryApplier
{
    /// <inheritdoc/>
    public IQueryable<TEntity> Apply<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        source = ApplyFilters(source, descriptor, profile);
        source = ApplySorts(source, descriptor, profile);
        source = ApplyPaging(source, descriptor);

        return source;
    }

    private static IQueryable<TEntity> ApplyFilters<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile)
    {
        foreach (var group in descriptor.FilterGroups)
        {
            if (group.Filters.Count == 0)
                continue;

            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            Expression body = null;

            foreach (var filter in group.Filters)
            {
                var field = profile.FindField(filter.Field);

                if (field == null)
                    throw new InvalidOperationException($"Field '{filter.Field}' is not configured.");

                var filterBody = BuildFilter(parameter, field, filter);

                body = body == null
                    ? filterBody
                    : group.LogicalOperator == QueryLogicalOperator.Or
                        ? Expression.OrElse(body, filterBody)
                        : Expression.AndAlso(body, filterBody);
            }

            if (body == null)
                continue;

            source = source.Where(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
        }

        return source;
    }

    private static IQueryable<TEntity> ApplySorts<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile)
    {
        var sorts = descriptor.Sorts.Count > 0
            ? descriptor.Sorts
            : profile.DefaultSort == null
                ? Array.Empty<SortDescriptor>()
                :
                [
                    new SortDescriptor
                    {
                        Field = profile.DefaultSort.FieldName,
                        Direction = profile.DefaultSort.Direction
                    }
                ];

        var ordered = false;

        foreach (var sort in sorts)
        {
            var field = profile.FindField(sort.Field);

            if (field == null)
                throw new InvalidOperationException($"Field '{sort.Field}' is not configured.");

            source = ApplySort(source, field, sort.Direction, ordered);
            ordered = true;
        }

        return source;
    }

    private static IQueryable<TEntity> ApplyPaging<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor)
    {
        if (!descriptor.Page.IsPaged)
            return source;

        var pageNumber = descriptor.Page.PageNumber.Value;
        var pageSize = descriptor.Page.PageSize.Value;

        if (pageNumber <= 0 || pageSize <= 0)
            return source;

        return source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    private static Expression BuildFilter(
        ParameterExpression parameter,
        QueryFieldDefinition field,
        FilterDescriptor filter)
    {
        var member = BuildMemberAccess(parameter, field.MemberPath);
        var operatorName = filter.Operator;

        return operatorName switch
        {
            SieveOperatorNames.Equal => Compare(member, filter.Value, Expression.Equal),
            SieveOperatorNames.EqualInsensitive => StringCompare(member, filter.Value, SieveOperatorNames.EqualInsensitive),
            SieveOperatorNames.NotEquals => Compare(member, filter.Value, Expression.NotEqual),
            SieveOperatorNames.NotEqualsInsensitive => StringCompare(member, filter.Value, SieveOperatorNames.NotEqualsInsensitive),
            SieveOperatorNames.GreaterThan => Compare(member, filter.Value, Expression.GreaterThan),
            SieveOperatorNames.GreaterThanOrEqual => Compare(member, filter.Value, Expression.GreaterThanOrEqual),
            SieveOperatorNames.LessThan => Compare(member, filter.Value, Expression.LessThan),
            SieveOperatorNames.LessThanOrEqual => Compare(member, filter.Value, Expression.LessThanOrEqual),
            SieveOperatorNames.Contains => StringCompare(member, filter.Value, SieveOperatorNames.Contains),
            SieveOperatorNames.ContainsInsensitive => StringCompare(member, filter.Value, SieveOperatorNames.ContainsInsensitive),
            SieveOperatorNames.NotContains => Expression.Not(StringCompare(member, filter.Value, SieveOperatorNames.Contains)),
            SieveOperatorNames.NotContainsInsensitive => Expression.Not(StringCompare(member, filter.Value, SieveOperatorNames.ContainsInsensitive)),
            SieveOperatorNames.StartsWith => StringCompare(member, filter.Value, SieveOperatorNames.StartsWith),
            SieveOperatorNames.StartsWithInsensitive => StringCompare(member, filter.Value, SieveOperatorNames.StartsWithInsensitive),
            SieveOperatorNames.NotStartsWith => Expression.Not(StringCompare(member, filter.Value, SieveOperatorNames.StartsWith)),
            SieveOperatorNames.NotStartsWithInsensitive => Expression.Not(StringCompare(member, filter.Value, SieveOperatorNames.StartsWithInsensitive)),
            SieveOperatorNames.EndsWith => StringCompare(member, filter.Value, SieveOperatorNames.EndsWith),
            SieveOperatorNames.EndsWithInsensitive => StringCompare(member, filter.Value, SieveOperatorNames.EndsWithInsensitive),
            SieveOperatorNames.NotEndsWith => Expression.Not(StringCompare(member, filter.Value, SieveOperatorNames.EndsWith)),
            SieveOperatorNames.NotEndsWithInsensitive => Expression.Not(StringCompare(member, filter.Value, SieveOperatorNames.EndsWithInsensitive)),
            _ => throw new NotSupportedException($"Query operator '{operatorName}' is not supported.")
        };
    }

    private static Expression Compare(
        Expression member,
        QueryValue value,
        Func<Expression, Expression, BinaryExpression> comparison)
    {
        var constant = BuildConstant(member.Type, value);
        return comparison(member, constant);
    }

    private static Expression StringCompare(Expression member, QueryValue value, string operatorName)
    {
        if (member.Type != typeof(string))
            throw new NotSupportedException($"Operator '{operatorName}' requires a string field.");

        if (value.IsNull || value.IsMissing)
            throw new NotSupportedException($"Operator '{operatorName}' requires a non-null value.");

        var text = Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty;
        Expression target = member;
        Expression constant = Expression.Constant(text, typeof(string));

        if (operatorName.EndsWith("Insensitive", StringComparison.Ordinal))
        {
            target = Expression.Call(member, typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);
            constant = Expression.Constant(text.ToLower(CultureInfo.InvariantCulture), typeof(string));
        }

        if (operatorName == SieveOperatorNames.EqualInsensitive ||
            operatorName == SieveOperatorNames.NotEqualsInsensitive)
        {
            var equals = Expression.Equal(target, constant);
            return operatorName == SieveOperatorNames.NotEqualsInsensitive
                ? Expression.Not(equals)
                : equals;
        }

        var methodName =
            operatorName == SieveOperatorNames.StartsWith || operatorName == SieveOperatorNames.StartsWithInsensitive
                ? nameof(string.StartsWith)
                : operatorName == SieveOperatorNames.EndsWith || operatorName == SieveOperatorNames.EndsWithInsensitive
                    ? nameof(string.EndsWith)
                    : nameof(string.Contains);

        return Expression.Call(target, typeof(string).GetMethod(methodName, [typeof(string)])!, constant);
    }

    private static ConstantExpression BuildConstant(Type targetType, QueryValue value)
    {
        if (value.IsNull || value.IsMissing)
            return Expression.Constant(null, targetType);

        var nullableType = Nullable.GetUnderlyingType(targetType);
        var conversionType = nullableType ?? targetType;
        object converted;

        if (conversionType.IsEnum)
        {
            converted = Enum.Parse(conversionType, Convert.ToString(value.Value, CultureInfo.InvariantCulture), ignoreCase: true);
        }
        else if (conversionType == typeof(Guid))
        {
            converted = Guid.Parse(Convert.ToString(value.Value, CultureInfo.InvariantCulture));
        }
        else if (conversionType == typeof(DateTime))
        {
            converted = DateTime.Parse(Convert.ToString(value.Value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }
        else if (conversionType == typeof(DateOnly))
        {
            converted = DateOnly.Parse(Convert.ToString(value.Value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }
        else if (conversionType == typeof(TimeOnly))
        {
            converted = TimeOnly.Parse(Convert.ToString(value.Value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }
        else
        {
            converted = Convert.ChangeType(value.Value, conversionType, CultureInfo.InvariantCulture);
        }

        return Expression.Constant(converted, targetType);
    }

    private static Expression BuildMemberAccess(Expression source, string memberPath)
    {
        var current = source;

        foreach (var member in memberPath.Split('.'))
            current = Expression.PropertyOrField(current, member);

        return current;
    }

    private static IQueryable<TEntity> ApplySort<TEntity>(
        IQueryable<TEntity> source,
        QueryFieldDefinition field,
        SortDirection direction,
        bool thenBy)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var body = BuildMemberAccess(parameter, field.MemberPath);
        var lambda = Expression.Lambda(body, parameter);
        var methodName = thenBy
            ? direction == SortDirection.Descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy)
            : direction == SortDirection.Descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var call = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(TEntity), body.Type],
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<TEntity>(call);
    }
}
