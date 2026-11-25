# 6. Architecture rules are tests, applied to every module by convention

- Status: Accepted
- Date: 2026-11-24

## Context

Project references already stop Core from using EF Core or an adapter. Other rules can't be enforced by the compiler: naming, "query handlers never use writers", "handlers are internal", "no repositories". Rules written only in a wiki are forgotten. Rules that must be added again for every new module are also forgotten.

## Decision

- One project, `Remodel.ArchitectureTests`, checks every module. It finds modules by convention: wildcard project references to `src/Modules/*/Remodel.*.Core` and `Remodel.*.Adapters.*`, then every `Remodel.{Module}.Core.dll` in the output folder. Each rule is a `[Theory]` with one case per module.
- **ArchUnitNET** checks dependency rules, because it reads the compiled code and sees dependencies inside method bodies. **Plain reflection** checks shape rules (names, generic ports, method signatures), where it is simpler.
- A small `Check` helper turns rule violations into a test failure with the full list. The `TngTech.ArchUnitNET.xUnitV3` package does the same, but it is built against a much older xUnit assertion package.
- Rules are checked against deliberate mistakes:
  - A canary test applies the "no data access libraries" rule to the adapters and expects it to fail. That proves the rule can see those dependencies.
  - A naming test checks that anything implementing `IQueryHandler<,>` is named `...QueryHandler` (and so on), because other rules find types by name.
- "Modules don't use each other" is skipped while there is only one module, and starts checking when the second one arrives.

## Consequences

- A new module is checked by every rule with no new test code and no change to the test project.
- Each rule was seen failing on a deliberate violation before it was trusted.
- Name-based rules depend on conventions. The naming test reduces that risk.
