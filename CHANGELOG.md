# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2026-08-11

### Added

- `QueryCriteriaResult<T>` and `ApplyCriteria(...)` APIs to expose whether a criteria application remains async provider-backed or has been materialized.
- EF SQLite coverage for provider-preserving filters, sorts, aliases, global convention filters, materialized custom filters, and downstream paging consumers.

## [1.0.0] - 2026-08-09

### Added

- Initial `DataScorpio` core package.
- String-first `QueryRequest` model for filters, sorts, search, page number, and page size.
- Native `QueryDescriptor` model for structured query execution.
- Sieve-compatible filter and sort string parser.
- Filter operators for equality, inequality, comparisons, contains, starts with, ends with, and their supported case-insensitive or negated variants.
- OR field groups and OR value groups in string filters.
- Explicit null handling in string filters and native JSON descriptors.
- Native JSON descriptor parser for filters, filter groups, sorts, search, includes, paging, and presets.
- Typed `QueryProfile<TEntity>` configuration with filter, sort, search, include, alias, default sort, and max page size support.
- Assembly discovery for query profiles and convention sets.
- Custom filters and custom sorts over `IQueryable<T>`.
- Contract-based reusable custom filters and custom sorts through query convention sets.
- Query descriptor validation with structured diagnostics for unknown fields, unsupported operations, includes, and paging errors.
- Provider-friendly `IQueryable<T>` query application for filtering, sorting, search, paging, and total row counts.
- `IQueryProcessor` execution pipeline with success and rejected result models.
- `QueryResult<T>` paging metadata with `Items`, `Results`, `PageNumber`, `CurrentPage`, `PageSize`, `RowCount`, `TotalRows`, `PageCount`, `TotalPages`, `HasPreviousPage`, and `HasNextPage`.
- Dependency injection registration through `AddDataScorpio(...)`.
- Compatibility registration through `AddDataScorpioSieveCompatibility(...)`.
- `DataScorpio.Testing` package with in-memory query testing helpers.
- `QueryTestHost<TEntity>` for lightweight profile tests.
- `IDataScorpioTesting<TEntity>` with async seeding and query execution helpers.
- Query assertions for success, rejection, validation codes, filtering, sorting, and paging.
- `DataScorpio.Testing.Sqlite` package for SQLite-backed provider behavior tests.
- Expanded basic sample covering filtering, sorting, paging, search, OR filters, aliases, custom conventions, null filters, native JSON, and validation.
- Mule-style build and release workflow, NuGet trusted publishing setup, shared build metadata, test projects, sample project, and repository documentation.
