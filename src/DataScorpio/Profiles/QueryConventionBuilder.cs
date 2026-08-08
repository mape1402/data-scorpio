namespace DataScorpio.Profiles;

using System.Linq.Expressions;
using DataScorpio.Querying;

internal sealed class QueryConventionBuilder : IQueryConventionBuilder
{
    private readonly List<IQueryProfileConvention> conventions = [];

    public IQueryConventionBuilder CustomFilter<TContract>(
        string name,
        Func<QueryValue, Expression<Func<TContract, bool>>> predicate)
    {
        conventions.Add(new QueryContractCustomFilterConvention<TContract>(NormalizeName(name), predicate));

        return this;
    }

    public IQueryConventionBuilder CustomSort<TContract>(
        string name,
        Expression<Func<TContract, object>> keySelector)
    {
        conventions.Add(new QueryContractCustomSortConvention<TContract>(NormalizeName(name), keySelector));

        return this;
    }

    public IReadOnlyList<IQueryProfileConvention> Build()
        => conventions;

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Query convention name cannot be empty.", nameof(name));

        return name.Trim();
    }
}

