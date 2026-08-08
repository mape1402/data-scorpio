namespace DataScorpio.Samples.Basic;

using DataScorpio.Execution;
using DataScorpio.Parsing.Json;
using DataScorpio.Profiles;
using DataScorpio.Querying;

internal static class Program
{
    private static void Main()
    {
        var customers = SeedCustomers().AsQueryable();
        var profile = new CustomerQueryProfile();

        RunSimpleQueryableQuery(customers, profile);
        RunCustomQuery(customers, profile);
        RunNativeJsonQuery(customers, profile);
    }

    private static void RunSimpleQueryableQuery(IQueryable<Customer> customers, CustomerQueryProfile profile)
    {
        var results = customers.ApplyDataScorpio(
            new QueryRequest
            {
                Filters = "Status==Active,Name@=*a",
                Sorts = "-CreatedAt",
                Search = "north",
                PageNumber = 1,
                PageSize = 2
            },
            profile);

        PrintItems("Direct IQueryable query", results);
    }

    private static void RunCustomQuery(IQueryable<Customer> customers, CustomerQueryProfile profile)
    {
        var results = customers.ApplyDataScorpio(
            new QueryRequest
            {
                Filters = "InRegion==South",
                Sorts = "-RecentlyCreated"
            },
            profile);

        PrintItems("Custom filter and sort", results);
    }

    private static void RunNativeJsonQuery(IQueryable<Customer> customers, CustomerQueryProfile profile)
    {
        var descriptor = new JsonQueryDescriptorParser().Parse("""
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

        var results = customers.ApplyDataScorpio(descriptor, profile);

        PrintItems("Native JSON descriptor query", results);
    }

    private static void PrintItems(string title, IQueryable<Customer> query)
    {
        Console.WriteLine(title);

        foreach (var customer in query)
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
                .CustomFilter("InRegion", (query, value) =>
                    query.Where(customer => customer.Region == Convert.ToString(value.Value)))
                .CustomSort("RecentlyCreated", (query, direction) => direction == SortDirection.Descending
                    ? query.OrderByDescending(customer => customer.CreatedAt)
                    : query.OrderBy(customer => customer.CreatedAt))
                .DefaultSort(customer => customer.CreatedAt, SortDirection.Descending)
                .MaxPageSize(50);
        }
    }

    private sealed record Customer(
        string Name,
        string Status,
        string Region,
        DateTime CreatedAt);
}
