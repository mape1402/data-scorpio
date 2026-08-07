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
- Query descriptor validation against profile metadata.
- `IQueryable` descriptor applier for filters, sorts, default sort, and paging.
