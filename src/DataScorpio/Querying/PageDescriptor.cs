namespace DataScorpio.Querying;

/// <summary>
/// Represents paging requested by a query.
/// </summary>
public sealed class PageDescriptor
{
    /// <summary>
    /// Gets or initializes the page number.
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Gets or initializes the page size.
    /// </summary>
    public int? PageSize { get; init; }

    /// <summary>
    /// Gets a value indicating whether paging was requested.
    /// </summary>
    public bool IsPaged => PageNumber.HasValue && PageSize.HasValue;

    /// <summary>
    /// Gets an unpaged descriptor.
    /// </summary>
    public static PageDescriptor Unpaged { get; } = new();
}
