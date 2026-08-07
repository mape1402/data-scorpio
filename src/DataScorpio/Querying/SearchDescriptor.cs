namespace DataScorpio.Querying;

/// <summary>
/// Represents a search request over configured searchable fields.
/// </summary>
public sealed class SearchDescriptor
{
    /// <summary>
    /// Gets or initializes the search term.
    /// </summary>
    public string Term { get; init; }

    /// <summary>
    /// Gets or initializes the fields that should be searched.
    /// </summary>
    public IReadOnlyList<string> Fields { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets an empty search descriptor.
    /// </summary>
    public static SearchDescriptor Empty { get; } = new();
}
