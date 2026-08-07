namespace DataScorpio.Querying;

/// <summary>
/// Represents transport-neutral query input before parsing and validation.
/// </summary>
public sealed class QueryRequest
{
    /// <summary>
    /// Gets or initializes the requested page number.
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Gets or initializes the requested page size.
    /// </summary>
    public int? PageSize { get; init; }

    /// <summary>
    /// Gets or initializes the raw filter string.
    /// </summary>
    public string Filters { get; init; }

    /// <summary>
    /// Gets or initializes the raw sort string.
    /// </summary>
    public string Sorts { get; init; }

    /// <summary>
    /// Gets or initializes the raw search term.
    /// </summary>
    public string Search { get; init; }
}
