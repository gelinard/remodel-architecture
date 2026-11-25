# 8. Stay on .NET 10 LTS packages, and one SQL Server driver version

- Status: Accepted
- Date: 2026-11-24

## Context

The project targets .NET 10, the current long-term support release (supported until November 2028). The .NET 11 packages are now out, but EF Core 11 targets `net11.0` only. EF Core 10 is built against `Microsoft.Data.SqlClient` 6.1, while SqlClient 7 is already available.

The contract test project references both the EF Core and the Dapper adapter. If the Dapper adapter asked for SqlClient 7, NuGet would give that version to EF Core too. EF Core would then run on a driver version it was not built and tested with.

## Decision

- Target `net10.0`. Use the latest `10.0.x` patch of the `Microsoft.EntityFrameworkCore.*` and `Microsoft.Extensions.*` packages.
- Pin `Microsoft.Data.SqlClient` to the latest 6.1.x patch, the line that EF Core 10 uses, so both adapters run on the same driver.
- Every version lives in `Directory.Packages.props`. Before a package is added or upgraded, its current version and licence are checked on nuget.org.

## Consequences

- A stable, supported base, with one driver version in the whole solution.
- Newer EF Core and SqlClient features have to wait. The versions are revisited at the next LTS release, or when a security fix needs it.
