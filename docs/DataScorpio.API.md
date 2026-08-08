# DataScorpio API Reference

This document should describe the public DataScorpio API as it is implemented.

## Packages

- `DataScorpio`
- `DataScorpio.Testing`
- `DataScorpio.Testing.Sqlite`

## Public Surface

- `QueryRequest` captures raw incoming filters, sorts, search, and paging.
- `QueryDescriptor` is the parsed provider-neutral query model.
- `SieveQueryParser` parses Sieve-compatible filter and sort strings.
- `JsonQueryDescriptorParser` parses native JSON descriptors into `QueryDescriptor`.
- `QueryProfile<TEntity>` and `IQueryProfileBuilder<TEntity>` define allowed query fields.
- `AllowInclude(...)` exposes provider include paths through profile allowlists.
- `QueryDescriptorValidator` rejects unknown fields, unknown includes, unsupported operators, and invalid paging.
- `QueryableQueryApplier` applies validated descriptors to `IQueryable<TEntity>`.
- `QueryProcessor` parses, validates, applies, and returns `QueryExecutionResult<TEntity>`.
- `AddDataScorpio(...)` registers core parser, validator, applier, registry, and processor services.
- `QueryTestHost<TEntity>`, `IDataScorpioTesting<TEntity>`, and `QueryResultAssertions` support consumer tests.
- `IDataScorpioSqliteTesting<TEntity>` supports SQLite-backed provider tests.

## Examples

### Core

```csharp
var descriptor = parser.Parse(new QueryRequest
{
    Filters = "Name@=*ada,Status==Active",
    Sorts = "-CreatedAt",
    Search = "north",
    PageNumber = 1,
    PageSize = 25
});

var result = processor.Execute(customers.AsQueryable(), descriptor, profile);
```

### Dependency Injection

```csharp
services.AddDataScorpio(registry =>
{
    registry.AddProfile<CustomerQueryProfile>();
});
```

### Native JSON

```json
{
  "filters": [
    { "field": "Status", "operator": "equals", "value": "Active" }
  ],
  "includes": [ "orders" ],
  "sorts": [
    { "field": "CreatedAt", "direction": "desc" }
  ],
  "page": { "pageNumber": 1, "pageSize": 25 }
}
```
