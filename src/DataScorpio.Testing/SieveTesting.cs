namespace DataScorpio.Testing;

using DataScorpio.Execution;
using DataScorpio.Querying;

internal sealed class SieveTesting<TEntity> : ISieveTesting<TEntity>
{
    private readonly IQueryProcessor processor;
    private readonly List<TEntity> seed = [];

    public SieveTesting(IQueryProcessor processor)
    {
        this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    public Task SeedAsync(IEnumerable<TEntity> items, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        seed.Clear();

        if (items != null)
            seed.AddRange(items);

        return Task.CompletedTask;
    }

    public Task<QueryExecutionResult<TEntity>> ApplyAsync(
        string filters = null,
        string sorts = null,
        int? pageNumber = null,
        int? pageSize = null,
        string search = null,
        CancellationToken cancellationToken = default)
        => ApplyAsync(seed.AsQueryable(), filters, sorts, pageNumber, pageSize, search, cancellationToken);

    public Task<QueryExecutionResult<TEntity>> ApplyAsync(
        IQueryable<TEntity> query,
        string filters = null,
        string sorts = null,
        int? pageNumber = null,
        int? pageSize = null,
        string search = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var result = processor.Execute(query, new QueryRequest
        {
            Filters = filters,
            Sorts = sorts,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search
        });

        return Task.FromResult(result);
    }
}

