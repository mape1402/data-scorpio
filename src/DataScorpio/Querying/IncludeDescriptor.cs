namespace DataScorpio.Querying;

/// <summary>
/// Represents one requested provider include.
/// </summary>
public sealed class IncludeDescriptor
{
    /// <summary>
    /// Gets or initializes the public include name or alias.
    /// </summary>
    public string Name { get; init; }
}
