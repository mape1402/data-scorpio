namespace DataScorpio.Profiles;

using System.Linq.Expressions;
using DataScorpio.Querying;

internal interface IQueryProfileConvention
{
    bool CanApply(Type entityType);

    QueryCustomFilterDefinition CreateFilter(Type entityType);

    QueryCustomSortDefinition CreateSort(Type entityType);
}

internal sealed class QueryContractCustomFilterConvention<TContract> : IQueryProfileConvention
{
    private readonly string name;
    private readonly Func<QueryValue, Expression<Func<TContract, bool>>> predicate;

    public QueryContractCustomFilterConvention(
        string name,
        Func<QueryValue, Expression<Func<TContract, bool>>> predicate)
    {
        this.name = name ?? throw new ArgumentNullException(nameof(name));
        this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    public bool CanApply(Type entityType)
        => typeof(TContract).IsAssignableFrom(entityType);

    public QueryCustomFilterDefinition CreateFilter(Type entityType)
    {
        if (!CanApply(entityType))
            return null;

        var method = GetType()
            .GetMethod(nameof(CreateTypedFilter), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .MakeGenericMethod(entityType);

        return (QueryCustomFilterDefinition)method.Invoke(this, null);
    }

    public QueryCustomSortDefinition CreateSort(Type entityType)
        => null;

    private QueryCustomFilterDefinition CreateTypedFilter<TEntity>()
        => new(name, (source, filter) =>
        {
            var query = (IQueryable<TEntity>)source;
            var expression = QueryContractExpressionAdapter.Adapt<TContract, TEntity>(predicate(filter.Value));

            return query.Where(expression);
        });
}

internal sealed class QueryContractCustomSortConvention<TContract> : IQueryProfileConvention
{
    private readonly string name;
    private readonly Expression<Func<TContract, object>> keySelector;

    public QueryContractCustomSortConvention(string name, Expression<Func<TContract, object>> keySelector)
    {
        this.name = name ?? throw new ArgumentNullException(nameof(name));
        this.keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
    }

    public bool CanApply(Type entityType)
        => typeof(TContract).IsAssignableFrom(entityType);

    public QueryCustomFilterDefinition CreateFilter(Type entityType)
        => null;

    public QueryCustomSortDefinition CreateSort(Type entityType)
    {
        if (!CanApply(entityType))
            return null;

        var method = GetType()
            .GetMethod(nameof(CreateTypedSort), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .MakeGenericMethod(entityType);

        return (QueryCustomSortDefinition)method.Invoke(this, null);
    }

    private QueryCustomSortDefinition CreateTypedSort<TEntity>()
    {
        var adaptedKeySelector = QueryContractExpressionAdapter.Adapt<TContract, TEntity>(keySelector);

        return new QueryCustomSortDefinition(
            name,
            (source, direction) => QueryProfileBuilder<TEntity>.ApplyContractSort(
                (IQueryable<TEntity>)source,
                adaptedKeySelector,
                direction));
    }
}

