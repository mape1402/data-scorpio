# DataScorpio

[![Build](https://github.com/mape1402/data-scorpio/actions/workflows/CI.yml/badge.svg)](https://github.com/mape1402/data-scorpio/actions/workflows/CI.yml)
[![NuGet](https://img.shields.io/nuget/v/DataScorpio.svg)](https://www.nuget.org/packages/DataScorpio)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**DataScorpio** is a .NET project initialized from the OctoMap working structure.

This repository starts with the shared project scaffolding used across Elysium Coding packages: GitHub workflows, release checklist, changelog, package metadata, documentation placeholders, test and benchmark folders, and common build settings.

## Status

DataScorpio is in initial project setup.

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

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [API Reference](docs/DataScorpio.API.md)
- [Roadmap](docs/ROADMAP.md)
- [Release Process](docs/RELEASE.md)

