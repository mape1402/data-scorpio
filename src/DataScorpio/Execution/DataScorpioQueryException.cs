namespace DataScorpio.Execution;

using DataScorpio.Validation;

/// <summary>
/// Exception thrown when a direct IQueryable query is rejected by DataScorpio validation.
/// </summary>
public sealed class DataScorpioQueryException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataScorpioQueryException"/> class.
    /// </summary>
    /// <param name="validation">The validation result.</param>
    public DataScorpioQueryException(QueryValidationResult validation)
        : base(BuildMessage(validation))
    {
        Validation = validation ?? throw new ArgumentNullException(nameof(validation));
    }

    /// <summary>
    /// Gets the validation result.
    /// </summary>
    public QueryValidationResult Validation { get; }

    private static string BuildMessage(QueryValidationResult validation)
    {
        if (validation == null || validation.Errors.Count == 0)
            return "The DataScorpio query was rejected.";

        return "The DataScorpio query was rejected: " +
            string.Join("; ", validation.Errors.Select(error => error.Message));
    }
}
