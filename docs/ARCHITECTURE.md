# DataScorpio Architecture

This document captures the intended architecture for DataScorpio.

## Goals

- Define the public API boundary before implementation details spread across the codebase.
- Keep runtime, integration, and package concerns separated.
- Make validation, diagnostics, testing, and release behavior explicit from the start.

## Project Shape

- `src/DataScorpio` contains the primary package.
- `src/DataScorpio.Testing` contains reusable consumer test helpers.
- `src/DataScorpio.Testing.Sqlite` contains SQLite-backed provider testing helpers.
- `tests/DataScorpio.Tests` contains core unit and integration tests.
- `benchmarks/DataScorpio.Benchmarks` contains performance scenarios when needed.
- `samples` contains runnable examples for supported use cases.

## Decisions

- Core remains independent from ASP.NET Core, Entity Framework Core, TurtlePath, OctoMap, Crabalidator, and DynaBee.
- Sieve compatibility is implemented as a parser/migration mode, not as the native internal model.
- TurtlePath integration is TurtlePath-owned; TurtlePath should reference DataScorpio, not the other way around.
- DataScorpio works over `IQueryable<T>` directly instead of shipping ORM-specific query packages by default.
- Search is implemented before projection because it is immediately useful and provider-safe with configured fields.
- DynaBee integration should only ship when it provides real generated metadata or fast-path behavior.

## Open Questions

- How far should native string syntax diverge from Sieve once JSON descriptors are stable?
- Which projection adapter should ship first, OctoMap or a provider-neutral projection contract only?
- Should contextual scopes be profile-only, service-driven, or both?
