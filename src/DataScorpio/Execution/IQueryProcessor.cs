namespace DataScorpio.Execution;

using DataScorpio.Profiles;
using DataScorpio.Querying;

/// <summary>
/// Parses, validates, and executes query requests.
/// </summary>
public interface IQueryProcessor
{
    /// <summary>
    /// Executes a query using a registered profile.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="request">The query request.</param>
    /// <returns>The execution result.</returns>
    QueryExecutionResult<TEntity> Execute<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request);

    /// <summary>
    /// Executes a query using an explicit profile.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="request">The query request.</param>
    /// <param name="profile">The profile definition.</param>
    /// <returns>The execution result.</returns>
    QueryExecutionResult<TEntity> Execute<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request,
        QueryProfileDefinition profile);
}
