# DataScorpio

[![Build](https://github.com/mape1402/data-scorpio/actions/workflows/CI.yml/badge.svg)](https://github.com/mape1402/data-scorpio/actions/workflows/CI.yml)
[![NuGet](https://img.shields.io/nuget/v/DataScorpio.svg)](https://www.nuget.org/packages/DataScorpio)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**DataScorpio** is a typed query engine for .NET. It turns incoming query input into validated, provider-friendly operations over `IQueryable<T>`.

Use it when an API needs safe filtering, sorting, paging, search, includes, diagnostics, and a migration path from Sieve-style query strings.

## Install

```bash
dotnet add package DataScorpio
```

Optional packages:

```bash
dotnet add package DataScorpio.Testing
```

## Getting Started

Create a query profile. Profiles are allowlists: fields are not queryable unless you expose them.

```csharp
using DataScorpio.Profiles;
using DataScorpio.Querying;

public sealed class CustomerQueryProfile : QueryProfile<Customer>
{
    public override void Configure(IQueryProfileBuilder<Customer> builder)
    {
        builder
            .AllowFilter(customer => customer.Name)
            .AllowFilter(customer => customer.Status)
            .AllowSearch(customer => customer.Name)
            .AllowSearch(customer => customer.Region)
            .AllowSort(customer => customer.CreatedAt)
            .DefaultSort(customer => customer.CreatedAt, SortDirection.Descending)
            .MaxPageSize(100);
    }
}
```

Aliases are optional:

```csharp
builder.AllowFilter("customerName", customer => customer.Name);
```

Register DataScorpio once:

```csharp
services.AddDataScorpio(profiles =>
{
    profiles.AddProfile<CustomerQueryProfile>();
});
```

Execute queries through the processor. DataScorpio works over `IQueryable<T>`, so the source can come from EF Core, another ORM, or an in-memory query.

```csharp
using DataScorpio.Execution;
using DataScorpio.Querying;

var result = processor.Execute(customers.AsQueryable(), new QueryRequest
{
    Filters = "Status==Active,Name@=*ada",
    Sorts = "-CreatedAt",
    Search = "north",
    PageNumber = 1,
    PageSize = 25
});

if (result.IsSuccess)
{
    foreach (var customer in result.Result.Items)
        Console.WriteLine(customer.Name);
}
```

## Query Strings

DataScorpio supports Sieve-compatible string input through `QueryRequest`.

```csharp
var request = new QueryRequest
{
    Filters = "Status==Active,CreatedAt>=2026-01-01",
    Sorts = "-CreatedAt,Name",
    Search = "ada",
    PageNumber = 1,
    PageSize = 20
};
```

Common filter operators:

| Operator | Meaning |
| --- | --- |
| `==` | equals |
| `!=` | not equals |
| `>` | greater than |
| `>=` | greater than or equal |
| `<` | less than |
| `<=` | less than or equal |
| `@=` | contains |
| `@=*` | contains, case-insensitive |
| `_=` | starts with |
| `_-=` | ends with |

OR syntax is also supported:

```text
(Name|Email)@=*ada
Status==Active|Pending
```

## Native JSON

Use `IJsonQueryDescriptorParser` when your API accepts structured JSON instead of query strings.

```csharp
using DataScorpio.Parsing.Json;

var jsonParser = serviceProvider.GetRequiredService<IJsonQueryDescriptorParser>();

var descriptor = jsonParser.Parse("""
{
  "filters": [
    { "field": "Status", "operator": "equals", "value": "Active" }
  ],
  "sorts": [
    { "field": "CreatedAt", "direction": "desc" }
  ],
  "search": {
    "term": "north",
    "fields": [ "Region" ]
  },
  "page": {
    "pageNumber": 1,
    "pageSize": 25
  }
}
""");

var result = processor.Execute(customers.AsQueryable(), descriptor);
```

JSON `null` is treated as an explicit null query value:

```json
{ "field": "deletedAt", "operator": "equals", "value": null }
```

## Entity Framework Core

DataScorpio's core query applier already works on EF Core because EF exposes `IQueryable<T>`.

Register:

```csharp
services.AddDataScorpio(profiles =>
{
    profiles.AddProfile<CustomerQueryProfile>();
});
```

Execute over a `DbSet<T>` or any EF `IQueryable<T>`:

```csharp
using DataScorpio.Execution;

var result = processor.Execute(
    dbContext.Customers.AsNoTracking(),
    new QueryRequest
    {
        Filters = "Status==Active",
        Sorts = "-CreatedAt",
        PageNumber = 1,
        PageSize = 25
    });
```

Includes are deny-by-default and must be configured in the profile:

```csharp
builder.AllowInclude("orders", customer => customer.Orders);
```

Then request them through native descriptors:

```json
{
  "includes": [ "orders" ]
}
```

## Custom Filters And Sorts

Use custom filters when a query name does not map cleanly to one property.

```csharp
public sealed class CustomerQueryProfile : QueryProfile<Customer>
{
    public override void Configure(IQueryProfileBuilder<Customer> builder)
    {
        builder
            .AllowFilter(customer => customer.Name)
            .AllowFilter(customer => customer.Status)
            .AllowSort(customer => customer.CreatedAt)
            .CustomFilter("InRegion", (query, value) =>
                query.Where(customer => customer.Region == Convert.ToString(value.Value)))
            .CustomSort("RecentlyCreated", (query, direction) =>
                direction == SortDirection.Descending
                    ? query.OrderByDescending(customer => customer.CreatedAt)
                    : query.OrderBy(customer => customer.CreatedAt));
    }
}
```

Then call it from query strings through the processor:

```csharp
var result = processor.Execute(customers.AsQueryable(), new QueryRequest
{
    Filters = "InRegion==South",
    Sorts = "-RecentlyCreated"
});
```

Custom filters and sorts receive `IQueryable<T>`, so they can stay provider-friendly when you write provider-translatable LINQ.
Use `CustomFilterDescriptor` when the custom filter needs the full operator/value descriptor.

Reusable custom filters and sorts can target a base class or interface contract. Put them in a convention set:

```csharp
public interface ITenantScoped
{
    string TenantId { get; }
}

public interface ICreated
{
    DateTime CreatedAt { get; }
}

public sealed class AppQueryConventions : QueryConventionSet
{
    public override void Configure(IQueryConventionBuilder builder)
    {
        builder
            .CustomFilter<ITenantScoped>("ForTenant", value =>
                entity => entity.TenantId == Convert.ToString(value.Value))
            .CustomSort<ICreated>("RecentlyCreated", entity => entity.CreatedAt);
    }
}
```

Then register the convention set once:

```csharp
services.AddDataScorpio(profiles => profiles
    .AddConventions<AppQueryConventions>()
    .AddProfile<CustomerQueryProfile>()
    .AddProfile<OrderQueryProfile>());
```

Every profile whose entity implements a matching contract receives those custom query names automatically.

```csharp
builder
    .AllowFilter(customer => customer.Name)
    .AllowSort(customer => customer.CreatedAt);
```

## ASP.NET Core

```csharp
using DataScorpio.Execution;
using DataScorpio.Querying;

app.MapGet("/customers", async (
    HttpContext http,
    CustomerDbContext db,
    IQueryProcessor processor,
    CancellationToken cancellationToken) =>
{
    var request = new QueryRequest
    {
        Filters = http.Request.Query["filters"],
        Sorts = http.Request.Query["sorts"],
        Search = http.Request.Query["search"],
        PageNumber = int.TryParse(http.Request.Query["pageNumber"], out var pageNumber) ? pageNumber : null,
        PageSize = int.TryParse(http.Request.Query["pageSize"], out var pageSize) ? pageSize : null
    };

    var result = processor.Execute(db.Customers.AsNoTracking(), request);

    return result.IsSuccess
        ? Results.Ok(result.Result)
        : Results.BadRequest(result.Validation);
});
```

Supported query keys:

```text
?filters=Status==Active&sorts=-CreatedAt&pageNumber=1&pageSize=25&search=ada
```

## Testing

Install:

```bash
dotnet add package DataScorpio.Testing
```

Use `QueryTestHost<T>` and assertions to test profile behavior without a database:

```csharp
using DataScorpio.Testing;

var host = new QueryTestHost<Customer>(new CustomerQueryProfile())
    .WithSeed(
        new Customer("Ada", "Active"),
        new Customer("Grace", "Inactive"));

host.Apply(filters: "Status==Active")
    .ShouldBeSuccessful()
    .ShouldContainOnly(customer => customer.Name == "Ada");
```

For Sieve-compatible testing through DI:

```csharp
services.AddSieveTesting(profiles =>
    profiles.AddProfile<CustomerQueryProfile>());

var sieve = provider.GetRequiredService<ISieveTesting<Customer>>();

await sieve.SeedAsync(customers);

var result = await sieve.ApplyAsync(
    filters: "Name@=*ada",
    sorts: "Name",
    pageNumber: 1,
    pageSize: 10);

result
    .ShouldBeSortedBy(customer => customer.Name)
    .ShouldContainOnly(customer => customer.IsActive)
    .ShouldHavePage(pageNumber: 1, pageSize: 10, totalRows: 25);
```

## API Surface

Core package:

| Type | Purpose |
| --- | --- |
| `QueryRequest` | Raw string-first request model for filters, sorts, search, and paging. |
| `QueryDescriptor` | Parsed native query model. |
| `FilterDescriptor`, `SortDescriptor`, `SearchDescriptor`, `PageDescriptor`, `IncludeDescriptor` | Query descriptor building blocks. |
| `QueryValue` | Represents parsed values, explicit null, and missing values. |
| `QueryProfile<TEntity>` | Base class for typed query profiles. |
| `IQueryProfileBuilder<TEntity>` | Fluent allowlist configuration API. |
| `IQueryParser` / `SieveQueryParser` | Sieve-compatible string parser. |
| `IJsonQueryDescriptorParser` / `JsonQueryDescriptorParser` | Native JSON descriptor parser. |
| `IQueryDescriptorValidator` | Validates descriptors against profiles. |
| `IQueryableQueryApplier` | Applies descriptors to `IQueryable<T>`. |
| `IQueryProcessor` | Parses, validates, applies, counts, pages, and returns results. |
| `QueryExecutionResult<T>` | Success or rejected result with validation diagnostics. |
| `QueryResult<T>` | Items and paging metadata. |

Registration methods:

| Method | Package |
| --- | --- |
| `services.AddDataScorpio(...)` | `DataScorpio` |
| `services.AddDataScorpioSieveCompatibility(...)` | `DataScorpio` |

## Sample

Run the basic in-memory sample:

```bash
dotnet run --project samples/DataScorpio.Samples.Basic/DataScorpio.Samples.Basic.csproj
```
