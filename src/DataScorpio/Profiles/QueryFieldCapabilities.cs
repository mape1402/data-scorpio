namespace DataScorpio.Profiles;

/// <summary>
/// Defines the query operations allowed for a configured field.
/// </summary>
[Flags]
public enum QueryFieldCapabilities
{
    /// <summary>
    /// No query operation is allowed.
    /// </summary>
    None = 0,

    /// <summary>
    /// The field may be filtered.
    /// </summary>
    Filter = 1,

    /// <summary>
    /// The field may be sorted.
    /// </summary>
    Sort = 2,

    /// <summary>
    /// The field may be searched.
    /// </summary>
    Search = 4
}
