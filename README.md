# DataScorpio

[![Build](https://github.com/mape1402/data-scorpio/actions/workflows/CI.yml/badge.svg)](https://github.com/mape1402/data-scorpio/actions/workflows/CI.yml)
[![NuGet](https://img.shields.io/nuget/v/DataScorpio.svg)](https://www.nuget.org/packages/DataScorpio)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**DataScorpio** is an Elysium-owned .NET query engine for safe, typed, provider-aware filtering, sorting, paging, searching, validation, and query execution over `IQueryable` sources.

The first milestone is a strong Sieve-compatible migration path for TurtlePath and existing APIs. DataScorpio parses raw query strings into structured descriptors, validates them against explicit query profiles, and applies provider-friendly expression trees.

## Status

DataScorpio is in active implementation.

Implemented foundation:

- Typed query request and descriptor models.
- TurtlePath-compatible query result metadata.
- Query profiles with field allowlists, aliases, default sort, and max page size.
- Sieve-compatible filter and sort parser.
- Native JSON descriptor parser.
- Query descriptor validation.
- `IQueryable` filter, sort, search, default sort, and paging applier.
- End-to-end query processor with validation diagnostics.
- Entity Framework Core async execution adapter with allowlisted includes.
- ASP.NET Core query collection binding adapter.
- TurtlePath-ready core APIs so TurtlePath can own its DataScorpio integration.
- Testing helpers for query behavior assertions.
- Opt-in `DataScorpio.DynaBee` package boundary for acceleration work.

Planned next layers include projection, specifications, authorization filters, multi-tenancy, soft delete scopes, richer optimization, caching hints, and compiled query support.

## Requirements

- .NET SDK 10.0+ recommended for development.
- The project is prepared for `net8.0`, `net9.0`, and `net10.0` targets.

## Repository Layout

- `src/` contains production projects.
- `tests/` contains automated tests.
- `benchmarks/` contains BenchmarkDotNet scenarios.
- `samples/` contains runnable examples.
- `docs/` contains architecture, API, roadmap, and release notes.
- `.github/workflows/` contains CI and release automation.

## Development

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```

## Samples

Run the basic in-memory sample:

```bash
dotnet run --project samples/DataScorpio.Samples.Basic/DataScorpio.Samples.Basic.csproj
```

## Example

```csharp
var profile = new CustomerQueryProfile().BuildDefinition();
var parser = new SieveQueryParser();
var applier = new QueryableQueryApplier();

var descriptor = parser.Parse(new QueryRequest
{
    Filters = "Name@=*ada,Status==Active",
    Sorts = "-CreatedAt",
    PageNumber = 1,
    PageSize = 25
});

var query = applier.Apply(customers.AsQueryable(), descriptor, profile);
```

## Packages

- `DataScorpio`: core descriptors, profiles, parser, validator, query processor, and `IQueryable` applier.
- `DataScorpio.EntityFrameworkCore`: EF Core async query execution and include application.
- `DataScorpio.AspNetCore`: HTTP query string binding into `QueryRequest`.
- `DataScorpio.Testing`: consumer-facing test helpers.
- `DataScorpio.DynaBee`: separate opt-in acceleration boundary.

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [API Reference](docs/DataScorpio.API.md)
- [Roadmap](docs/ROADMAP.md)
- [Release Process](docs/RELEASE.md)
