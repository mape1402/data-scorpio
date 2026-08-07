namespace DataScorpio.Querying;

/// <summary>
/// Represents query results with DataScorpio and TurtlePath-compatible paging metadata.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class QueryResult<T>
{
    /// <summary>
    /// Gets or initializes the result items.
    /// </summary>
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Gets or initializes the result items using TurtlePath's property name.
    /// </summary>
    public IEnumerable<T> Results
    {
        get => Items;
        init => Items = value?.ToArray() ?? Array.Empty<T>();
    }

    /// <summary>
    /// Gets or initializes the current page number.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets or initializes the current page number using TurtlePath's property name.
    /// </summary>
    public int CurrentPage
    {
        get => PageNumber;
        init => PageNumber = value;
    }

    /// <summary>
    /// Gets or initializes the page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets or initializes the total row count.
    /// </summary>
    public long RowCount { get; init; }

    /// <summary>
    /// Gets or initializes the total row count using DataScorpio's property name.
    /// </summary>
    public long TotalRows
    {
        get => RowCount;
        init => RowCount = value;
    }

    /// <summary>
    /// Gets or initializes the total page count.
    /// </summary>
    public int PageCount { get; init; }

    /// <summary>
    /// Gets or initializes the total page count using DataScorpio's property name.
    /// </summary>
    public int TotalPages
    {
        get => PageCount;
        init => PageCount = value;
    }

    /// <summary>
    /// Gets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage => PageNumber > 0 && PageNumber < PageCount;
}
