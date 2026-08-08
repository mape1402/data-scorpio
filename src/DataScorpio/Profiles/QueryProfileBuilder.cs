namespace DataScorpio.Profiles;

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using DataScorpio.Querying;

/// <summary>
/// Builds typed query profile metadata.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed class QueryProfileBuilder<TEntity> : IQueryProfileBuilder<TEntity>
{
    private readonly Dictionary<string, QueryFieldDefinition> fields = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, QueryIncludeDefinition> includes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, QueryCustomFilterDefinition> customFilters = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, QueryCustomSortDefinition> customSorts = new(StringComparer.OrdinalIgnoreCase);
    private QuerySortDefinition defaultSort;
    private int? maxPageSize;

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowFilter(Expression<Func<TEntity, object>> field)
        => AllowFilter(GetDefaultName(field), field);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowFilter(string name, Expression<Func<TEntity, object>> field)
        => Allow(name, field, QueryFieldCapabilities.Filter);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowSort(Expression<Func<TEntity, object>> field)
        => AllowSort(GetDefaultName(field), field);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowSort(string name, Expression<Func<TEntity, object>> field)
        => Allow(name, field, QueryFieldCapabilities.Sort);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowSearch(Expression<Func<TEntity, object>> field)
        => AllowSearch(GetDefaultName(field), field);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowSearch(string name, Expression<Func<TEntity, object>> field)
        => Allow(name, field, QueryFieldCapabilities.Search);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowInclude(Expression<Func<TEntity, object>> include)
        => AllowInclude(GetDefaultName(include), include);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> AllowInclude(string name, Expression<Func<TEntity, object>> include)
    {
        if (include == null)
            throw new ArgumentNullException(nameof(include));

        var normalizedName = NormalizeName(name);
        var member = QueryMemberPath.From(include);

        if (includes.TryGetValue(normalizedName, out var existing) &&
            !string.Equals(existing.MemberPath, member.Path, StringComparison.Ordinal))
            throw new InvalidOperationException($"Query include '{normalizedName}' is already mapped to '{existing.MemberPath}'.");

        includes[normalizedName] = new QueryIncludeDefinition(normalizedName, member.Path);

        return this;
    }

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> CustomFilter(
        string name,
        Func<IQueryable<TEntity>, QueryValue, IQueryable<TEntity>> filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        return CustomFilterDescriptor(name, (query, descriptor) => filter(query, descriptor.Value));
    }

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> CustomFilter<TContract>(
        string name,
        Func<QueryValue, Expression<Func<TContract, bool>>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        EnsureContractApplies<TContract>();

        return CustomFilter(name, (query, value) =>
            query.Where(QueryContractExpressionAdapter.Adapt<TContract, TEntity>(predicate(value))));
    }

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> CustomFilterDescriptor(
        string name,
        Func<IQueryable<TEntity>, FilterDescriptor, IQueryable<TEntity>> filter)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var normalizedName = NormalizeName(name);

        customFilters[normalizedName] = new QueryCustomFilterDefinition(
            normalizedName,
            (source, descriptor) => filter((IQueryable<TEntity>)source, descriptor));

        return this;
    }

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> CustomSort(
        string name,
        Func<IQueryable<TEntity>, SortDirection, IQueryable<TEntity>> sort)
    {
        if (sort == null)
            throw new ArgumentNullException(nameof(sort));

        var normalizedName = NormalizeName(name);

        customSorts[normalizedName] = new QueryCustomSortDefinition(
            normalizedName,
            (source, direction) => sort((IQueryable<TEntity>)source, direction));

        return this;
    }

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> CustomSort<TContract>(
        string name,
        Expression<Func<TContract, object>> keySelector)
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));

        EnsureContractApplies<TContract>();

        var adaptedKeySelector = QueryContractExpressionAdapter.Adapt<TContract, TEntity>(keySelector);

        return CustomSort(name, (query, direction) => ApplyContractSort(query, adaptedKeySelector, direction));
    }

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> DefaultSort(
        Expression<Func<TEntity, object>> field,
        SortDirection direction = SortDirection.Ascending)
        => DefaultSort(GetDefaultName(field), field, direction);

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> DefaultSort(
        string name,
        Expression<Func<TEntity, object>> field,
        SortDirection direction = SortDirection.Ascending)
    {
        AllowSort(name, field);
        defaultSort = new QuerySortDefinition(NormalizeName(name), direction);

        return this;
    }

    /// <inheritdoc/>
    public IQueryProfileBuilder<TEntity> MaxPageSize(int maxPageSize)
    {
        if (maxPageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPageSize), "Max page size must be greater than zero.");

        this.maxPageSize = maxPageSize;

        return this;
    }

    /// <summary>
    /// Builds the immutable profile definition.
    /// </summary>
    /// <returns>The profile definition.</returns>
    public QueryProfileDefinition Build()
        => new(
            typeof(TEntity),
            new ReadOnlyDictionary<string, QueryFieldDefinition>(new Dictionary<string, QueryFieldDefinition>(fields, fields.Comparer)),
            new ReadOnlyDictionary<string, QueryIncludeDefinition>(new Dictionary<string, QueryIncludeDefinition>(includes, includes.Comparer)),
            new ReadOnlyDictionary<string, QueryCustomFilterDefinition>(new Dictionary<string, QueryCustomFilterDefinition>(customFilters, customFilters.Comparer)),
            new ReadOnlyDictionary<string, QueryCustomSortDefinition>(new Dictionary<string, QueryCustomSortDefinition>(customSorts, customSorts.Comparer)),
            defaultSort,
            maxPageSize);

    private IQueryProfileBuilder<TEntity> Allow(
        string name,
        Expression<Func<TEntity, object>> field,
        QueryFieldCapabilities capabilities)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var normalizedName = NormalizeName(name);
        var member = QueryMemberPath.From(field);

        if (fields.TryGetValue(normalizedName, out var existing))
        {
            if (!string.Equals(existing.MemberPath, member.Path, StringComparison.Ordinal) || existing.FieldType != member.Type)
                throw new InvalidOperationException($"Query field '{normalizedName}' is already mapped to '{existing.MemberPath}'.");

            fields[normalizedName] = existing.WithCapabilities(capabilities);
            return this;
        }

        fields[normalizedName] = new QueryFieldDefinition(
            normalizedName,
            member.Path,
            member.Type,
            capabilities);

        return this;
    }

    private static string GetDefaultName(Expression<Func<TEntity, object>> field)
        => QueryMemberPath.From(field).LeafName;

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Query field name cannot be empty.", nameof(name));

        return name.Trim();
    }

    private static void EnsureContractApplies<TContract>()
    {
        if (!typeof(TContract).IsAssignableFrom(typeof(TEntity)))
            throw new InvalidOperationException(
                $"Contract '{typeof(TContract).Name}' cannot be used for entity '{typeof(TEntity).Name}'.");
    }

    internal static IQueryable<TEntity> ApplyContractSort(
        IQueryable<TEntity> source,
        LambdaExpression keySelector,
        SortDirection direction)
    {
        var body = UnwrapObjectConversion(keySelector.Body);
        var delegateType = typeof(Func<,>).MakeGenericType(typeof(TEntity), body.Type);
        var typedKeySelector = Expression.Lambda(delegateType, body, keySelector.Parameters);
        var methodName = direction == SortDirection.Descending
            ? nameof(Queryable.OrderByDescending)
            : nameof(Queryable.OrderBy);

        var method = typeof(Queryable)
            .GetMethods()
            .Single(method =>
                method.Name == methodName &&
                method.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TEntity), body.Type);

        return (IQueryable<TEntity>)method.Invoke(null, [source, typedKeySelector]);
    }

    private static Expression UnwrapObjectConversion(Expression expression)
    {
        while (expression is UnaryExpression unary &&
               (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked) &&
               unary.Type == typeof(object))
        {
            expression = unary.Operand;
        }

        return expression;
    }
}
