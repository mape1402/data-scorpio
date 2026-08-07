namespace DataScorpio.Tests.Execution;

using DataScorpio.Execution;
using DataScorpio.Parsing.Sieve;
using DataScorpio.Profiles;
using DataScorpio.Querying;

public sealed class QueryableQueryApplierTests
{
    private readonly QueryableQueryApplier applier = new();
    private readonly SieveQueryParser parser = new();
    private readonly QueryProfileDefinition profile = new CustomerQueryProfile().BuildDefinition();

    [Fact]
    public void Apply_filters_with_sieve_contains_and_equality()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = "Name@=Ada,Status==Active"
        });

        var results = Apply(descriptor).ToArray();

        var result = Assert.Single(results);
        Assert.Equal("Ada", result.Name);
    }

    [Fact]
    public void Apply_supports_or_filter_groups()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = "(Name|Email)@=grace"
        });

        var results = Apply(descriptor).ToArray();

        var result = Assert.Single(results);
        Assert.Equal("Grace", result.Name);
    }

    [Fact]
    public void Apply_supports_case_insensitive_string_operators()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = "Name@=*ADA"
        });

        var result = Assert.Single(Apply(descriptor));

        Assert.Equal("Ada", result.Name);
    }

    [Fact]
    public void Apply_supports_null_equality()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = "DeletedAt==null"
        });

        var results = Apply(descriptor).Select(customer => customer.Name).ToArray();

        Assert.Equal(["Grace", "Ada"], results);
    }

    [Fact]
    public void Apply_sorts_with_then_by_order()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Sorts = "Status,-CreatedAt"
        });

        var results = Apply(descriptor).Select(customer => customer.Name).ToArray();

        Assert.Equal(["Ada", "Grace", "Alan"], results);
    }

    [Fact]
    public void Apply_uses_default_sort_when_no_sort_is_requested()
    {
        var results = Apply(QueryDescriptor.Empty).Select(customer => customer.Name).ToArray();

        Assert.Equal(["Grace", "Alan", "Ada"], results);
    }

    [Fact]
    public void Apply_pages_after_filtering_and_sorting()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Sorts = "Name",
            PageNumber = 2,
            PageSize = 1
        });

        var result = Assert.Single(Apply(descriptor));

        Assert.Equal("Alan", result.Name);
    }

    [Fact]
    public void Apply_searches_configured_search_fields()
    {
        var descriptor = new QueryDescriptor
        {
            Search = new SearchDescriptor { Term = "NAVY" }
        };

        var result = Assert.Single(Apply(descriptor));

        Assert.Equal("Grace", result.Name);
    }

    [Fact]
    public void Apply_supports_comparison_operators()
    {
        var descriptor = parser.Parse(new QueryRequest
        {
            Filters = "Score>=90"
        });

        var results = Apply(descriptor).Select(customer => customer.Name).ToArray();

        Assert.Equal(["Grace", "Ada"], results);
    }

    private IQueryable<Customer> Apply(QueryDescriptor descriptor)
        => applier.Apply(Customers().AsQueryable(), descriptor, profile);

    private static IReadOnlyList<Customer> Customers()
        =>
        [
            new Customer
            {
                Name = "Ada",
                Email = "ada@turtlepath.dev",
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 3),
                Score = 95
            },
            new Customer
            {
                Name = "Grace",
                Email = "grace@navy.mil",
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 1),
                Score = 99
            },
            new Customer
            {
                Name = "Alan",
                Email = "alan@computing.org",
                Status = "Inactive",
                CreatedAt = new DateTime(2026, 1, 2),
                DeletedAt = new DateTime(2026, 2, 1),
                Score = 80
            }
        ];

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter(customer => customer.Name)
                .AllowSort(customer => customer.Name)
                .AllowSearch(customer => customer.Name)
                .AllowFilter(customer => customer.Email)
                .AllowSearch(customer => customer.Email)
                .AllowFilter(customer => customer.Status)
                .AllowSort(customer => customer.Status)
                .AllowFilter(customer => customer.DeletedAt)
                .AllowFilter(customer => customer.Score)
                .DefaultSort(customer => customer.CreatedAt);
        }
    }

    private sealed class Customer
    {
        public string Name { get; init; }

        public string Email { get; init; }

        public string Status { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? DeletedAt { get; init; }

        public int Score { get; init; }
    }
}
