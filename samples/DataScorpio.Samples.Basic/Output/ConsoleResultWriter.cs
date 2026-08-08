namespace DataScorpio.Samples.Basic.Output;

using DataScorpio.Execution;
using DataScorpio.Samples.Basic.Domain;

internal sealed class ConsoleResultWriter
{
    public void Write(string title, QueryExecutionResult<Customer> result)
    {
        Console.WriteLine(title);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Validation.Errors)
                Console.WriteLine($"  - {error.Code}: {error.Message}");

            Console.WriteLine();
            return;
        }

        Console.WriteLine($"  rows: {result.Result.RowCount}, page: {result.Result.PageNumber}/{result.Result.PageCount}");

        foreach (var customer in result.Result.Items)
        {
            var deleted = customer.DeletedAt.HasValue ? $"deleted {customer.DeletedAt:yyyy-MM-dd}" : "active row";
            Console.WriteLine(
                $"  - {customer.Name} | {customer.Status} | {customer.Region} | {customer.TenantId} | {customer.CreatedAt:yyyy-MM-dd} | {deleted}");
        }

        Console.WriteLine();
    }
}

