namespace DataScorpio.Profiles;

using DataScorpio.Querying;

/// <summary>
/// Describes a custom query sort exposed by a profile.
/// </summary>
public sealed class QueryCustomSortDefinition
{
    private readonly Func<IQueryable, SortDirection, IQueryable> apply;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCustomSortDefinition"/> class.
    /// </summary>
    /// <param name="name">The public sort name.</param>
    /// <param name="apply">The sort implementation.</param>
    public QueryCustomSortDefinition(string name, Func<IQueryable, SortDirection, IQueryable> apply)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
    }

    /// <summary>
    /// Gets the public sort name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Applies the custom sort.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="direction">The requested sort direction.</param>
    /// <returns>The sorted query.</returns>
    public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, SortDirection direction)
        => (IQueryable<TEntity>)apply(source, direction);
}
