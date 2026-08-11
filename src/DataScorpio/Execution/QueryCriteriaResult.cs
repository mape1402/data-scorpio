namespace DataScorpio.Execution;

/// <summary>
/// Represents the result of applying DataScorpio criteria without forcing a terminal query result.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class QueryCriteriaResult<T>
{
    /// <summary>
    /// Gets or initializes the query after criteria were applied.
    /// </summary>
    public IQueryable<T> Query { get; init; }

    /// <summary>
    /// Gets or initializes a value indicating whether the result has been materialized into memory.
    /// </summary>
    public bool IsMaterialized { get; init; }

    /// <summary>
    /// Gets or initializes materialized items when <see cref="IsMaterialized"/> is true.
    /// </summary>
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Gets a value indicating whether the query can be consumed through async enumeration.
    /// </summary>
    public bool SupportsAsyncEnumeration => !IsMaterialized;

    internal static QueryCriteriaResult<T> FromQuery(IQueryable<T> query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        if (query is IAsyncEnumerable<T>)
        {
            return new QueryCriteriaResult<T>
            {
                Query = query,
                IsMaterialized = false
            };
        }

        var items = query.ToArray();

        return new QueryCriteriaResult<T>
        {
            Query = items.AsQueryable(),
            IsMaterialized = true,
            Items = items
        };
    }
}
