namespace DataScorpio.Profiles;

using System.Linq.Expressions;
using DataScorpio.Querying;

/// <summary>
/// Builds a query profile registry.
/// </summary>
public sealed class QueryProfileRegistryBuilder
{
    private readonly List<QueryProfileDefinition> profiles = [];
    private readonly List<IQueryProfileConvention> conventions = [];

    /// <summary>
    /// Adds a profile by type.
    /// </summary>
    /// <typeparam name="TProfile">The profile type.</typeparam>
    /// <returns>The same builder.</returns>
    public QueryProfileRegistryBuilder AddProfile<TProfile>()
        where TProfile : IQueryProfile, new()
        => AddProfile(new TProfile().BuildDefinition());

    /// <summary>
    /// Adds a profile instance.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="profile">The profile instance.</param>
    /// <returns>The same builder.</returns>
    public QueryProfileRegistryBuilder AddProfile<TEntity>(QueryProfile<TEntity> profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        profiles.Add(profile.BuildDefinition());

        return this;
    }

    /// <summary>
    /// Adds a profile definition.
    /// </summary>
    /// <param name="profile">The profile definition.</param>
    /// <returns>The same builder.</returns>
    public QueryProfileRegistryBuilder AddProfile(QueryProfileDefinition profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        profiles.Add(profile);

        return this;
    }

    /// <summary>
    /// Adds a reusable custom filter for every profile whose entity implements or inherits a contract.
    /// </summary>
    /// <typeparam name="TContract">The base class or interface contract.</typeparam>
    /// <param name="name">The public filter name.</param>
    /// <param name="predicate">Builds the contract predicate from the query value.</param>
    /// <returns>The same builder.</returns>
    public QueryProfileRegistryBuilder CustomFilter<TContract>(
        string name,
        Func<QueryValue, Expression<Func<TContract, bool>>> predicate)
    {
        conventions.Add(new QueryContractCustomFilterConvention<TContract>(NormalizeName(name), predicate));

        return this;
    }

    /// <summary>
    /// Adds a reusable custom sort for every profile whose entity implements or inherits a contract.
    /// </summary>
    /// <typeparam name="TContract">The base class or interface contract.</typeparam>
    /// <param name="name">The public sort name.</param>
    /// <param name="keySelector">The contract key selector.</param>
    /// <returns>The same builder.</returns>
    public QueryProfileRegistryBuilder CustomSort<TContract>(
        string name,
        Expression<Func<TContract, object>> keySelector)
    {
        conventions.Add(new QueryContractCustomSortConvention<TContract>(NormalizeName(name), keySelector));

        return this;
    }

    /// <summary>
    /// Builds the registry.
    /// </summary>
    /// <returns>The profile registry.</returns>
    public QueryProfileRegistry Build()
        => new(profiles.Select(ApplyConventions));

    private QueryProfileDefinition ApplyConventions(QueryProfileDefinition profile)
    {
        if (conventions.Count == 0)
            return profile;

        var customFilters = new Dictionary<string, QueryCustomFilterDefinition>(StringComparer.OrdinalIgnoreCase);
        var customSorts = new Dictionary<string, QueryCustomSortDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var convention in conventions.Where(convention => convention.CanApply(profile.EntityType)))
        {
            var filter = convention.CreateFilter(profile.EntityType);

            if (filter != null)
                customFilters[filter.Name] = filter;

            var sort = convention.CreateSort(profile.EntityType);

            if (sort != null)
                customSorts[sort.Name] = sort;
        }

        foreach (var filter in profile.CustomFilters.Values)
            customFilters[filter.Name] = filter;

        foreach (var sort in profile.CustomSorts.Values)
            customSorts[sort.Name] = sort;

        return new QueryProfileDefinition(
            profile.EntityType,
            profile.Fields,
            profile.Includes,
            customFilters,
            customSorts,
            profile.DefaultSort,
            profile.MaxPageSize);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Query field name cannot be empty.", nameof(name));

        return name.Trim();
    }
}
