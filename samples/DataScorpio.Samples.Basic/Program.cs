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
        var customers = SeedCustomers().AsQueryable();
        using var services = new ServiceCollection()
            .AddDataScorpio(profiles => profiles
                .AddProfile<CustomerQueryProfile>()
                .CustomFilter<IRegional>("InRegion", value =>
                    customer => customer.Region == Convert.ToString(value.Value))
                .CustomSort<ICreated>("RecentlyCreated", customer => customer.CreatedAt))
            .BuildServiceProvider();

        var processor = services.GetRequiredService<IQueryProcessor>();
        var jsonParser = services.GetRequiredService<IJsonQueryDescriptorParser>();

        RunSimpleQueryableQuery(customers, processor);
        RunCustomQuery(customers, processor);
        RunNativeJsonQuery(customers, processor, jsonParser);
    }

    private static void RunSimpleQueryableQuery(IQueryable<Customer> customers, IQueryProcessor processor)
    {
        var result = processor.Execute(customers, new QueryRequest
        {
            Filters = "Status==Active,Name@=*a",
            Sorts = "-CreatedAt",
            Search = "north",
            PageNumber = 1,
            PageSize = 2
        });

        PrintItems("IQueryable query", result);
    }

    private static void RunCustomQuery(IQueryable<Customer> customers, IQueryProcessor processor)
    {
        var result = processor.Execute(customers, new QueryRequest
        {
            Filters = "InRegion==South",
            Sorts = "-RecentlyCreated"
        });

        PrintItems("Custom filter and sort", result);
    }

    private static void RunNativeJsonQuery(
        IQueryable<Customer> customers,
        IQueryProcessor processor,
        IJsonQueryDescriptorParser jsonParser)
    {
        var descriptor = jsonParser.Parse("""
        {
          "filters": [
            { "field": "Status", "operator": "equals", "value": "Active" }
          ],
          "sorts": [
            { "field": "CreatedAt", "direction": "desc" }
          ],
          "search": {
            "term": "south",
            "fields": [ "Region" ]
          },
          "page": {
            "pageNumber": 1,
            "pageSize": 5
          }
        }
        """);

        var result = processor.Execute(customers, descriptor);

        PrintItems("Native JSON descriptor query", result);
    }

    private static void PrintItems(string title, QueryExecutionResult<Customer> result)
    {
        Console.WriteLine(title);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Validation.Errors)
                Console.WriteLine($"  - {error.Code}: {error.Message}");

            Console.WriteLine();
            return;
        }

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
                .AllowFilter(customer => customer.Name)
                .AllowFilter(customer => customer.Status)
                .AllowSearch(customer => customer.Name)
                .AllowSearch(customer => customer.Region)
                .AllowSort(customer => customer.CreatedAt)
                .DefaultSort(customer => customer.CreatedAt, SortDirection.Descending)
                .MaxPageSize(50);
        }
    }

    private interface IRegional
    {
        string Region { get; }
    }

    private interface ICreated
    {
        DateTime CreatedAt { get; }
    }

    private sealed record Customer(
        string Name,
        string Status,
        string Region,
        DateTime CreatedAt) : IRegional, ICreated;
}
