namespace DataScorpio.Tests.Execution;

using DataScorpio.Execution;
using DataScorpio.Profiles;
using DataScorpio.Querying;

public sealed class DataScorpioQueryableExtensionsTests
{
    [Fact]
    public void ApplyDataScorpio_applies_request_directly_to_queryable()
    {
        var result = Customers()
            .AsQueryable()
            .ApplyDataScorpio(new QueryRequest
            {
                Filters = "Status==Active",
                Sorts = "-CreatedAt"
            }, new CustomerQueryProfile())
            .ToArray();

        Assert.Equal(["Ada", "Grace"], result.Select(customer => customer.Name));
    }

    [Fact]
    public void ApplyDataScorpio_throws_validation_exception_for_rejected_query()
    {
        var exception = Assert.Throws<DataScorpioQueryException>(() =>
            Customers()
                .AsQueryable()
                .ApplyDataScorpio(new QueryRequest { Filters = "PasswordHash==secret" }, new CustomerQueryProfile())
                .ToArray());

        Assert.Contains(exception.Validation.Errors, error => error.Code == "query.unknown_field");
    }

    private static IReadOnlyList<Customer> Customers()
        =>
        [
            new Customer { Name = "Ada", Status = "Active", CreatedAt = new DateTime(2026, 1, 3) },
            new Customer { Name = "Grace", Status = "Active", CreatedAt = new DateTime(2026, 1, 1) },
            new Customer { Name = "Alan", Status = "Inactive", CreatedAt = new DateTime(2026, 1, 2) }
        ];

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter(customer => customer.Status)
                .AllowSort(customer => customer.CreatedAt);
        }
    }

    private sealed class Customer
    {
        public string Name { get; init; }

        public string Status { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
