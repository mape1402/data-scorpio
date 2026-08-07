namespace DataScorpio.Execution;

using DataScorpio.Profiles;
using DataScorpio.Querying;

/// <summary>
/// Applies parsed query descriptors to <see cref="IQueryable{T}"/> sources.
/// </summary>
public interface IQueryableQueryApplier
{
    /// <summary>
    /// Applies a query descriptor to a source query.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="descriptor">The parsed query descriptor.</param>
    /// <param name="profile">The profile definition.</param>
    /// <returns>The query with descriptor operations applied.</returns>
    IQueryable<TEntity> Apply<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile);
}
