namespace DataScorpio.Testing;

using DataScorpio.Execution;

/// <summary>
/// Executes Sieve-compatible query strings in tests.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface ISieveTesting<TEntity>
{
    /// <summary>
    /// Seeds the in-memory test dataset.
    /// </summary>
    /// <param name="items">The seed items.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    Task SeedAsync(IEnumerable<TEntity> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies Sieve-compatible filters, sorts, search, and paging to the seeded dataset.
    /// </summary>
    /// <param name="filters">The filters string.</param>
    /// <param name="sorts">The sorts string.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">The search term.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query result.</returns>
    Task<QueryExecutionResult<TEntity>> ApplyAsync(
        string filters = null,
        string sorts = null,
        int? pageNumber = null,
        int? pageSize = null,
        string search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies Sieve-compatible filters, sorts, search, and paging to a query.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="filters">The filters string.</param>
    /// <param name="sorts">The sorts string.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">The search term.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query result.</returns>
    Task<QueryExecutionResult<TEntity>> ApplyAsync(
        IQueryable<TEntity> query,
        string filters = null,
        string sorts = null,
        int? pageNumber = null,
        int? pageSize = null,
        string search = null,
        CancellationToken cancellationToken = default);
}

