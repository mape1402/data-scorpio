namespace DataScorpio.Querying;

/// <summary>
/// Represents one sort operation against a configured query field.
/// </summary>
public sealed class SortDescriptor
{
    /// <summary>
    /// Gets or initializes the public query field name or alias.
    /// </summary>
    public string Field { get; init; }

    /// <summary>
    /// Gets or initializes the sort direction.
    /// </summary>
    public SortDirection Direction { get; init; } = SortDirection.Ascending;
}
