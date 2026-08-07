namespace DataScorpio.Querying;

/// <summary>
/// Defines how child query descriptors are combined.
/// </summary>
public enum QueryLogicalOperator
{
    /// <summary>
    /// Combines child descriptors with logical AND.
    /// </summary>
    And = 0,

    /// <summary>
    /// Combines child descriptors with logical OR.
    /// </summary>
    Or = 1
}
