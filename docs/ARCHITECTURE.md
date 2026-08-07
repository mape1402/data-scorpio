# DataScorpio Architecture

This document captures the intended architecture for DataScorpio.

## Goals

- Define the public API boundary before implementation details spread across the codebase.
- Keep runtime, integration, and package concerns separated.
- Make validation, diagnostics, testing, and release behavior explicit from the start.

## Project Shape

- `src/DataScorpio` contains the primary package.
- `tests/DataScorpio.Tests` contains unit and integration tests.
- `benchmarks/DataScorpio.Benchmarks` contains performance scenarios when needed.
- `samples` contains runnable examples for supported use cases.

## Decisions

Record durable architecture decisions here as the project takes shape.

## Open Questions

- What is the first production use case?
- Which public abstractions should be stable in the first package?
- Which integrations belong in the core package and which should live in separate packages?

