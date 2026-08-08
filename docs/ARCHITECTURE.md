# DataScorpio Architecture

This document captures the intended architecture for DataScorpio.

## Goals

- Define the public API boundary before implementation details spread across the codebase.
- Keep runtime, integration, and package concerns separated.
- Make validation, diagnostics, testing, and release behavior explicit from the start.

## Project Shape

- `src/DataScorpio` contains the primary package.
- `src/DataScorpio.EntityFrameworkCore` contains provider-specific async EF Core execution.
- `src/DataScorpio.AspNetCore` contains query string binding helpers.
- `src/DataScorpio.Testing` contains reusable consumer test helpers.
- `src/DataScorpio.DynaBee` contains the opt-in DynaBee acceleration boundary.
- `tests/DataScorpio.Tests` contains core unit and integration tests.
- `benchmarks/DataScorpio.Benchmarks` contains performance scenarios when needed.
- `samples` contains runnable examples for supported use cases.

## Decisions

- Core remains independent from ASP.NET Core, EF Core, TurtlePath, OctoMap, Crabalidator, and DynaBee.
- Sieve compatibility is implemented as a parser/migration mode, not as the native internal model.
- TurtlePath integration is TurtlePath-owned; TurtlePath should reference DataScorpio, not the other way around.
- Search is implemented before projection because it is immediately useful and provider-safe with configured fields.
- DynaBee lives in a separate package so acceleration can evolve without coupling the core public API.

## Open Questions

- How far should native string syntax diverge from Sieve once JSON descriptors are stable?
- Which projection adapter should ship first, OctoMap or a provider-neutral projection contract only?
- Should contextual scopes be profile-only, service-driven, or both?
