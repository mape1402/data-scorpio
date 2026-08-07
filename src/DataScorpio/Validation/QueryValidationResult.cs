namespace DataScorpio.Validation;

/// <summary>
/// Represents the result of query validation.
/// </summary>
public sealed class QueryValidationResult
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyCollection<QueryValidationError> Errors { get; init; } = Array.Empty<QueryValidationError>();

    /// <summary>
    /// Gets a value indicating whether the query is valid.
    /// </summary>
    public bool IsValid => !Errors.Any(error => error.Severity == QueryDiagnosticSeverity.Error);

    /// <summary>
    /// Gets a successful validation result.
    /// </summary>
    public static QueryValidationResult Success { get; } = new();
}
