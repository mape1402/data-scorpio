namespace DataScorpio.Testing;

using DataScorpio.Execution;

/// <summary>
/// Lightweight assertion helpers for DataScorpio testing.
/// </summary>
public static class QueryResultAssertions
{
    /// <summary>
    /// Throws when the result is not successful.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="result">The execution result.</param>
    /// <returns>The same result.</returns>
    public static QueryExecutionResult<T> ShouldBeSuccessful<T>(this QueryExecutionResult<T> result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        if (!result.IsSuccess)
            throw new InvalidOperationException("Expected the query to be successful.");

        return result;
    }

    /// <summary>
    /// Throws when the result is successful.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="result">The execution result.</param>
    /// <returns>The same result.</returns>
    public static QueryExecutionResult<T> ShouldBeRejected<T>(this QueryExecutionResult<T> result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        if (result.IsSuccess)
            throw new InvalidOperationException("Expected the query to be rejected.");

        return result;
    }

    /// <summary>
    /// Throws when any result item does not match the predicate.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="result">The execution result.</param>
    /// <param name="predicate">The expected predicate.</param>
    /// <returns>The same result.</returns>
    public static QueryExecutionResult<T> ShouldContainOnly<T>(
        this QueryExecutionResult<T> result,
        Func<T, bool> predicate)
    {
        result.ShouldBeSuccessful();

        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (result.Result.Items.Any(item => !predicate(item)))
            throw new InvalidOperationException("Expected every result item to satisfy the predicate.");

        return result;
    }

    /// <summary>
    /// Throws when paging metadata does not match.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="result">The execution result.</param>
    /// <param name="pageNumber">The expected page number.</param>
    /// <param name="pageSize">The expected page size.</param>
    /// <param name="rowCount">The expected row count.</param>
    /// <returns>The same result.</returns>
    public static QueryExecutionResult<T> ShouldHavePage<T>(
        this QueryExecutionResult<T> result,
        int pageNumber,
        int pageSize,
        long rowCount)
    {
        result.ShouldBeSuccessful();

        if (result.Result.PageNumber != pageNumber ||
            result.Result.PageSize != pageSize ||
            result.Result.RowCount != rowCount)
        {
            throw new InvalidOperationException("Query page metadata did not match the expected values.");
        }

        return result;
    }

    /// <summary>
    /// Throws when a validation code is not present.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="result">The execution result.</param>
    /// <param name="code">The expected diagnostic code.</param>
    /// <returns>The same result.</returns>
    public static QueryExecutionResult<T> ShouldRejectWith<T>(
        this QueryExecutionResult<T> result,
        string code)
    {
        result.ShouldBeRejected();

        if (!result.Validation.Errors.Any(error => error.Code == code))
            throw new InvalidOperationException($"Expected validation code '{code}'.");

        return result;
    }
}
