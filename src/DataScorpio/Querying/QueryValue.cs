namespace DataScorpio.Querying;

/// <summary>
/// Represents a parsed query value, including explicit null and missing states.
/// </summary>
public sealed class QueryValue
{
    private QueryValue(object value, bool isNull, bool isMissing)
    {
        Value = value;
        IsNull = isNull;
        IsMissing = isMissing;
    }

    /// <summary>
    /// Gets the parsed value.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Gets a value indicating whether the query value explicitly represents null.
    /// </summary>
    public bool IsNull { get; }

    /// <summary>
    /// Gets a value indicating whether the query value is missing.
    /// </summary>
    public bool IsMissing { get; }

    /// <summary>
    /// Gets an explicit null query value.
    /// </summary>
    public static QueryValue Null { get; } = new(null, isNull: true, isMissing: false);

    /// <summary>
    /// Gets a missing query value.
    /// </summary>
    public static QueryValue Missing { get; } = new(null, isNull: false, isMissing: true);

    /// <summary>
    /// Creates a non-null query value.
    /// </summary>
    /// <param name="value">The parsed value.</param>
    /// <returns>The query value wrapper.</returns>
    public static QueryValue From(object value)
    {
        if (value == null)
            return Null;

        return new QueryValue(value, isNull: false, isMissing: false);
    }
}
