# DataScorpio Roadmap

DataScorpio is the Elysium query engine.

Its first job is to replace Sieve-style query handling without breaking existing TurtlePath expectations. Its long-term job is to become a secure, typed, provider-aware, high-performance query pipeline for APIs, workers, automations, consumers, and internal service flows.

## Vision

DataScorpio should let applications describe query intent once and execute it safely across supported providers.

The library should own:

- query request models
- Sieve-compatible parsing
- native query descriptors
- typed profile configuration
- field allowlists
- operator registration
- value parsing
- validation
- diagnostics
- query expression building
- provider adapters
- compatibility adapters
- performance-oriented metadata and expression caching

The library should not be a TurtlePath feature. TurtlePath should consume DataScorpio through a thin adapter.

## Core Principles

- Deny unknown fields by default.
- Deny unknown operators by default.
- Prefer profiles over automatic property exposure.
- Keep the core package independent from ASP.NET Core, Entity Framework Core, TurtlePath, OctoMap, Crabalidator, and DynaBee.
- Keep the query model transport-neutral.
- Parse raw query strings into structured descriptors before validation or execution.
- Validate before applying expressions.
- Emit provider-safe expression trees for `IQueryable` providers.
- Use DynaBee for metadata, parser, accessor, validator, and in-memory execution optimization where it does not break provider translation.
- Preserve Sieve compatibility as a first-class migration path.

## Pipeline

The full DataScorpio pipeline should evolve toward this shape:

```text
Query Request
      |
Parsing
      |
Descriptor Normalization
      |
Validation
      |
Contextual Scopes
      |  Authorization, multi-tenancy, soft delete
      |
Filters
      |
Search
      |
Specifications / Presets
      |
Sorting
      |
Includes
      |
Projection Preparation
      |
Paging
      |
Optimization
      |  caching hints, expression cache, compiled query hints
      |
Provider Execution
```

Paging must run after sorting. TurtlePath currently owns paging separately from Sieve, so the TurtlePath adapter must preserve that behavior unless a caller explicitly asks DataScorpio to execute the full query.

## Package Plan

### Core Package

```text
DataScorpio
```

Owns:

- request and descriptor models
- parser abstractions
- native parser
- Sieve compatibility parser
- profile API
- attributes
- field binding metadata
- operators
- value parsers
- validation
- diagnostics
- expression builder abstractions
- provider-neutral query applier contracts

The core package should depend only on platform-level .NET abstractions.

### Provider Packages

```text
DataScorpio.EntityFrameworkCore
DataScorpio.AspNetCore
DataScorpio.Testing
```

Potential future packages:

```text
DataScorpio.TurtlePath
DataScorpio.OctoMap
DataScorpio.Crabalidator
DataScorpio.CId
DataScorpio.MongoDB
DataScorpio.Dapper
DataScorpio.OpenSearch
DataScorpio.Analyzers
```

If a package introduces dependency on another Elysium library, that dependency belongs in an adapter package, not in `DataScorpio`.

## TurtlePath Compatibility Target

TurtlePath currently routes string filters and sorts through `IStorageCriteriaApplier`.

Current expected shape:

```csharp
StorageReaderAdapter
    .For<TEntity>()
    .AsNoTracking()
    .Where(expression)
    .FilterBy(filters)
    .SortBy(expression)
    .SortBy(sorts)
    .Page(pageNumber, pageSize)
    .ToBatchAsync<TResponse>();
```

The first TurtlePath adapter should implement the same boundary Sieve uses today:

```csharp
public sealed class DataScorpioStorageCriteriaApplier : IStorageCriteriaApplier
{
    public IQueryable<TEntity> Apply<TEntity>(
        IQueryable<TEntity> source,
        GetManyCriteria<TEntity> criteria)
        where TEntity : class, IEntity;
}
```

Important compatibility rules:

- DataScorpio applies `criteria.Filters` and `criteria.Sorts`.
- TurtlePath keeps applying paging through `Page(...)`.
- Existing expression filters and expression sorts remain outside DataScorpio for this adapter.
- Empty filters and sorts return the source query unchanged.
- Sieve-compatible failures should be configurable: silent mode for migration, strict diagnostics for new apps.
- Existing public API fields such as `Name`, `Email`, and nested aliases must remain available when configured.

Registration goal:

```csharp
services.AddTurtlePath()
    .UseDataScorpio();
```

Optional migration mode:

```csharp
services.AddTurtlePath()
    .UseDataScorpio(options => options.UseSieveCompatibility());
```

## Sieve Compatibility Contract

Sieve compatibility is a v1 requirement, not a later convenience.

DataScorpio should provide the best practical compatibility with Sieve's filtering and sorting DSL, especially `filters`.

### Sieve Sorting

Support comma-delimited sorting:

```text
sorts=LikeCount,CommentCount,-created
```

Rules:

- Comma separates ordered sort fields.
- A leading `-` means descending.
- Later fields become `ThenBy` / `ThenByDescending`.
- Field aliases must be supported.
- Case sensitivity should be configurable.

### Sieve Filtering

Support comma-delimited filters:

```text
filters=LikeCount>10,Title@=awesome title
```

Formal shape:

```text
{Name}{Operator}{Value}
```

Rules:

- Comma separates AND filter clauses.
- `{Name}` is a configured property alias or custom filter name.
- `{Operator}` is one of the supported Sieve operators.
- `{Value}` is parsed according to the configured target field type.
- Spaces are allowed around filter clauses, except inside field names and operators.

### Sieve OR Logic

Support OR across fields:

```text
filters=(LikeCount|CommentCount)>10
```

Meaning:

```text
LikeCount > 10 OR CommentCount > 10
```

Support OR across values:

```text
filters=Title@=new|hot
```

Meaning:

```text
Title contains "new" OR Title contains "hot"
```

Support combined OR:

```text
filters=(Title|Summary)@=new|hot
```

Meaning:

```text
Title contains "new"
OR Title contains "hot"
OR Summary contains "new"
OR Summary contains "hot"
```

### Sieve Escaping

Support backslash escaping:

```text
Title@=some\,title
Title@=some\|title
Title@=\null
```

Required behavior:

- `\,` means literal comma.
- `\|` means literal pipe.
- `\null` means the literal string `"null"`, not a null value.
- Unescaped `null` may map to null depending on operator and field type.

### Sieve Operators

Support the full documented operator set:

```text
==     equals
!=     not equals
>      greater than
<      less than
>=     greater than or equal
<=     less than or equal
@=     contains
_=     starts with
_-=    ends with
!@=    does not contain
!_=    does not start with
!_-=   does not end with
@=*    case-insensitive contains
_=*    case-insensitive starts with
_-=*   case-insensitive ends with
==*    case-insensitive equals
!=*    case-insensitive not equals
!@=*   case-insensitive does not contain
!_=*   case-insensitive does not start with
```

If Sieve adds or documents additional operators, compatibility tests should be updated before release.

### Sieve Custom Filters

Sieve supports custom filter methods. DataScorpio should provide an equivalent concept through registered custom filters:

```csharp
builder.CustomFilter("IsNew", filter => ...);
```

The compatibility parser should resolve a filter name as:

1. configured field alias
2. configured custom filter
3. validation error or silent skip based on options

### Sieve Options Mapping

DataScorpio should expose compatibility options aligned with Sieve behavior:

- case-sensitive field names
- throw exceptions vs collect diagnostics
- max page size
- default page size
- ignore nulls on not-equal
- disable nullable sorting workaround if needed for provider compatibility

## Native Query Model

Raw strings should not survive past parsing.

Transport model:

```csharp
public sealed class QueryRequest
{
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
    public string Filters { get; init; }
    public string Sorts { get; init; }
    public string Search { get; init; }
}
```

Parsed model:

```csharp
public sealed class QueryDescriptor
{
    public IReadOnlyList<FilterDescriptor> Filters { get; init; }
    public IReadOnlyList<SortDescriptor> Sorts { get; init; }
    public SearchDescriptor Search { get; init; }
    public PageDescriptor Page { get; init; }
    public IReadOnlyList<QueryPresetDescriptor> Presets { get; init; }
}
```

Filter groups should support explicit logical structure:

```csharp
public enum QueryLogicalOperator
{
    And,
    Or
}
```

This allows Sieve OR syntax to map cleanly without turning the whole engine into string manipulation.

## Profile API

Profiles are the main configuration mechanism.

```csharp
public sealed class CustomerQueryProfile : QueryProfile<Customer>
{
    public override void Configure(IQueryProfileBuilder<Customer> builder)
    {
        builder
            .AllowFilter(x => x.Name)
            .AllowFilter(x => x.Email)
            .AllowFilter(x => x.Status)
            .AllowFilter(x => x.CreatedAt)
            .AllowSort(x => x.Name)
            .AllowSort(x => x.CreatedAt)
            .AllowSearch(x => x.Name)
            .AllowSearch(x => x.Email)
            .DefaultSort(x => x.Name)
            .MaxPageSize(100);
    }
}
```

Alias support:

```csharp
builder.AllowFilter("customerName", x => x.Name);
builder.AllowSort("created", x => x.CreatedAt);
```

Nested path support:

```csharp
builder.AllowFilter("creatorName", x => x.Creator.Name);
```

Per-field operator control:

```csharp
builder.AllowFilter(x => x.Name, operators => operators
    .Contains()
    .StartsWith()
    .Equals());
```

Presets:

```csharp
builder.Preset("active", query => query.Where(customer => customer.IsActive));
```

Context scopes:

```csharp
builder.ApplyScope<ICurrentTenant>((query, tenant) =>
    query.Where(customer => customer.TenantId == tenant.Id));
```

## Attribute API

Attributes are useful for small local cases. Profiles remain preferred for application-level configuration.

```csharp
public sealed class Customer
{
    [QueryableFilter("name")]
    [QueryableSearch]
    public string Name { get; set; }

    [QueryableSort("created")]
    public DateTime CreatedAt { get; set; }
}
```

Required attributes:

- `QueryableFilterAttribute`
- `QueryableSortAttribute`
- `QueryableSearchAttribute`
- `QueryableIgnoreAttribute`

Optional later attributes:

- `QueryableFieldAttribute`
- `QueryableOperatorAttribute`
- `QueryablePresetAttribute`

## Operators

Built-in native operators:

- equals
- not equals
- greater than
- greater than or equal
- less than
- less than or equal
- contains
- starts with
- ends with
- in
- not in
- between
- is null
- is not null

Operator registration:

```csharp
builder.Operator("contains", new ContainsQueryOperator());
builder.Operator("between", new BetweenQueryOperator());
```

Provider-aware operators:

```csharp
public interface IQueryOperator
{
    string Name { get; }

    QueryExpression Build(QueryOperatorContext context);
}
```

## Value Parsing

Value parsing must be typed and safe.

Required parsers:

- string
- bool
- int
- long
- decimal
- double
- DateTime
- DateOnly
- TimeOnly
- Guid
- enum
- nullable values
- arrays and collections

Optional ecosystem parser:

- CId

Registration:

```csharp
builder.ValueParser<CId>(new CIdQueryValueParser());
```

## Validation

Validation runs before execution.

Validation must cover:

- unknown filter field
- unknown sort field
- unsupported operator
- invalid value type
- invalid page size
- invalid page number
- disallowed field
- disallowed operator for field
- unsafe nested traversal
- unsupported provider translation
- incompatible custom filter signature
- malformed Sieve filter syntax
- malformed Sieve escaping

Result model:

```csharp
public sealed class QueryValidationResult
{
    public bool IsValid { get; init; }

    public IReadOnlyCollection<QueryValidationError> Errors { get; init; }
}
```

Diagnostic shape:

```csharp
public sealed class QueryValidationError
{
    public string Code { get; init; }
    public string Field { get; init; }
    public string Operator { get; init; }
    public string RawValue { get; init; }
    public string Message { get; init; }
    public QueryDiagnosticSeverity Severity { get; init; }
}
```

## Query Execution

Core abstractions:

```csharp
public interface IQueryExecutor
{
    Task<QueryResult<TResponse>> ExecuteAsync<TEntity, TResponse>(
        IQueryable<TEntity> source,
        QueryRequest request,
        CancellationToken cancellationToken = default);
}
```

Provider-neutral applier:

```csharp
public interface IQueryableQueryApplier
{
    IQueryable<TEntity> Apply<TEntity>(
        IQueryable<TEntity> source,
        QueryDescriptor descriptor);
}
```

Provider-specific packages can own async execution details such as `CountAsync`, `ToListAsync`, projection, includes, tracking behavior, and compiled query integration.

## Result Model

DataScorpio's native result model:

```csharp
public sealed class QueryResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalRows { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
```

TurtlePath adapter should map to TurtlePath's existing `PagedResponse<T>` shape without forcing TurtlePath to adopt this model immediately.

## Projection

Core should not depend on a mapper.

Projection abstraction:

```csharp
public interface IQueryProjectionAdapter
{
    IQueryable<TResponse> Project<TEntity, TResponse>(IQueryable<TEntity> source);

    Task<IReadOnlyCollection<TResponse>> MapAsync<TEntity, TResponse>(
        IReadOnlyCollection<TEntity> source,
        CancellationToken cancellationToken = default);
}
```

Optional adapter:

```csharp
services.AddDataScorpioOctoMap();
```

Projection should be designed carefully:

- EF projection should happen before materialization when possible.
- Includes may be unnecessary when projection fully selects the shape.
- TurtlePath currently maps after materialization, so migration should be incremental.

## Search

Search should be separate from filters.

Initial search:

- configured searchable fields
- text contains
- case-insensitive mode
- multiple fields with OR
- provider-safe expression generation

Later search:

- database-specific full-text search
- OpenSearch adapter
- ranking metadata
- normalized search tokens

## Includes

Includes belong in provider adapters, primarily EF Core.

Profile examples:

```csharp
builder.AllowInclude("orders", x => x.Orders);
builder.DefaultInclude(x => x.Profile);
```

Rules:

- deny arbitrary includes by default
- support aliases
- validate include paths
- skip includes when projection makes them unnecessary

## Specifications And Presets

Specifications and presets should represent named reusable query constraints.

Examples:

```csharp
builder.Preset("active", q => q.Where(x => x.IsActive));
builder.Preset("recent", args => ...);
```

Later:

- parameterized presets
- specification interfaces
- composition with authorization and tenancy scopes

## Authorization, Tenancy, And Soft Delete

These are contextual scopes, not ordinary client filters.

Rules:

- Client input cannot disable authorization filters.
- Client input cannot disable tenant filters unless the profile explicitly allows an elevated scope.
- Soft delete defaults should be profile-controlled.
- Scope filters run before client filters.
- Diagnostics should distinguish client query rejection from server-side contextual filtering.

## Query Optimization

Optimization should happen after correctness and compatibility.

Targets:

- cache parsed descriptors by raw request and profile version
- cache profile metadata by entity type
- cache expression fragments by field/operator/type/provider
- avoid reflection after startup where possible
- normalize equivalent query shapes
- expose caching hints without forcing caching into core

## DynaBee Integration

DynaBee should improve performance without making DataScorpio provider-hostile.

Good DynaBee targets:

- generated field metadata readers
- generated field accessors for in-memory evaluation
- generated value parser dispatch
- generated validator dispatch
- generated descriptor factories
- generated custom filter adapters
- precomputed profile registries
- fast in-memory execution for tests, workers, and non-EF providers

Avoid:

- replacing EF-translatable expression trees with opaque delegates
- generating provider-specific SQL in core
- leaking DynaBee abstractions into public DataScorpio APIs

DynaBee should sit behind internal or adapter-level services.

## Testing Strategy

### Core Tests

- descriptor model creation
- profile registration
- field allowlist behavior
- operator validation
- value parsing
- diagnostics
- native filter parsing
- native sort parsing

### Sieve Compatibility Tests

Create fixture groups:

- `SieveCompatibilityFilteringTests`
- `SieveCompatibilitySortingTests`
- `SieveCompatibilityOperatorTests`
- `SieveCompatibilityEscapingTests`
- `SieveCompatibilityOrLogicTests`
- `SieveCompatibilityNestedFieldTests`
- `SieveCompatibilityCustomFilterTests`
- `SieveCompatibilityOptionsTests`

Required cases:

```text
Title@=awesome title
LikeCount>10
Title@=new|hot
(LikeCount|CommentCount)>10
(Title|Summary)@=new|hot
Title@=some\,title
Title@=some\|title
Title@=\null
User.Name==specific_name
```

### TurtlePath Migration Tests

Use TurtlePath-style behavior:

- `FilterBy(filters)` applies Sieve-compatible filters.
- `SortBy(sorts)` applies Sieve-compatible sorts.
- `Page(...)` remains owned by TurtlePath.
- existing expression filters run before DataScorpio filters.
- existing expression sorts run before DataScorpio sorts.
- no filters and no sorts return source unchanged.

### Provider Tests

For EF Core:

- expressions translate to SQL where expected
- count happens before paging
- sorting is stable across multiple sort fields
- paging metadata is correct
- no client-side evaluation for supported operators
- nullable sorting behavior is configurable

## Milestones

## v0.1 - Project Foundation

Goals:

- Create solution structure.
- Add core project.
- Add test project.
- Add CI coverage.
- Define request, descriptor, validation, and diagnostics models.
- Add initial README and API docs.

Exit criteria:

- `dotnet restore`, `dotnet build`, and `dotnet test` pass.
- Core public models are documented.
- No provider dependencies in core.

## v0.2 - Profiles And Metadata

Goals:

- Add `QueryProfile<TEntity>`.
- Add `IQueryProfileBuilder<TEntity>`.
- Add field allowlists.
- Add field aliases.
- Add default sort.
- Add max page size.
- Add metadata registry.

Exit criteria:

- Unknown fields are rejected.
- Aliases resolve correctly.
- Profiles can be registered from assemblies.

## v0.3 - Sieve Compatibility Parser

Goals:

- Parse Sieve sort strings.
- Parse Sieve filter strings.
- Support Sieve operators.
- Support OR fields.
- Support OR values.
- Support escaping.
- Support nested field names.
- Support Sieve-compatible options.

Exit criteria:

- Sieve compatibility tests cover documented syntax.
- Parsed descriptors are provider-neutral.
- Malformed filters return diagnostics.

## v0.4 - Expression Builder

Goals:

- Convert validated descriptors into LINQ expressions.
- Apply filters.
- Apply sorts and then-by chains.
- Support string operators.
- Support comparison operators.
- Support null behavior.
- Support nullable values.

Exit criteria:

- Queryable tests pass against in-memory data.
- EF Core translation smoke tests pass for supported operators.

## v0.5 - Paging And Results

Goals:

- Add page descriptors.
- Add max page validation.
- Add `QueryResult<T>`.
- Add provider-neutral paging applier.

Exit criteria:

- Page metadata is correct.
- Invalid page requests are rejected.
- Paging order is deterministic when default sort exists.

## v0.6 - TurtlePath Adapter

Goals:

- Add `DataScorpio.TurtlePath`.
- Implement `IStorageCriteriaApplier`.
- Add `UseDataScorpio()` registration.
- Preserve TurtlePath paging ownership.
- Add migration tests against TurtlePath-style criteria.

Exit criteria:

- TurtlePath can replace `.UseSieve()` with `.UseDataScorpio()`.
- Existing Sieve-style TurtlePath filters and sorts work.
- No TurtlePath dependency exists in core.

## v0.7 - EF Core Adapter

Goals:

- Add `DataScorpio.EntityFrameworkCore`.
- Add async count/list execution.
- Validate provider-safe expressions.
- Add optional no-tracking support.
- Add SQL translation tests for supported operators.

Exit criteria:

- EF adapter can execute `QueryRequest` to `QueryResult<T>`.
- No supported operator causes unintended client-side evaluation.

## v0.8 - ASP.NET Core Adapter

Goals:

- Add `DataScorpio.AspNetCore`.
- Bind query parameters to `QueryRequest`.
- Support naming conventions.
- Convert validation diagnostics to HTTP-friendly error shapes.

Exit criteria:

- Minimal API and MVC examples compile.
- Invalid query responses are consistent.

## v0.9 - Testing Package

Goals:

- Add `DataScorpio.Testing`.
- Add query test host.
- Add seed data helpers.
- Add validation assertions.
- Add sorting and paging assertions.

Exit criteria:

- Consumers can test profile behavior without EF.
- Sieve compatibility can be tested with one-line helpers.

## v1.0 - Sieve Replacement Release

Scope:

- Core query request model.
- Descriptor model.
- Profile configuration.
- Field allowlists.
- Filters.
- Sorts.
- Paging.
- Default sort.
- Max page size.
- Built-in operators.
- Typed value parsing.
- Validation diagnostics.
- Sieve compatibility mode.
- TurtlePath adapter.
- EF Core adapter.
- ASP.NET Core adapter.
- Testing package.

Exit criteria:

- TurtlePath migration path is proven.
- Sieve compatibility contract is documented and tested.
- Security defaults are deny-by-default.
- README includes migration examples.
- Release notes include compatibility caveats.

## v1.1 - Search

Goals:

- Add configured search fields.
- Add multi-field search.
- Add case-insensitive search.
- Add provider-safe search expressions.

## v1.2 - Projection Preparation

Goals:

- Add projection abstraction.
- Add OctoMap adapter.
- Support projection before materialization where possible.
- Keep TurtlePath migration path compatible.

## v1.3 - Includes

Goals:

- Add include descriptors.
- Add include allowlists.
- Add EF Core include adapter.
- Skip unnecessary includes when projection is used.

## v1.4 - Presets And Specifications

Goals:

- Add named presets.
- Add parameterized presets.
- Add specification abstraction.
- Add composition rules.

## v1.5 - Contextual Filters

Goals:

- Add authorization filters.
- Add multi-tenancy scopes.
- Add soft delete scopes.
- Add scope diagnostics.

## v1.6 - Optimization

Goals:

- Add descriptor cache.
- Add expression fragment cache.
- Add profile versioning.
- Add query-shape normalization.
- Add caching hints.

## v1.7 - DynaBee Acceleration

Goals:

- Add generated metadata access.
- Add generated parser dispatch where useful.
- Add generated validator dispatch.
- Add generated in-memory execution path.
- Benchmark against reflection-based paths.

## v2.0 - Query Engine Expansion

Potential scope:

- compiled query hints
- provider plugins
- MongoDB adapter
- Dapper adapter
- OpenSearch adapter
- analyzer package
- source generator package
- OpenAPI query metadata
- query documentation generation

## Decisions

- Native DataScorpio syntax should support both JSON-first and string-first usage.
- Sieve compatibility should not be enabled by default in the TurtlePath adapter.
- TurtlePath should use strict mode for invalid Sieve-compatible queries in new DataScorpio integrations.
- DataScorpio should mirror TurtlePath's full `PagedResponse<T>` shape for smoother migration.
- DynaBee acceleration should live in a separate `DataScorpio.DynaBee` package so the core remains dependency-clean.

## Null Value Decision

Null handling should be explicit, provider-safe, and easy to validate.

For JSON/native descriptors, null should be represented as a real JSON null:

```json
{
  "field": "deletedAt",
  "operator": "eq",
  "value": null
}
```

For string syntax, DataScorpio should support explicit null operators:

```text
deletedAt is null
deletedAt is not null
```

For Sieve compatibility, DataScorpio should follow Sieve-compatible behavior:

```text
DeletedAt==null
DeletedAt!=null
```

Escaped null should remain a literal string:

```text
Name==\null
```

Recommended rules:

- `null` means null only when unescaped and used with an operator that supports null.
- `\null` means the literal string `"null"`.
- `is null` and `is not null` are preferred in native string syntax.
- `== null` and `!= null` are accepted for familiarity.
- Contains, starts-with, and ends-with should reject null values unless a custom operator explicitly allows them.

## Search Before Projection Decision

Search should be implemented before projection.

Reasons:

- Search is a direct API usefulness feature and belongs naturally beside filters and sorts.
- Search can be implemented with provider-safe expressions over configured fields.
- Search does not require choosing a mapper or projection strategy.
- Search helps TurtlePath and ASP.NET endpoints earlier than projection does.
- Projection is more complex because it interacts with includes, materialization, mapper adapters, and provider translation.

Projection should follow search once the query descriptor, validation, filtering, sorting, paging, and search pipeline are stable.

## Remaining Open Questions

- Should strict mode be the global default for all adapters or only for TurtlePath?
- Should native string syntax copy Sieve operators, use readable words, or support both?
- Should `DataScorpio.DynaBee` be opt-in globally or opt-in per profile/provider?

## Immediate Next Steps

1. Expand native JSON descriptor binding alongside the Sieve-compatible string parser.
2. Add projection descriptors and a provider-neutral projection abstraction.
3. Add include descriptors and EF Core include execution.
4. Add specification/preset execution beyond descriptor storage.
5. Add contextual scopes for authorization, multi-tenancy, and soft delete.
6. Add descriptor/profile/expression caching and benchmarking.
7. Replace the current DynaBee marker boundary with generated metadata and fast-path services.

## Current Implementation Snapshot

Completed in the first implementation pass:

- Project foundation, CI/release scaffolding, and package metadata.
- Core request, descriptor, result, validation, and diagnostic models.
- Profile builder with field allowlists, aliases, default sort, max page size, and search fields.
- Sieve-compatible filtering/sorting parser with OR groups, escaping, null handling, and operator coverage.
- Descriptor validation against configured profile metadata.
- `IQueryable` execution for filters, search, sorts, default sort, and paging.
- End-to-end core query processor and dependency injection registration.
- EF Core async query processor package.
- ASP.NET Core query binding package.
- TurtlePath `IStorageCriteriaApplier` adapter package.
- Testing helper package.
- Separate DynaBee acceleration package boundary.

Still pending from the full long-term roadmap:

- Native JSON-first descriptor binder.
- Projection execution and OctoMap adapter.
- Includes and EF Core include allowlists.
- Executable specifications and parameterized presets.
- Authorization, tenant, and soft-delete scopes.
- Query optimization, caching hints, compiled query hints, and DynaBee-generated fast paths.
