namespace DataScorpio.Testing;

using DataScorpio.Execution;
using DataScorpio.Parsing.Sieve;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Validation;

/// <summary>
/// In-memory host for testing DataScorpio query profiles and request strings.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed class QueryTestHost<TEntity>
{
    private readonly QueryProfileDefinition profile;
    private readonly List<TEntity> seed = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryTestHost{TEntity}"/> class.
    /// </summary>
    /// <param name="profile">The query profile.</param>
    public QueryTestHost(QueryProfile<TEntity> profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        this.profile = profile.BuildDefinition();
    }

    /// <summary>
    /// Adds seed values.
    /// </summary>
    /// <param name="items">The seed values.</param>
    /// <returns>The same host.</returns>
    public QueryTestHost<TEntity> WithSeed(params TEntity[] items)
    {
        if (items != null)
            seed.AddRange(items);

        return this;
    }

    /// <summary>
    /// Executes a Sieve-compatible query against the seed values.
    /// </summary>
    /// <param name="filters">The filters string.</param>
    /// <param name="sorts">The sorts string.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">The search term.</param>
    /// <returns>The execution result.</returns>
    public QueryExecutionResult<TEntity> Apply(
        string filters = null,
        string sorts = null,
        int? pageNumber = null,
        int? pageSize = null,
        string search = null)
    {
        var processor = new QueryProcessor(
            new SieveQueryParser(),
            new QueryDescriptorValidator(),
            new QueryableQueryApplier(),
            new QueryProfileRegistryBuilder().AddProfile(profile).Build());

        return processor.Execute(seed.AsQueryable(), new QueryRequest
        {
            Filters = filters,
            Sorts = sorts,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search
        }, profile);
    }
}
