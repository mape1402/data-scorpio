# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

- Initial repository structure copied from the OctoMap working setup.
- GitHub CI and NuGet release workflow scaffolding.
- Shared build metadata, package settings, documentation placeholders, and standard source, test, benchmark, and sample folders.
- Core query request, descriptor, result, validation, and diagnostic models.
- Typed query profile metadata with field allowlists, aliases, default sort, and max page size.
- Sieve-compatible filter and sort parser with OR groups, escaping, null handling, and documented operator coverage.
- Native JSON descriptor parser for JSON-first query payloads.
- Query descriptor validation against profile metadata.
- `IQueryable` descriptor applier for filters, sorts, default sort, and paging.
- Configured multi-field search execution.
- Allowlisted include metadata, validation, JSON parsing, and EF Core execution.
- End-to-end query processor and dependency injection registration.
- Entity Framework Core async query processor package.
- ASP.NET Core query collection binding package.
- DataScorpio testing helper package.
- TurtlePath integration direction documented as TurtlePath-owned, with DataScorpio exposing reusable core APIs.
- Separate DynaBee acceleration package boundary.
