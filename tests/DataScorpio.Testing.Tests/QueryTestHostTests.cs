namespace DataScorpio.Testing.Tests;

using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Validation;

public sealed class QueryTestHostTests
{
    [Fact]
    public void Apply_executes_queries_against_seed_data()
    {
        var result = CreateHost()
            .Apply(filters: "Status==Active", sorts: "Name", pageNumber: 1, pageSize: 1);

        result
            .ShouldBeSuccessful()
            .ShouldContainOnly(customer => customer.Status == "Active")
            .ShouldHavePage(pageNumber: 1, pageSize: 1, totalRows: 2);

        Assert.Equal("Ada", Assert.Single(result.Result.Items).Name);
    }

    [Fact]
    public void Apply_exposes_validation_assertions()
    {
        var result = CreateHost()
            .Apply(filters: "PasswordHash==secret");

        result.ShouldRejectWith(QueryValidationCodes.UnknownField);
    }

    private static QueryTestHost<Customer> CreateHost()
        => new QueryTestHost<Customer>(new CustomerQueryProfile())
            .WithSeed(
                new Customer { Name = "Ada", Status = "Active" },
                new Customer { Name = "Grace", Status = "Active" },
                new Customer { Name = "Alan", Status = "Inactive" });

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter(customer => customer.Name)
                .AllowSort(customer => customer.Name)
                .AllowFilter(customer => customer.Status)
                .MaxPageSize(10);
        }
    }

    private sealed class Customer
    {
        public string Name { get; init; }

        public string Status { get; init; }
    }
}
