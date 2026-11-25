# 12. Writes call the legacy stored procedure in every adapter, with no shared SQL helper project yet

- Status: Accepted
- Date: 2026-11-25

## Context

The rules for returning a car (rental must exist, not already returned, km in ≥ km out, return after pickup) live in `dbo.CarReturn`, and other legacy applications use the same procedure. Copying these rules into LINQ would create two versions of the truth.

The original plan had a `Remodel.Data.SqlServer` building block with a stored procedure helper for `OUTPUT` parameters and the `@ErrMsg` convention.

## Decision

- Both the Dapper and the EF Core adapter call `dbo.CarReturn`. They map its return code to a `Result` (0 = success, 1 = not found, 2 = conflict, other = rejected) and use `@ErrMsg` as the message.
- Each adapter uses its library's own support for `OUTPUT` parameters: `DynamicParameters` in Dapper, `SqlParameter` with `ExecuteSqlRawAsync` in EF Core. Each takes about ten lines that are easy to read.
- `Remodel.Data.SqlServer` is not created for now. It will be added when the same code appears in a third place.
- Reads can still be written again in LINQ (as in the EF Core readers), because the contract tests compare them with the procedures.

## Consequences

- The legacy business rules stay in one place, and both adapters behave the same. The contract tests check every return code against both adapters.
- The small return-code mapping exists in both adapters. The contract tests keep the two copies in line.
- Fewer projects for readers to learn. The README's project structure still lists `Remodel.Data.SqlServer` and will be updated in Stage 4.
