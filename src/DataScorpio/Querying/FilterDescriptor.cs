namespace DataScorpio.Querying;

/// <summary>
/// Represents one typed filter operation against a configured query field.
/// </summary>
public sealed class FilterDescriptor
{
    /// <summary>
    /// Gets or initializes the public query field name or alias.
    /// </summary>
    public string Field { get; init; }

    /// <summary>
    /// Gets or initializes the normalized operator name.
    /// </summary>
    public string Operator { get; init; }

    /// <summary>
    /// Gets or initializes the parsed value.
    /// </summary>
    public QueryValue Value { get; init; } = QueryValue.Missing;

    /// <summary>
    /// Gets or initializes the raw value before parsing.
    /// </summary>
    public string RawValue { get; init; }
}
