namespace DataScorpio.Testing.Sqlite.Tests;

using DataScorpio.Execution;
using DataScorpio.Profiles;
using DataScorpio.Querying;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public sealed class QueryCriteriaResultSqliteTests
{
    [Fact]
    public async Task ApplyCriteria_preserves_async_provider_for_simple_filters_aliases_and_sorts()
    {
        using var provider = CreateProvider();
        await using var database = await CreateDatabaseAsync();
        var processor = provider.GetRequiredService<IQueryProcessor>();

        var result = processor.ApplyCriteria(database.Customers.AsNoTracking(), new QueryRequest
        {
            Filters = "customerName@=*a,IsActive==true",
            Sorts = "-CreatedAt"
        });

        Assert.False(result.IsMaterialized);
        Assert.True(result.SupportsAsyncEnumeration);
        Assert.Empty(result.Items);
        Assert.IsAssignableFrom<IAsyncEnumerable<Customer>>(result.Query);

        var rows = await result.Query.ToListAsync();

        Assert.Equal(["Ada", "Grace"], rows.Select(customer => customer.Name));
    }

    [Fact]
    public async Task ApplyCriteria_preserves_async_provider_for_global_convention_custom_filters()
    {
        using var provider = CreateProvider();
        await using var database = await CreateDatabaseAsync();
        var processor = provider.GetRequiredService<IQueryProcessor>();

        var result = processor.ApplyCriteria(database.Customers.AsNoTracking(), new QueryRequest
        {
            Filters = "InRegion==South",
            Sorts = "Name"
        });

        Assert.False(result.IsMaterialized);
        Assert.True(result.SupportsAsyncEnumeration);

        var rows = await result.Query.ToListAsync();

        Assert.Equal(["Alan", "Grace"], rows.Select(customer => customer.Name));
    }

    [Fact]
    public async Task ApplyCriteria_marks_result_as_materialized_when_custom_filter_returns_in_memory_query()
    {
        using var provider = CreateProvider();
        await using var database = await CreateDatabaseAsync();
        var processor = provider.GetRequiredService<IQueryProcessor>();

        var result = processor.ApplyCriteria(database.Customers.AsNoTracking(), new QueryRequest
        {
            Filters = "MaterializedActive==true",
            Sorts = "Name"
        });

        Assert.True(result.IsMaterialized);
        Assert.False(result.SupportsAsyncEnumeration);
        Assert.NotEmpty(result.Items);
        Assert.False(result.Query is IAsyncEnumerable<Customer>);

        var rows = result.IsMaterialized
            ? result.Items
            : await result.Query.ToListAsync();

        Assert.Equal(["Ada", "Grace"], rows.Select(customer => customer.Name));
    }

    [Fact]
    public async Task ApplyCriteria_allows_provider_backed_consumer_to_page_after_criteria()
    {
        using var provider = CreateProvider();
        await using var database = await CreateDatabaseAsync();
        var processor = provider.GetRequiredService<IQueryProcessor>();

        var result = processor.ApplyCriteria(database.Customers.AsNoTracking(), new QueryRequest
        {
            Filters = "IsActive==true",
            Sorts = "Name"
        });

        Assert.False(result.IsMaterialized);

        var totalRows = await result.Query.CountAsync();
        var page = await result.Query.Skip(1).Take(1).ToListAsync();

        Assert.Equal(2, totalRows);
        Assert.Equal("Grace", Assert.Single(page).Name);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();

        services.AddDataScorpio(options => options
            .AddConventions<CustomerQueryConventions>()
            .AddProfile<CustomerQueryProfile>());

        return services.BuildServiceProvider();
    }

    private static async Task<CustomerDbContext> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseSqlite(connection)
            .Options;

        var database = new CustomerDbContext(options);

        await database.Database.EnsureCreatedAsync();
        database.Customers.AddRange(Customers());
        await database.SaveChangesAsync();

        return database;
    }

    private static IReadOnlyList<Customer> Customers()
        =>
        [
            new Customer { Id = 1, Name = "Ada", IsActive = true, Region = "North", CreatedAt = new DateTime(2026, 1, 3) },
            new Customer { Id = 2, Name = "Grace", IsActive = true, Region = "South", CreatedAt = new DateTime(2026, 1, 1) },
            new Customer { Id = 3, Name = "Alan", IsActive = false, Region = "South", CreatedAt = new DateTime(2026, 1, 2) }
        ];

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter("customerName", customer => customer.Name)
                .AllowFilter(customer => customer.IsActive)
                .AllowSort(customer => customer.Name)
                .AllowSort(customer => customer.CreatedAt)
                .CustomFilter("MaterializedActive", (query, value) =>
                    query.Where(customer => customer.IsActive).ToList().AsQueryable())
                .MaxPageSize(10);
        }
    }

    private sealed class CustomerQueryConventions : QueryConventionSet
    {
        public override void Configure(IQueryConventionBuilder builder)
        {
            builder.CustomFilter<IHasRegion>("InRegion", value =>
                customer => customer.Region == Convert.ToString(value.Value));
        }
    }

    private interface IHasRegion
    {
        string Region { get; }
    }

    private sealed class Customer : IHasRegion
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public bool IsActive { get; init; }

        public string Region { get; init; }

        public DateTime CreatedAt { get; init; }
    }

    private sealed class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
    }
}
