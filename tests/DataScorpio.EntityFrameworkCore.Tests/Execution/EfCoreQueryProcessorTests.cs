namespace DataScorpio.EntityFrameworkCore.Tests.Execution;

using DataScorpio.EntityFrameworkCore.Execution;
using DataScorpio.Parsing.Sieve;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Validation;
using Microsoft.EntityFrameworkCore;

public sealed class EfCoreQueryProcessorTests
{
    [Fact]
    public async Task ExecuteAsync_uses_ef_async_count_and_list()
    {
        await using var db = CreateDb();
        var processor = CreateProcessor();

        var result = await processor.ExecuteAsync(db.Customers.AsNoTracking(), new QueryRequest
        {
            Filters = "Status==Active",
            Sorts = "-CreatedAt",
            PageNumber = 1,
            PageSize = 1
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Result.RowCount);
        Assert.Equal(2, result.Result.PageCount);
        Assert.Equal("Ada", Assert.Single(result.Result.Items).Name);
    }

    [Fact]
    public async Task ExecuteAsync_returns_validation_errors_without_query_results()
    {
        await using var db = CreateDb();
        var processor = CreateProcessor();

        var result = await processor.ExecuteAsync(db.Customers.AsNoTracking(), new QueryRequest
        {
            Filters = "PasswordHash==secret"
        });

        Assert.False(result.IsSuccess);
        Assert.Null(result.Result);
        Assert.Contains(result.Validation.Errors, error => error.Code == QueryValidationCodes.UnknownField);
    }

    [Fact]
    public async Task ExecuteAsync_accepts_native_descriptor_without_parsing_strings()
    {
        await using var db = CreateDb();
        var processor = CreateProcessor();
        var descriptor = new QueryDescriptor
        {
            FilterGroups =
            [
                new FilterGroupDescriptor
                {
                    Filters =
                    [
                        new FilterDescriptor
                        {
                            Field = "Status",
                            Operator = "equals",
                            Value = QueryValue.From("Active")
                        }
                    ]
                }
            ],
            Sorts = [new SortDescriptor { Field = "CreatedAt", Direction = SortDirection.Descending }],
            Page = new PageDescriptor { PageNumber = 1, PageSize = 1 }
        };

        var result = await processor.ExecuteAsync(db.Customers.AsNoTracking(), descriptor);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Result.RowCount);
        Assert.Equal("Ada", Assert.Single(result.Result.Items).Name);
    }

    [Fact]
    public async Task ExecuteAsync_applies_configured_includes()
    {
        await using var db = CreateDb();
        var processor = CreateProcessor();
        var descriptor = new QueryDescriptor
        {
            Includes = [new IncludeDescriptor { Name = "orders" }],
            Sorts = [new SortDescriptor { Field = "CreatedAt", Direction = SortDirection.Descending }],
            Page = new PageDescriptor { PageNumber = 1, PageSize = 1 }
        };

        var result = await processor.ExecuteAsync(db.Customers.AsNoTracking(), descriptor);

        var customer = Assert.Single(result.Result.Items);
        Assert.Equal("Ada", customer.Name);
        Assert.NotEmpty(customer.Orders);
    }

    [Fact]
    public async Task ExecuteAsync_rejects_unknown_includes()
    {
        await using var db = CreateDb();
        var processor = CreateProcessor();
        var descriptor = new QueryDescriptor
        {
            Includes = [new IncludeDescriptor { Name = "passwords" }]
        };

        var result = await processor.ExecuteAsync(db.Customers.AsNoTracking(), descriptor);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Validation.Errors, error => error.Code == QueryValidationCodes.UnknownInclude);
    }

    private static EfCoreQueryProcessor CreateProcessor()
    {
        var registry = new QueryProfileRegistryBuilder()
            .AddProfile(new CustomerQueryProfile())
            .Build();

        return new EfCoreQueryProcessor(
            new SieveQueryParser(),
            new QueryDescriptorValidator(),
            new DataScorpio.Execution.QueryableQueryApplier(),
            registry);
    }

    private static CustomerDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new CustomerDbContext(options);

        db.Customers.AddRange(
            new Customer
            {
                Name = "Ada",
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 3),
                Orders = [new Order { Number = "A-001" }]
            },
            new Customer { Name = "Grace", Status = "Active", CreatedAt = new DateTime(2026, 1, 1) },
            new Customer { Name = "Alan", Status = "Inactive", CreatedAt = new DateTime(2026, 1, 2) });
        db.SaveChanges();

        return db;
    }

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter(customer => customer.Name)
                .AllowFilter(customer => customer.Status)
                .AllowSort(customer => customer.CreatedAt)
                .AllowInclude("orders", customer => customer.Orders)
                .MaxPageSize(25);
        }
    }

    private sealed class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
    }

    private sealed class Customer
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public string Status { get; init; }

        public DateTime CreatedAt { get; init; }

        public List<Order> Orders { get; init; } = [];
    }

    private sealed class Order
    {
        public int Id { get; init; }

        public string Number { get; init; }
    }
}
