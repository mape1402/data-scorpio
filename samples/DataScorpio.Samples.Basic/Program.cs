namespace DataScorpio.Samples.Basic;

using DataScorpio.Execution;
using DataScorpio.Parsing.Json;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using Microsoft.Extensions.DependencyInjection;

internal static class Program
{
    private static void Main()
    {
        using var services = new ServiceCollection()
            .AddDataScorpio(profiles => profiles.AddProfile(new CustomerQueryProfile()))
            .BuildServiceProvider();

        var processor = services.GetRequiredService<IQueryProcessor>();
        var jsonParser = services.GetRequiredService<IJsonQueryDescriptorParser>();
        var customers = SeedCustomers().AsQueryable();

        RunSieveCompatibleQuery(processor, customers);
        RunNativeJsonQuery(processor, jsonParser, customers);
    }

    private static void RunSieveCompatibleQuery(IQueryProcessor processor, IQueryable<Customer> customers)
    {
        var result = processor.Execute(customers, new QueryRequest
        {
            Filters = "Status==Active,Name@=*a",
            Sorts = "-created",
            Search = "north",
            PageNumber = 1,
            PageSize = 2
        });

        PrintResult("Sieve-compatible query", result);
    }

    private static void RunNativeJsonQuery(
        IQueryProcessor processor,
        IJsonQueryDescriptorParser jsonParser,
        IQueryable<Customer> customers)
    {
        var descriptor = jsonParser.Parse("""
        {
          "filters": [
            { "field": "status", "operator": "equals", "value": "Active" }
          ],
          "sorts": [
            { "field": "created", "direction": "desc" }
          ],
          "search": {
            "term": "south",
            "fields": [ "region" ]
          },
          "page": {
            "pageNumber": 1,
            "pageSize": 5
          }
        }
        """);

        var result = processor.Execute(customers, descriptor);

        PrintResult("Native JSON descriptor query", result);
    }

    private static void PrintResult(string title, QueryExecutionResult<Customer> result)
    {
        Console.WriteLine(title);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Validation.Errors)
                Console.WriteLine($"  {error.Code}: {error.Message}");

            return;
        }

        Console.WriteLine($"  Page {result.Result.PageNumber}/{result.Result.PageCount}");
        Console.WriteLine($"  Rows: {result.Result.RowCount}");

        foreach (var customer in result.Result.Items)
            Console.WriteLine($"  - {customer.Name} | {customer.Status} | {customer.Region} | {customer.CreatedAt:yyyy-MM-dd}");

        Console.WriteLine();
    }

    private static IReadOnlyList<Customer> SeedCustomers()
        =>
        [
            new Customer("Ada Lovelace", "Active", "North", new DateTime(2026, 1, 3)),
            new Customer("Grace Hopper", "Active", "South", new DateTime(2026, 1, 2)),
            new Customer("Alan Turing", "Inactive", "North", new DateTime(2026, 1, 1)),
            new Customer("Katherine Johnson", "Active", "South", new DateTime(2026, 1, 4))
        ];

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter("name", customer => customer.Name)
                .AllowFilter("status", customer => customer.Status)
                .AllowSearch("name", customer => customer.Name)
                .AllowSearch("region", customer => customer.Region)
                .AllowSort("created", customer => customer.CreatedAt)
                .DefaultSort("created", customer => customer.CreatedAt, SortDirection.Descending)
                .MaxPageSize(50);
        }
    }

    private sealed record Customer(
        string Name,
        string Status,
        string Region,
        DateTime CreatedAt);
}
