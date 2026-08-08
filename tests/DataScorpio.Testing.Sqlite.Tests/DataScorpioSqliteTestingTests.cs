namespace DataScorpio.Testing.Sqlite.Tests;

using DataScorpio.Profiles;
using DataScorpio.Querying;
using Microsoft.Extensions.DependencyInjection;

public sealed class DataScorpioSqliteTestingTests
{
    [Fact]
    public async Task ApplyAsync_runs_against_sqlite_provider()
    {
        var services = new ServiceCollection();

        services.AddDataScorpioSqliteTesting(profiles => profiles.AddProfile<CustomerQueryProfile>());

        using var provider = services.BuildServiceProvider();
        var dataScorpio = provider.GetRequiredService<IDataScorpioSqliteTesting<Customer>>();

        await dataScorpio.SeedAsync(Customers());

        var result = await dataScorpio.ApplyAsync(
            filters: "Name@=*a,IsActive==true",
            sorts: "Name",
            pageNumber: 1,
            pageSize: 2);

        result
            .ShouldBeSuccessful()
            .ShouldContainOnly(customer => customer.IsActive)
            .ShouldBeSortedBy(customer => customer.Name)
            .ShouldHavePage(pageNumber: 1, pageSize: 2, totalRows: 2);
    }

    private static IReadOnlyList<Customer> Customers()
        =>
        [
            new Customer { Id = 1, Name = "Ada", IsActive = true },
            new Customer { Id = 2, Name = "Grace", IsActive = true },
            new Customer { Id = 3, Name = "Alan", IsActive = false }
        ];

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter(customer => customer.Name)
                .AllowFilter(customer => customer.IsActive)
                .AllowSort(customer => customer.Name)
                .MaxPageSize(10);
        }
    }

    private sealed class Customer
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public bool IsActive { get; init; }
    }
}

