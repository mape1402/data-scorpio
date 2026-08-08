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
dotnet add package DataScorpio.EntityFrameworkCore
dotnet add package DataScorpio.AspNetCore
dotnet add package DataScorpio.Testing
dotnet add package DataScorpio.DynaBee
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
            .AllowFilter("name", customer => customer.Name)
            .AllowFilter("status", customer => customer.Status)
            .AllowSearch("name", customer => customer.Name)
            .AllowSearch("region", customer => customer.Region)
            .AllowSort("created", customer => customer.CreatedAt)
            .DefaultSort("created", customer => customer.CreatedAt, SortDirection.Descending)
            .MaxPageSize(100);
    }
}
```

Register DataScorpio:

```csharp
using Microsoft.Extensions.DependencyInjection;

services.AddDataScorpio(profiles =>
{
    profiles.AddProfile(new CustomerQueryProfile());
});
```

Execute a query:

```csharp
using DataScorpio.Execution;
using DataScorpio.Querying;

var processor = serviceProvider.GetRequiredService<IQueryProcessor>();

var result = processor.Execute(customers.AsQueryable(), new QueryRequest
{
    Filters = "Status==Active,Name@=*ada",
    Sorts = "-created",
    Search = "north",
    PageNumber = 1,
    PageSize = 25
});

if (!result.IsSuccess)
{
    foreach (var error in result.Validation.Errors)
        Console.WriteLine($"{error.Code}: {error.Message}");

    return;
}

foreach (var customer in result.Result.Items)
    Console.WriteLine(customer.Name);
```

## Query Strings

DataScorpio supports Sieve-compatible string input through `QueryRequest`.

```csharp
var request = new QueryRequest
{
    Filters = "Status==Active,CreatedAt>=2026-01-01",
    Sorts = "-created,name",
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
    { "field": "status", "operator": "equals", "value": "Active" }
  ],
  "sorts": [
    { "field": "created", "direction": "desc" }
  ],
  "search": {
    "term": "north",
    "fields": [ "region" ]
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

Install:

```bash
dotnet add package DataScorpio.EntityFrameworkCore
```

Register:

```csharp
services.AddDataScorpio(profiles =>
{
    profiles.AddProfile(new CustomerQueryProfile());
});

services.AddDataScorpioEntityFrameworkCore();
```

Execute with EF Core async APIs:

```csharp
using DataScorpio.EntityFrameworkCore.Execution;

var processor = serviceProvider.GetRequiredService<IEfCoreQueryProcessor>();

var result = await processor.ExecuteAsync(
    dbContext.Customers.AsNoTracking(),
    new QueryRequest
    {
        Filters = "Status==Active",
        Sorts = "-created",
        PageNumber = 1,
        PageSize = 25
    },
    cancellationToken);
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

## ASP.NET Core

Install:

```bash
dotnet add package DataScorpio.AspNetCore
```

Convert `HttpContext.Request.Query` into a `QueryRequest`:

```csharp
using DataScorpio.AspNetCore.QueryRequestBinding;

app.MapGet("/customers", async (
    HttpContext http,
    CustomerDbContext db,
    IEfCoreQueryProcessor processor,
    CancellationToken cancellationToken) =>
{
    var request = http.Request.Query.ToDataScorpioQueryRequest();
    var result = await processor.ExecuteAsync(db.Customers.AsNoTracking(), request, cancellationToken);

    return result.IsSuccess
        ? Results.Ok(result.Result)
        : Results.BadRequest(result.Validation);
});
```

Supported query keys:

```text
?filters=Status==Active&sorts=-created&pageNumber=1&pageSize=25&search=ada
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

host.Apply(new QueryRequest { Filters = "Status==Active" })
    .ShouldBeSuccessful()
    .ShouldContainOnly(customer => customer.Name == "Ada");
```

## DynaBee

`DataScorpio.DynaBee` is an opt-in acceleration boundary for future generated metadata and fast paths.

```bash
dotnet add package DataScorpio.DynaBee
```

```csharp
services.AddDataScorpioDynaBee();
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
| `services.AddDataScorpioEntityFrameworkCore()` | `DataScorpio.EntityFrameworkCore` |
| `query.ToDataScorpioQueryRequest()` | `DataScorpio.AspNetCore` |
| `services.AddDataScorpioDynaBee()` | `DataScorpio.DynaBee` |

## Sample

Run the basic in-memory sample:

```bash
dotnet run --project samples/DataScorpio.Samples.Basic/DataScorpio.Samples.Basic.csproj
```
