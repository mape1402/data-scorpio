namespace DataScorpio.Profiles;

using DataScorpio.Querying;

/// <summary>
/// Describes a custom query filter exposed by a profile.
/// </summary>
public sealed class QueryCustomFilterDefinition
{
    private readonly Func<IQueryable, FilterDescriptor, IQueryable> apply;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCustomFilterDefinition"/> class.
    /// </summary>
    /// <param name="name">The public filter name.</param>
    /// <param name="apply">The filter implementation.</param>
    public QueryCustomFilterDefinition(string name, Func<IQueryable, FilterDescriptor, IQueryable> apply)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
    }

    /// <summary>
    /// Gets the public filter name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Applies the custom filter.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="filter">The filter descriptor.</param>
    /// <returns>The filtered query.</returns>
    public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, FilterDescriptor filter)
        => (IQueryable<TEntity>)apply(source, filter);
}
