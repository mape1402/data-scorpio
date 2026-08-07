namespace DataScorpio.EntityFrameworkCore.Execution;

using DataScorpio.Execution;
using DataScorpio.Profiles;
using DataScorpio.Querying;

/// <summary>
/// Executes DataScorpio queries using Entity Framework Core async APIs.
/// </summary>
public interface IEfCoreQueryProcessor
{
    /// <summary>
    /// Executes a query using a registered profile.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The execution result.</returns>
    Task<QueryExecutionResult<TEntity>> ExecuteAsync<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Executes a parsed query descriptor using a registered profile.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="descriptor">The parsed query descriptor.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The execution result.</returns>
    Task<QueryExecutionResult<TEntity>> ExecuteAsync<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Executes a query using an explicit profile.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="request">The query request.</param>
    /// <param name="profile">The profile definition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The execution result.</returns>
    Task<QueryExecutionResult<TEntity>> ExecuteAsync<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request,
        QueryProfileDefinition profile,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Executes a parsed query descriptor using an explicit profile.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="descriptor">The parsed query descriptor.</param>
    /// <param name="profile">The profile definition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The execution result.</returns>
    Task<QueryExecutionResult<TEntity>> ExecuteAsync<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile,
        CancellationToken cancellationToken = default)
        where TEntity : class;
}
