namespace DataScorpio.Tests.Validation;

using DataScorpio.Profiles;
using DataScorpio.Querying;
using DataScorpio.Validation;

public sealed class QueryDescriptorValidatorTests
{
    private readonly QueryDescriptorValidator validator = new();
    private readonly QueryProfileDefinition profile = new CustomerQueryProfile().BuildDefinition();

    [Fact]
    public void Validate_accepts_configured_fields_and_page_size()
    {
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
                            Field = "name",
                            Operator = "contains",
                            RawValue = "ada",
                            Value = QueryValue.From("ada")
                        }
                    ]
                }
            ],
            Sorts =
            [
                new SortDescriptor { Field = "created", Direction = SortDirection.Descending }
            ],
            Search = new SearchDescriptor
            {
                Term = "ada",
                Fields = ["name"]
            },
            Includes = [new IncludeDescriptor { Name = "orders" }],
            Page = new PageDescriptor
            {
                PageNumber = 1,
                PageSize = 50
            }
        };

        var result = validator.Validate(descriptor, profile);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_rejects_unknown_filter_field()
    {
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
                            Field = "passwordHash",
                            Operator = "contains",
                            RawValue = "secret",
                            Value = QueryValue.From("secret")
                        }
                    ]
                }
            ]
        };

        var error = Assert.Single(validator.Validate(descriptor, profile).Errors);

        Assert.Equal(QueryValidationCodes.UnknownField, error.Code);
        Assert.Equal("passwordHash", error.Field);
    }

    [Fact]
    public void Validate_rejects_filtering_sort_only_field()
    {
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
                            Field = "created",
                            Operator = "equals",
                            RawValue = "2026-08-07",
                            Value = QueryValue.From("2026-08-07")
                        }
                    ]
                }
            ]
        };

        var error = Assert.Single(validator.Validate(descriptor, profile).Errors);

        Assert.Equal(QueryValidationCodes.FieldNotFilterable, error.Code);
    }

    [Fact]
    public void Validate_rejects_sorting_filter_only_field()
    {
        var descriptor = new QueryDescriptor
        {
            Sorts =
            [
                new SortDescriptor { Field = "status" }
            ]
        };

        var error = Assert.Single(validator.Validate(descriptor, profile).Errors);

        Assert.Equal(QueryValidationCodes.FieldNotSortable, error.Code);
    }

    [Fact]
    public void Validate_rejects_searching_non_searchable_field()
    {
        var descriptor = new QueryDescriptor
        {
            Search = new SearchDescriptor
            {
                Term = "active",
                Fields = ["status"]
            }
        };

        var error = Assert.Single(validator.Validate(descriptor, profile).Errors);

        Assert.Equal(QueryValidationCodes.FieldNotSearchable, error.Code);
    }

    [Fact]
    public void Validate_rejects_unknown_include()
    {
        var descriptor = new QueryDescriptor
        {
            Includes = [new IncludeDescriptor { Name = "passwords" }]
        };

        var error = Assert.Single(validator.Validate(descriptor, profile).Errors);

        Assert.Equal(QueryValidationCodes.UnknownInclude, error.Code);
        Assert.Equal("passwords", error.Field);
    }

    [Fact]
    public void Validate_rejects_invalid_paging()
    {
        var descriptor = new QueryDescriptor
        {
            Page = new PageDescriptor
            {
                PageNumber = 0,
                PageSize = 500
            }
        };

        var result = validator.Validate(descriptor, profile);

        Assert.Contains(result.Errors, error => error.Code == QueryValidationCodes.InvalidPageNumber);
        Assert.Contains(result.Errors, error => error.Code == QueryValidationCodes.PageSizeExceeded);
    }

    [Fact]
    public void Validate_rejects_missing_filter_operator()
    {
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
                            Field = "name",
                            Value = QueryValue.From("ada")
                        }
                    ]
                }
            ]
        };

        var error = Assert.Single(validator.Validate(descriptor, profile).Errors);

        Assert.Equal(QueryValidationCodes.MissingOperator, error.Code);
    }

    [Fact]
    public void Validate_accepts_custom_filter_and_sort_names()
    {
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
                            Field = "activeOnly",
                            Operator = "equals",
                            Value = QueryValue.From(true)
                        }
                    ]
                }
            ],
            Sorts = [new SortDescriptor { Field = "newest", Direction = SortDirection.Descending }]
        };

        var result = validator.Validate(descriptor, profile);

        Assert.True(result.IsValid);
    }

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter("name", customer => customer.Name)
                .AllowSearch("name", customer => customer.Name)
                .AllowFilter("status", customer => customer.Status)
                .AllowSort("created", customer => customer.CreatedAt)
                .AllowInclude("orders", customer => customer.Orders)
                .CustomFilter("activeOnly", (query, value) => query.Where(customer => customer.Status == "Active"))
                .CustomSort("newest", (query, direction) => direction == SortDirection.Descending
                    ? query.OrderByDescending(customer => customer.CreatedAt)
                    : query.OrderBy(customer => customer.CreatedAt))
                .MaxPageSize(100);
        }
    }

    private sealed class Customer
    {
        public string Name { get; init; }

        public string Status { get; init; }

        public DateTime CreatedAt { get; init; }

        public IReadOnlyCollection<Order> Orders { get; init; } = Array.Empty<Order>();
    }

    private sealed class Order
    {
        public string Number { get; init; }
    }
}
