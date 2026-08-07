namespace DataScorpio.Querying;

/// <summary>
/// Represents a parsed query that no longer depends on raw transport strings.
/// </summary>
public sealed class QueryDescriptor
{
    /// <summary>
    /// Gets the filter groups to apply.
    /// </summary>
    public IReadOnlyList<FilterGroupDescriptor> FilterGroups { get; init; } = Array.Empty<FilterGroupDescriptor>();

    /// <summary>
    /// Gets the sort descriptors to apply in order.
    /// </summary>
    public IReadOnlyList<SortDescriptor> Sorts { get; init; } = Array.Empty<SortDescriptor>();

    /// <summary>
    /// Gets the search descriptor.
    /// </summary>
    public SearchDescriptor Search { get; init; } = SearchDescriptor.Empty;

    /// <summary>
    /// Gets the page descriptor.
    /// </summary>
    public PageDescriptor Page { get; init; } = PageDescriptor.Unpaged;

    /// <summary>
    /// Gets the named presets requested by the query.
    /// </summary>
    public IReadOnlyList<QueryPresetDescriptor> Presets { get; init; } = Array.Empty<QueryPresetDescriptor>();

    /// <summary>
    /// Gets an empty descriptor.
    /// </summary>
    public static QueryDescriptor Empty { get; } = new();
}
