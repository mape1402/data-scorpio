namespace DataScorpio.Validation;

/// <summary>
/// Represents a validation diagnostic produced before query execution.
/// </summary>
public sealed class QueryValidationError
{
    /// <summary>
    /// Gets or initializes the stable diagnostic code.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Gets or initializes the related field.
    /// </summary>
    public string Field { get; init; }

    /// <summary>
    /// Gets or initializes the related operator.
    /// </summary>
    public string Operator { get; init; }

    /// <summary>
    /// Gets or initializes the raw value.
    /// </summary>
    public string RawValue { get; init; }

    /// <summary>
    /// Gets or initializes the human-readable message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets or initializes the severity.
    /// </summary>
    public QueryDiagnosticSeverity Severity { get; init; } = QueryDiagnosticSeverity.Error;
}
