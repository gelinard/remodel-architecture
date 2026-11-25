# 1. Separate projects per module and per adapter

- Status: Accepted
- Date: 2026-11-24

## Context

The main promise of this repository is that business code never touches the database. If a module lives in one project, that promise depends on discipline: nothing stops a handler from adding `using Microsoft.EntityFrameworkCore;`. Folders don't enforce anything.

## Decision

Every module is split into projects with a clear dependency direction:

| Project | Contains | May reference |
|---|---|---|
| `Remodel.<Module>.Core` | queries, commands, handlers, ports, result records | `Remodel.Dispatching` only, no database packages |
| `Remodel.<Module>.Adapters.<Tech>` | one implementation of the module's ports (`EfCore`, `Dapper`, ...) | the module's Core and that one technology |

Building blocks (`Remodel.Dispatching`, and later `Remodel.Data.SqlServer`) are separate projects too. Every project name starts with `Remodel.`.

To keep many small projects cheap to maintain:

- `Directory.Build.props` holds the shared settings: `net10.0`, nullable on, implicit usings, warnings as errors, code style checked in the build.
- `Directory.Packages.props` holds every package version (central package management). A project file just says *which* packages it uses.
- `tests/Directory.Build.props` adds the test framework and assertion library to every test project.

## Consequences

- The compiler enforces the boundary. Core can't use EF Core because the package isn't referenced, so the code doesn't compile.
- Adding an adapter means adding a project. Core doesn't change.
- Each adapter brings only its own packages. A host that runs Dapper doesn't load EF Core.
- More projects: about three per module, plus tests. Central settings keep most `.csproj` files under ten lines, and a `dotnet new` module template is planned for v2.
- Handlers and adapters can stay `internal`, so each project exposes one registration method (`AddFleetCore()`, `AddFleetDapper(...)`) instead of its types.
