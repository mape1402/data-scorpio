namespace DataScorpio.Execution;

using DataScorpio.Parsing.Sieve;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Validation;

/// <summary>
/// Provides direct DataScorpio helpers for <see cref="IQueryable{T}"/>.
/// </summary>
public static class DataScorpioQueryableExtensions
{
    /// <summary>
    /// Applies a query request directly to an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="request">The query request.</param>
    /// <param name="profile">The query profile.</param>
    /// <returns>The filtered, searched, sorted, and paged query.</returns>
    public static IQueryable<TEntity> ApplyDataScorpio<TEntity>(
        this IQueryable<TEntity> source,
        QueryRequest request,
        QueryProfile<TEntity> profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        var descriptor = new SieveQueryParser().Parse(request);
        return source.ApplyDataScorpio(descriptor, profile.BuildDefinition());
    }

    /// <summary>
    /// Applies a parsed query descriptor directly to an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="descriptor">The query descriptor.</param>
    /// <param name="profile">The query profile.</param>
    /// <returns>The filtered, searched, sorted, and paged query.</returns>
    public static IQueryable<TEntity> ApplyDataScorpio<TEntity>(
        this IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfile<TEntity> profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        return source.ApplyDataScorpio(descriptor, profile.BuildDefinition());
    }

    /// <summary>
    /// Applies a parsed query descriptor directly to an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="descriptor">The query descriptor.</param>
    /// <param name="profile">The query profile definition.</param>
    /// <returns>The filtered, searched, sorted, and paged query.</returns>
    public static IQueryable<TEntity> ApplyDataScorpio<TEntity>(
        this IQueryable<TEntity> source,
        QueryDescriptor descriptor,
        QueryProfileDefinition profile)
    {
        var validation = new QueryDescriptorValidator().Validate(descriptor, profile);

        if (!validation.IsValid)
            throw new DataScorpioQueryException(validation);

        return new QueryableQueryApplier().Apply(source, descriptor, profile);
    }
}
