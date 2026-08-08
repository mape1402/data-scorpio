namespace DataScorpio.Tests.Profiles;

using DataScorpio.Profiles;
using DataScorpio.Querying;

public sealed class QueryProfileBuilderTests
{
    [Fact]
    public void Profile_builds_allowed_fields_with_aliases()
    {
        var definition = new CustomerQueryProfile().BuildDefinition();

        var name = definition.FindField("customerName");

        Assert.NotNull(name);
        Assert.Equal(typeof(Customer), definition.EntityType);
        Assert.Equal("customerName", name.Name);
        Assert.Equal("Name", name.MemberPath);
        Assert.Equal(typeof(string), name.FieldType);
        Assert.True(name.CanFilter);
        Assert.True(name.CanSort);
        Assert.True(name.CanSearch);
    }

    [Fact]
    public void Profile_supports_nested_member_paths()
    {
        var definition = new CustomerQueryProfile().BuildDefinition();

        var city = definition.FindField("city");

        Assert.NotNull(city);
        Assert.Equal("Address.City", city.MemberPath);
        Assert.Equal(typeof(string), city.FieldType);
        Assert.True(city.CanFilter);
    }

    [Fact]
    public void Profile_captures_default_sort_and_max_page_size()
    {
        var definition = new CustomerQueryProfile().BuildDefinition();

        Assert.Equal(100, definition.MaxPageSize);
        Assert.NotNull(definition.DefaultSort);
        Assert.Equal("created", definition.DefaultSort.FieldName);
        Assert.Equal(SortDirection.Descending, definition.DefaultSort.Direction);
    }

    [Fact]
    public void Profile_captures_include_aliases()
    {
        var definition = new CustomerQueryProfile().BuildDefinition();

        var include = definition.FindInclude("orders");

        Assert.NotNull(include);
        Assert.Equal("Orders", include.MemberPath);
    }

    [Fact]
    public void Profile_captures_custom_filters_and_sorts()
    {
        var definition = new CustomerQueryProfile().BuildDefinition();

        Assert.NotNull(definition.FindCustomFilter("activeOnly"));
        Assert.NotNull(definition.FindCustomSort("newest"));
    }

    [Fact]
    public void Builder_rejects_non_member_expressions()
    {
        var builder = new QueryProfileBuilder<Customer>();

        var exception = Assert.Throws<ArgumentException>(() =>
            builder.AllowFilter("lowerName", customer => customer.Name.ToLower()));

        Assert.Contains("simple member access", exception.Message);
    }

    [Fact]
    public void Builder_rejects_alias_reuse_for_different_members()
    {
        var builder = new QueryProfileBuilder<Customer>();

        builder.AllowFilter("field", customer => customer.Name);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.AllowSort("field", customer => customer.Email));

        Assert.Contains("already mapped", exception.Message);
    }

    [Fact]
    public void Builder_rejects_invalid_max_page_size()
    {
        var builder = new QueryProfileBuilder<Customer>();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.MaxPageSize(0));
    }

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter("customerName", customer => customer.Name)
                .AllowSort("customerName", customer => customer.Name)
                .AllowSearch("customerName", customer => customer.Name)
                .AllowFilter("email", customer => customer.Email)
                .AllowFilter("city", customer => customer.Address.City)
                .AllowInclude("orders", customer => customer.Orders)
                .CustomFilter("activeOnly", (query, value) => query.Where(customer => customer.Email != null))
                .CustomSort("newest", (query, direction) => direction == SortDirection.Descending
                    ? query.OrderByDescending(customer => customer.CreatedAt)
                    : query.OrderBy(customer => customer.CreatedAt))
                .DefaultSort("created", customer => customer.CreatedAt, SortDirection.Descending)
                .MaxPageSize(100);
        }
    }

    private sealed class Customer
    {
        public string Name { get; init; }

        public string Email { get; init; }

        public DateTime CreatedAt { get; init; }

        public Address Address { get; init; } = new();

        public IReadOnlyCollection<Order> Orders { get; init; } = Array.Empty<Order>();
    }

    private sealed class Address
    {
        public string City { get; init; }
    }

    private sealed class Order
    {
        public string Number { get; init; }
    }
}
