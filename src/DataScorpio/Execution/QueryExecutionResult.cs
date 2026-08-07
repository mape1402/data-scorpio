namespace DataScorpio.Execution;

using DataScorpio.Querying;
using DataScorpio.Validation;

/// <summary>
/// Represents either a successful query result or validation diagnostics.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class QueryExecutionResult<T>
{
    private QueryExecutionResult(QueryResult<T> result, QueryValidationResult validation)
    {
        Result = result;
        Validation = validation ?? throw new ArgumentNullException(nameof(validation));
    }

    /// <summary>
    /// Gets a value indicating whether execution succeeded.
    /// </summary>
    public bool IsSuccess => Validation.IsValid;

    /// <summary>
    /// Gets the query result when execution succeeded.
    /// </summary>
    public QueryResult<T> Result { get; }

    /// <summary>
    /// Gets validation diagnostics.
    /// </summary>
    public QueryValidationResult Validation { get; }

    /// <summary>
    /// Creates a successful execution result.
    /// </summary>
    /// <param name="result">The query result.</param>
    /// <returns>The execution result.</returns>
    public static QueryExecutionResult<T> Success(QueryResult<T> result)
        => new(result ?? throw new ArgumentNullException(nameof(result)), QueryValidationResult.Success);

    /// <summary>
    /// Creates a rejected execution result.
    /// </summary>
    /// <param name="validation">The validation diagnostics.</param>
    /// <returns>The execution result.</returns>
    public static QueryExecutionResult<T> Rejected(QueryValidationResult validation)
        => new(null, validation);
}
