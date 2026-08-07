namespace DataScorpio.TurtlePath.Tests;

using DataScorpio.Execution;
using DataScorpio.Parsing.Sieve;
using DataScorpio.Profiles;
using DataScorpio.Validation;
using global::TurtlePath.Domain.Contracts;
using global::TurtlePath.Persistence;

public sealed class DataScorpioStorageCriteriaApplierTests
{
    [Fact]
    public void Apply_filters_and_sorts_turtlepath_criteria()
    {
        var applier = CreateApplier();
        var criteria = new GetManyCriteria<Customer>
        {
            Filters = "Status==Active",
            Sorts = "-Name"
        };

        var results = applier.Apply(Customers().AsQueryable(), criteria).Select(customer => customer.Name).ToArray();

        Assert.Equal(["Grace", "Ada"], results);
    }

    [Fact]
    public void Apply_returns_source_when_no_string_criteria_exist()
    {
        var applier = CreateApplier();
        var source = Customers().AsQueryable();

        var result = applier.Apply(source, new GetManyCriteria<Customer>());

        Assert.Same(source, result);
    }

    [Fact]
    public void Apply_throws_strict_exception_for_unknown_fields()
    {
        var applier = CreateApplier();
        var criteria = new GetManyCriteria<Customer>
        {
            Filters = "PasswordHash==secret"
        };

        var exception = Assert.Throws<DataScorpioTurtlePathQueryException>(() =>
            applier.Apply(Customers().AsQueryable(), criteria).ToArray());

        Assert.Contains(exception.Validation.Errors, error => error.Code == QueryValidationCodes.UnknownField);
    }

    private static DataScorpioStorageCriteriaApplier CreateApplier()
    {
        var registry = new QueryProfileRegistryBuilder()
            .AddProfile(new CustomerQueryProfile())
            .Build();

        return new DataScorpioStorageCriteriaApplier(
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
                .AllowFilter(customer => customer.Status)
                .AllowSort(customer => customer.Name);
        }
    }

    private sealed class Customer : IEntity
    {
        public string Name { get; init; }

        public string Status { get; init; }
    }
}
