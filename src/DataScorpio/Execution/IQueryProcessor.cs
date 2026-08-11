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
    /// Executes a parsed query descriptor using a registered profile.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="descriptor">The parsed query descriptor.</param>
    /// <returns>The execution result.</returns>
    QueryExecutionResult<TEntity> Execute<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor);

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

    /// <summary>
    /// Executes a parsed query descriptor using an explicit profile.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="descriptor">The parsed query descriptor.</param>
    /// <param name="profile">The profile definition.</param>
    /// <returns>The execution result.</returns>
    QueryExecutionResult<TEntity> Execute<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile);

    /// <summary>
    /// Applies criteria from a query request using a registered profile without building a terminal page result.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="request">The query request.</param>
    /// <returns>The criteria application result.</returns>
    QueryCriteriaResult<TEntity> ApplyCriteria<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request);

    /// <summary>
    /// Applies criteria from a parsed descriptor using a registered profile without building a terminal page result.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="descriptor">The parsed query descriptor.</param>
    /// <returns>The criteria application result.</returns>
    QueryCriteriaResult<TEntity> ApplyCriteria<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor);

    /// <summary>
    /// Applies criteria from a query request using an explicit profile without building a terminal page result.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="request">The query request.</param>
    /// <param name="profile">The profile definition.</param>
    /// <returns>The criteria application result.</returns>
    QueryCriteriaResult<TEntity> ApplyCriteria<TEntity>(
        IQueryable<TEntity> source,
        QueryRequest request,
        QueryProfileDefinition profile);

    /// <summary>
    /// Applies criteria from a parsed descriptor using an explicit profile without building a terminal page result.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="descriptor">The parsed query descriptor.</param>
    /// <param name="profile">The profile definition.</param>
    /// <returns>The criteria application result.</returns>
    QueryCriteriaResult<TEntity> ApplyCriteria<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile);
}
