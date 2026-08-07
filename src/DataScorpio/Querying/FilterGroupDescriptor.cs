namespace DataScorpio.Querying;

/// <summary>
/// Represents a group of filters combined with a logical operator.
/// </summary>
public sealed class FilterGroupDescriptor
{
    /// <summary>
    /// Gets or initializes the logical operator used inside the group.
    /// </summary>
    public QueryLogicalOperator LogicalOperator { get; init; } = QueryLogicalOperator.And;

    /// <summary>
    /// Gets or initializes the filters in the group.
    /// </summary>
    public IReadOnlyList<FilterDescriptor> Filters { get; init; } = Array.Empty<FilterDescriptor>();
}
