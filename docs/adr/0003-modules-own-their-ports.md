# 3. Modules own their ports

- Status: Accepted
- Date: 2026-11-24

## Context

The business code of a module must not care *how* its data is fetched: EF Core, Dapper or a stored procedure. Only then can the module move to a new database later without changing its handlers. The legacy schema is also hard to read (`tbl_Rental.dtPickup`, `DelFlag = 'Y'`), and that knowledge should not spread into business code.

## Decision

- The module's Core declares each piece of data it needs as an interface, a **port**, in the use case's folder: `Features/GetCarHistory/ICarHistoryReader.cs`.
- Ports return records owned by the module (`CarHistoryItem`), take a `CancellationToken`, and return `null` or an empty list for "not found" instead of throwing.
- **Adapters** implement the ports in their own projects. Their classes are `internal`, and each adapter exposes one registration method: `AddFleetDapper(connectionString)` or `AddFleetEfCore(connectionString)`.
- Adapters translate the legacy schema into the module's records. EF Core row classes (`RentalRow`) are internal and never leave the adapter.
- The Dapper adapter gets its connections from a typed class (`LegacyFleetDb`) instead of a shared connection factory, so modules on different databases can't mix up their connection strings.

## Consequences

- Core can be unit tested without a database and is checked by the architecture tests (no data access namespaces allowed).
- The host chooses the adapter with one line. Handlers don't change.
- This is the path for the strangler fig: give a module a new adapter for a new database, and the contract tests show when it behaves like the old one.
- Rules that live in stored procedures (for example "hide deleted rentals") must be repeated in LINQ adapters. The contract tests catch it when they are not.
