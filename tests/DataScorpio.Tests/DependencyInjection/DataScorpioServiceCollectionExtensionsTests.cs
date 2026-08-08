namespace DataScorpio.Tests.DependencyInjection;

using DataScorpio.Execution;
using DataScorpio.Parsing.Json;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using Microsoft.Extensions.DependencyInjection;

public sealed class DataScorpioServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDataScorpio_registers_core_and_native_json_services()
    {
        var services = new ServiceCollection();

        services.AddDataScorpio(profiles =>
            profiles.AddProfile<CustomerQueryProfile>());

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IQueryProcessor>());
        Assert.NotNull(provider.GetRequiredService<IJsonQueryDescriptorParser>());
    }

    [Fact]
    public void AddDataScorpioSieveCompatibility_registers_core_services()
    {
        var services = new ServiceCollection();

        services.AddDataScorpioSieveCompatibility(profiles =>
            profiles.AddProfile<CustomerQueryProfile>());

        using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IQueryProcessor>();

        var result = processor.Execute(Customers().AsQueryable(), new QueryRequest
        {
            Filters = "Name@=*ada"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada", Assert.Single(result.Result.Items).Name);
    }

    [Fact]
    public void AddDataScorpio_applies_contract_custom_queries_to_matching_profiles()
    {
        var services = new ServiceCollection();

        services.AddDataScorpio(profiles => profiles
            .AddProfile<CustomerQueryProfile>()
            .CustomFilter<IHasName>("named", value => entity => entity.Name == Convert.ToString(value.Value))
            .CustomSort<IHasName>("byName", entity => entity.Name));

        using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IQueryProcessor>();

        var result = processor.Execute(Customers().AsQueryable(), new QueryRequest
        {
            Filters = "named==Grace",
            Sorts = "-byName"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Grace", Assert.Single(result.Result.Items).Name);
    }

    private static IReadOnlyList<Customer> Customers()
        => [new Customer { Name = "Ada" }, new Customer { Name = "Grace" }];

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder.AllowFilter(customer => customer.Name);
        }
    }

    private interface IHasName
    {
        string Name { get; }
    }

    private sealed class Customer : IHasName
    {
        public string Name { get; init; }
    }
}
