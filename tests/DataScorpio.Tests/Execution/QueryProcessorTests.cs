namespace DataScorpio.Tests.Execution;

using DataScorpio.Execution;
using DataScorpio.Parsing.Sieve;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Validation;

public sealed class QueryProcessorTests
{
    [Fact]
    public void Execute_parses_validates_counts_pages_and_returns_results()
    {
        var processor = CreateProcessor();

        var result = processor.Execute(Customers().AsQueryable(), new QueryRequest
        {
            Filters = "Status==Active",
            Sorts = "Name",
            PageNumber = 1,
            PageSize = 1
        });

        Assert.True(result.IsSuccess);
        Assert.True(result.Validation.IsValid);
        Assert.Equal(2, result.Result.RowCount);
        Assert.Equal(2, result.Result.PageCount);
        Assert.Equal(1, result.Result.PageNumber);
        Assert.Equal(1, result.Result.PageSize);
        Assert.Equal("Ada", Assert.Single(result.Result.Items).Name);
    }

    [Fact]
    public void Execute_rejects_invalid_queries_before_execution()
    {
        var processor = CreateProcessor();

        var result = processor.Execute(Customers().AsQueryable(), new QueryRequest
        {
            Filters = "PasswordHash==secret"
        });

        Assert.False(result.IsSuccess);
        Assert.Null(result.Result);
        Assert.Contains(result.Validation.Errors, error => error.Code == QueryValidationCodes.UnknownField);
    }

    [Fact]
    public void Execute_can_resolve_profile_from_registry()
    {
        var processor = CreateProcessor();

        var result = processor.Execute(Customers().AsQueryable(), new QueryRequest
        {
            Filters = "Name@=*ada"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada", Assert.Single(result.Result.Items).Name);
    }

    private static QueryProcessor CreateProcessor()
    {
        var profile = new CustomerQueryProfile().BuildDefinition();
        var registry = new QueryProfileRegistryBuilder()
            .AddProfile(profile)
            .Build();

        return new QueryProcessor(
            new SieveQueryParser(),
            new QueryDescriptorValidator(),
            new QueryableQueryApplier(),
            registry);
    }

    private static IReadOnlyList<Customer> Customers()
        =>
        [
            new Customer { Name = "Ada", Status = "Active" },
            new Customer { Name = "Grace", Status = "Active" },
            new Customer { Name = "Alan", Status = "Inactive" }
        ];

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter(customer => customer.Name)
                .AllowSort(customer => customer.Name)
                .AllowFilter(customer => customer.Status)
                .MaxPageSize(25);
        }
    }

    private sealed class Customer
    {
        public string Name { get; init; }

        public string Status { get; init; }
    }
}
