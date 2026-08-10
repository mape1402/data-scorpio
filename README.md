# DataScorpio

[![Build](https://github.com/mape1402/data-scorpio/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/mape1402/data-scorpio/actions/workflows/build-and-release.yml)
[![NuGet](https://img.shields.io/nuget/v/DataScorpio.svg)](https://www.nuget.org/packages/DataScorpio)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**DataScorpio** is a typed query engine for .NET APIs. It parses incoming query input, validates it against an explicit profile, and applies provider-friendly filtering, sorting, search, and paging over `IQueryable<T>`.

It is designed for APIs that need a simple string query model today, a structured JSON query model when the client grows, and a testing layer that can validate behavior against LINQ-to-Objects or SQLite.

## Packages

```bash
dotnet add package DataScorpio
```

Optional testing packages:

```bash
dotnet add package DataScorpio.Testing
dotnet add package DataScorpio.Testing.Sqlite
```

| Package | Purpose |
| --- | --- |
| `DataScorpio` | Core query profiles, parsing, validation, execution, and DI registration. |
| `DataScorpio.Testing` | In-memory test host, async testing service, and query assertions. |
| `DataScorpio.Testing.Sqlite` | SQLite-backed testing service for provider translation checks. |

## Getting Started

Create a profile for the entity you want to query. By default, nothing is queryable until the profile exposes it.

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
            .AllowFilter(customer => customer.CreatedAt)
            .AllowSearch(customer => customer.Name)
            .AllowSearch(customer => customer.Email)
            .AllowSearch(customer => customer.Region)
            .AllowSort(customer => customer.Name)
            .AllowSort(customer => customer.CreatedAt)
            .DefaultSort(customer => customer.CreatedAt, SortDirection.Descending)
            .MaxPageSize(100);
    }
}
```

Register DataScorpio once. The simplest path is assembly discovery:

```csharp
using DataScorpio.DependencyInjection;

services.AddDataScorpio(options =>
{
    options.FromAssemblies(typeof(CustomerQueryProfile).Assembly);
});
```

This discovers concrete `QueryProfile<TEntity>` and `QueryConventionSet` types with parameterless constructors from the selected assemblies.

You can still register explicitly when you want full control:

```csharp
services.AddDataScorpio(profiles => profiles
    .AddConventions<AppQueryConventions>()
    .AddProfile<CustomerQueryProfile>());
```

Execute against any `IQueryable<T>`:

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

if (!result.IsSuccess)
    return Results.BadRequest(result.Validation);

return Results.Ok(result.Result);
```

`IQueryProcessor` returns a `QueryExecutionResult<T>`. Successful executions contain a `QueryResult<T>` with items and paging metadata.

## QueryRequest

`QueryRequest` is the string-first request model:

```csharp
public sealed class QueryRequest
{
    public string Filters { get; init; }
    public string Sorts { get; init; }
    public string Search { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}
```

Example query:

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

## String Filtering

DataScorpio supports a Sieve-compatible filter and sort string parser.

Common filter examples:

```text
Status==Active
Status!=Inactive
CreatedAt>=2026-01-01
Name@=*ada
(Name|Email)@=*ada
Status==Active|Pending
DeletedAt==null
```

Supported operators:

| Operator | Normalized name | Meaning |
| --- | --- | --- |
| `==` | `equals` | Equals. |
| `==*` | `equalsInsensitive` | Equals, case-insensitive. |
| `!=` | `notEquals` | Not equals. |
| `!=*` | `notEqualsInsensitive` | Not equals, case-insensitive. |
| `>` | `greaterThan` | Greater than. |
| `>=` | `greaterThanOrEqual` | Greater than or equal. |
| `<` | `lessThan` | Less than. |
| `<=` | `lessThanOrEqual` | Less than or equal. |
| `@=` | `contains` | String contains. |
| `@=*` | `containsInsensitive` | String contains, case-insensitive. |
| `!@=` | `notContains` | String does not contain. |
| `!@=*` | `notContainsInsensitive` | String does not contain, case-insensitive. |
| `_=` | `startsWith` | String starts with. |
| `_=*` | `startsWithInsensitive` | String starts with, case-insensitive. |
| `!_=` | `notStartsWith` | String does not start with. |
| `!_=*` | `notStartsWithInsensitive` | String does not start with, case-insensitive. |
| `_-=` | `endsWith` | String ends with. |
| `_-=*` | `endsWithInsensitive` | String ends with, case-insensitive. |
| `!_-=` | `notEndsWith` | String does not end with. |
| `!_-=*` | `notEndsWithInsensitive` | String does not end with, case-insensitive. |

Sorts use comma-separated field names. Prefix a field with `-` for descending order:

```text
Name
-CreatedAt,Name
```

## Filters And Search

Filters are field-specific predicates:

```text
Status==Active
CreatedAt>=2026-01-01
```

Search is one free-text term applied across the fields marked with `AllowSearch(...)`:

```csharp
var request = new QueryRequest
{
    Search = "north"
};
```

Use filters when the client knows the field and operator. Use search when the client has a general term and wants "find this across the searchable text fields".

## Native JSON

Use `IJsonQueryDescriptorParser` when your API accepts structured JSON instead of strings.

```csharp
using DataScorpio.Parsing.Json;

var jsonParser = serviceProvider.GetRequiredService<IJsonQueryDescriptorParser>();

var descriptor = jsonParser.Parse("""
{
  "filters": [
    { "field": "Status", "operator": "equals", "value": "Active" },
    { "field": "DeletedAt", "operator": "equals", "value": null }
  ],
  "sorts": [
    { "field": "CreatedAt", "direction": "desc" }
  ],
  "search": {
    "term": "north",
    "fields": [ "Name", "Email", "Region" ]
  },
  "includes": [ "orders" ],
  "page": {
    "pageNumber": 1,
    "pageSize": 25
  }
}
""");

var result = processor.Execute(customers.AsQueryable(), descriptor);
```

JSON supports:

| Property | Purpose |
| --- | --- |
| `filters` | A default `and` group of filters. |
| `filterGroups` | Explicit `and` or `or` groups. |
| `sorts` | Ordered sort descriptors. |
| `search` | Free-text search term and optional fields. |
| `includes` | Include names or objects with a `name` property. |
| `page` | `pageNumber` and `pageSize`. |
| `presets` | Parsed preset descriptors for higher-level adapters. |

JSON `null` is represented as an explicit null value. String input uses `null`:

```text
DeletedAt==null
```

```json
{ "field": "DeletedAt", "operator": "equals", "value": null }
```

## Profiles And Aliases

The default public query name is the property name from the expression:

```csharp
builder
    .AllowFilter(customer => customer.Name)
    .AllowSort(customer => customer.CreatedAt);
```

Aliases are optional and useful when the public API name should differ from the CLR property:

```csharp
builder
    .AllowFilter("customerName", customer => customer.Name)
    .AllowSort("created", customer => customer.CreatedAt);
```

Then clients can query:

```text
customerName@=*ada
-created
```

Includes are also explicit:

```csharp
builder.AllowInclude("orders", customer => customer.Orders);
```

The core package parses and validates include names. Provider-specific include execution should live in the application or in an adapter.

## Custom Filters And Sorts

Use a custom filter when the query name is a business concept instead of a single property.

```csharp
public sealed class CustomerQueryProfile : QueryProfile<Customer>
{
    public override void Configure(IQueryProfileBuilder<Customer> builder)
    {
        builder
            .AllowFilter(customer => customer.Name)
            .AllowSort(customer => customer.CreatedAt)
            .CustomFilter("InRegion", (query, value) =>
                query.Where(customer => customer.Region == Convert.ToString(value.Value)))
            .CustomFilterDescriptor("CreatedWindow", (query, filter) =>
                query.Where(customer => customer.CreatedAt >= DateTime.UtcNow.AddDays(-30)))
            .CustomSort("RecentlyCreated", (query, direction) =>
                direction == SortDirection.Descending
                    ? query.OrderByDescending(customer => customer.CreatedAt)
                    : query.OrderBy(customer => customer.CreatedAt));
    }
}
```

Usage:

```csharp
var result = processor.Execute(customers.AsQueryable(), new QueryRequest
{
    Filters = "InRegion==South",
    Sorts = "-RecentlyCreated"
});
```

Custom filters and sorts receive `IQueryable<T>`, so they can stay provider-friendly when the LINQ you write can be translated by the underlying provider.

## Reusable Conventions

When the same custom query applies to every entity that implements a base class or interface, put it in a convention set and register it once.

```csharp
public interface ITenantScoped
{
    string TenantId { get; }
}

public interface ICreatedEntity
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
            .CustomSort<ICreatedEntity>("RecentlyCreated", entity => entity.CreatedAt);
    }
}
```

With assembly discovery, convention sets are picked up automatically with profiles:

```csharp
services.AddDataScorpio(options =>
{
    options.FromAssemblies(typeof(AppQueryConventions).Assembly);
});
```

You can also register conventions explicitly:

```csharp
services.AddDataScorpio(profiles => profiles
    .AddConventions<AppQueryConventions>()
    .AddProfile<CustomerQueryProfile>()
    .AddProfile<OrderQueryProfile>());
```

Any registered profile whose entity implements the matching contract receives those custom query names automatically.

## Results And Validation

Invalid queries are rejected before execution. Unknown fields, non-filterable fields, non-sortable fields, unknown includes, invalid page numbers, invalid page sizes, and page sizes above `MaxPageSize(...)` produce validation errors.

```csharp
var result = processor.Execute(customers.AsQueryable(), new QueryRequest
{
    Filters = "SecretInternalField==true"
});

if (!result.IsSuccess)
{
    foreach (var error in result.Validation.Errors)
        Console.WriteLine($"{error.Code}: {error.Message}");
}
```

`QueryResult<T>` mirrors the paging names commonly used by TurtlePath responses:

| Property | Alias |
| --- | --- |
| `Items` | `Results` |
| `PageNumber` | `CurrentPage` |
| `RowCount` | `TotalRows` |
| `PageCount` | `TotalPages` |
| `PageSize` | |
| `HasPreviousPage` | |
| `HasNextPage` | |

## ASP.NET Core Usage

DataScorpio does not require an ASP.NET Core package. Build a `QueryRequest` from query parameters and pass your `IQueryable<T>` to `IQueryProcessor`.

```csharp
using DataScorpio.Execution;
using DataScorpio.Querying;
using Microsoft.EntityFrameworkCore;

app.MapGet("/customers", (
    HttpContext http,
    CustomerDbContext db,
    IQueryProcessor processor) =>
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

Example URL:

```text
/customers?filters=Status==Active&sorts=-CreatedAt&pageNumber=1&pageSize=25&search=ada
```

## IQueryable Providers

The core package applies queries to `IQueryable<T>`. That means it can run over EF Core, another ORM that exposes `IQueryable<T>`, or in-memory data.

DataScorpio does not ship separate EF Core, ASP.NET Core, or DynaBee packages. The core `IQueryable<T>` pipeline is the integration point.

## Testing

Use `DataScorpio.Testing` to validate filters, sorts, paging, validation errors, and profile behavior without depending on your application host.

```csharp
using DataScorpio.Testing;

var host = new QueryTestHost<Customer>(new CustomerQueryProfile())
    .WithSeed(
        new Customer { Name = "Ada", Status = "Active" },
        new Customer { Name = "Grace", Status = "Inactive" });

host.Apply(filters: "Status==Active", sorts: "Name", pageNumber: 1, pageSize: 10)
    .ShouldBeSuccessful()
    .ShouldBeSortedBy(customer => customer.Name)
    .ShouldContainOnly(customer => customer.Status == "Active")
    .ShouldHavePage(pageNumber: 1, pageSize: 10, totalRows: 1);
```

DI-based testing:

```csharp
using DataScorpio.Testing;

services.AddDataScorpioTesting(profiles =>
    profiles.AddProfile<CustomerQueryProfile>());

var dataScorpio = provider.GetRequiredService<IDataScorpioTesting<Customer>>();

await dataScorpio.SeedAsync(customers);

var result = await dataScorpio.ApplyAsync(
    filters: "Name@=*ada",
    sorts: "Name",
    pageNumber: 1,
    pageSize: 10);

result
    .ShouldBeSuccessful()
    .ShouldBeSortedBy(customer => customer.Name)
    .ShouldContainOnly(customer => customer.Name.Contains("Ada", StringComparison.OrdinalIgnoreCase));
```

SQLite-backed testing:

```csharp
using DataScorpio.Testing.Sqlite;

services.AddDataScorpioSqliteTesting(profiles =>
    profiles.AddProfile<CustomerQueryProfile>());

var dataScorpio = provider.GetRequiredService<IDataScorpioSqliteTesting<Customer>>();

await dataScorpio.SeedAsync(customers);

var result = await dataScorpio.ApplyAsync(
    filters: "Name@=*ada",
    sorts: "Name");

result.ShouldBeSuccessful();
```

SQLite testing is useful when you want a more realistic query provider than LINQ-to-Objects. The entity must be suitable for EF Core SQLite mapping, such as having a key.

Available assertions:

| Assertion | Purpose |
| --- | --- |
| `ShouldBeSuccessful()` | Requires a successful query. |
| `ShouldBeRejected()` | Requires a rejected query. |
| `ShouldRejectWith(code)` | Requires a validation error code. |
| `ShouldContainOnly(predicate)` | Requires every result item to match a predicate. |
| `ShouldBeSortedBy(selector, descending: false)` | Requires result order to match a selector. |
| `ShouldHavePage(pageNumber, pageSize, totalRows)` | Requires paging metadata to match. |

## API Surface

Core:

| Type | Purpose |
| --- | --- |
| `QueryRequest` | String-first request model for filters, sorts, search, and paging. |
| `QueryDescriptor` | Native parsed query model. |
| `FilterDescriptor`, `FilterGroupDescriptor` | Field, operator, value, and grouping model for filters. |
| `SortDescriptor` | Field and direction model for sorting. |
| `SearchDescriptor` | Free-text search descriptor. |
| `IncludeDescriptor` | Include request descriptor. |
| `QueryPresetDescriptor` | Parsed preset descriptor for higher-level adapters. |
| `PageDescriptor` | Page number and page size descriptor. |
| `QueryValue` | Parsed value wrapper for normal values, explicit null, and missing values. |
| `QueryProfile<TEntity>` | Base class for typed query profiles. |
| `IQueryProfileBuilder<TEntity>` | Fluent API for allowlists, aliases, defaults, includes, and custom queries. |
| `QueryConventionSet` | Reusable cross-profile custom filters and sorts. |
| `QueryProfileRegistryBuilder` | Manual and assembly-discovery registration API for profiles and conventions. |
| `IQueryParser` / `SieveQueryParser` | String parser compatible with Sieve-style filter and sort syntax. |
| `IJsonQueryDescriptorParser` / `JsonQueryDescriptorParser` | Native JSON descriptor parser. |
| `IQueryDescriptorValidator` | Validates descriptors against a profile before execution. |
| `IQueryableQueryApplier` | Applies validated descriptors to `IQueryable<T>`. |
| `IQueryProcessor` | Parses, validates, applies, counts, pages, and returns results. |
| `QueryExecutionResult<T>` | Success or rejected result with validation diagnostics. |
| `QueryResult<T>` | Items and paging metadata. |

Registration:

| Method | Package |
| --- | --- |
| `services.AddDataScorpio(...)` | `DataScorpio` |
| `options.FromAssemblies(...)` | `DataScorpio` |
| `options.FromAssembly(...)` | `DataScorpio` |
| `options.FromAssemblyOf<TMarker>()` | `DataScorpio` |
| `services.AddDataScorpioSieveCompatibility(...)` | `DataScorpio` |
| `services.AddDataScorpioTesting(...)` | `DataScorpio.Testing` |
| `services.AddDataScorpioSqliteTesting(...)` | `DataScorpio.Testing.Sqlite` |

## Sample

Run the expanded sample:

```bash
dotnet run --project samples/DataScorpio.Samples.Basic/DataScorpio.Samples.Basic.csproj
```

The sample includes basic filtering, sorting, paging, search, OR filters, aliases, custom conventions, null filters, native JSON, and validation scenarios.
