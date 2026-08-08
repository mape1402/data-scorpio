namespace DataScorpio.Testing.Tests;

using DataScorpio.Profiles;
using DataScorpio.Querying;
using Microsoft.Extensions.DependencyInjection;

public sealed class DataScorpioTestingTests
{
    [Fact]
    public async Task AddDataScorpioTesting_registers_async_testing_host()
    {
        var services = new ServiceCollection();

        services.AddDataScorpioTesting(profiles => profiles.AddProfile<CustomerQueryProfile>());

        using var provider = services.BuildServiceProvider();
        var dataScorpio = provider.GetRequiredService<IDataScorpioTesting<Customer>>();

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

    [Fact]
    public async Task ApplyAsync_accepts_explicit_query_provider()
    {
        var services = new ServiceCollection();

        services.AddDataScorpioTesting(profiles => profiles.AddProfile<CustomerQueryProfile>());

        using var provider = services.BuildServiceProvider();
        var dataScorpio = provider.GetRequiredService<IDataScorpioTesting<Customer>>();

        var result = await dataScorpio.ApplyAsync(
            Customers().AsQueryable(),
            filters: "IsActive==true",
            sorts: "-Name");

        result
            .ShouldBeSuccessful()
            .ShouldContainOnly(customer => customer.IsActive)
            .ShouldBeSortedBy(customer => customer.Name, descending: true);
    }

    [Fact]
    public async Task ApplyAsync_exposes_validation_failures()
    {
        var services = new ServiceCollection();

        services.AddDataScorpioTesting(profiles => profiles.AddProfile<CustomerQueryProfile>());

        using var provider = services.BuildServiceProvider();
        var dataScorpio = provider.GetRequiredService<IDataScorpioTesting<Customer>>();

        await dataScorpio.SeedAsync(Customers());

        var result = await dataScorpio.ApplyAsync(filters: "PasswordHash==secret");

        result.ShouldRejectWith(Validation.QueryValidationCodes.UnknownField);
    }

    private static IReadOnlyList<Customer> Customers()
        =>
        [
            new Customer { Name = "Ada", IsActive = true },
            new Customer { Name = "Grace", IsActive = true },
            new Customer { Name = "Alan", IsActive = false }
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
        public string Name { get; init; }

        public bool IsActive { get; init; }
    }
}

