namespace DataScorpio.Querying;

/// <summary>
/// Represents a named reusable query preset.
/// </summary>
public sealed class QueryPresetDescriptor
{
    /// <summary>
    /// Gets or initializes the preset name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets or initializes preset arguments.
    /// </summary>
    public IReadOnlyDictionary<string, QueryValue> Arguments { get; init; } = new Dictionary<string, QueryValue>();
}
