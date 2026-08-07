namespace DataScorpio.Tests.Validation;

using DataScorpio.Validation;

public sealed class QueryValidationResultTests
{
    [Fact]
    public void Success_has_no_errors()
    {
        var result = QueryValidationResult.Success;

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Warnings_do_not_make_result_invalid()
    {
        var result = new QueryValidationResult
        {
            Errors =
            [
                new QueryValidationError
                {
                    Code = "query.warning",
                    Message = "A warning.",
                    Severity = QueryDiagnosticSeverity.Warning
                }
            ]
        };

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Errors_make_result_invalid()
    {
        var result = new QueryValidationResult
        {
            Errors =
            [
                new QueryValidationError
                {
                    Code = "query.invalid_field",
                    Field = "passwordHash",
                    Message = "The field is not queryable."
                }
            ]
        };

        Assert.False(result.IsValid);
    }
}
