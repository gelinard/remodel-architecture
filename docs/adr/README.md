# Architecture decision records

Short notes on the decisions behind this repository: the context, what was decided, and what it costs.

| # | Decision | Status |
|---|----------|--------|
| [1](0001-separate-projects-per-module.md) | Separate projects per module and per adapter | Accepted |
| [2](0002-own-dispatcher-instead-of-mediatr.md) | A small own dispatcher instead of MediatR | Accepted |
| [3](0003-modules-own-their-ports.md) | Modules own their ports | Accepted |
| [4](0004-one-port-per-use-case.md) | One port per use case, no generic repositories | Accepted |
| [5](0005-contract-tests-on-a-real-database.md) | Contract tests run every adapter on a real database | Accepted |
| [6](0006-architecture-rules-as-tests.md) | Architecture rules are tests, applied to every module by convention | Accepted |
| [7](0007-test-stack.md) | Test stack: xUnit v3 on Microsoft Testing Platform, and Shouldly | Accepted |
| [8](0008-stay-on-dotnet-10-lts-packages.md) | Stay on .NET 10 LTS packages, and one SQL Server driver version | Accepted |
| [9](0009-results-for-expected-failures.md) | Expected failures are results, unexpected failures are exceptions | Accepted |
| [10](0010-small-own-validator-interface.md) | A small own validator interface instead of FluentValidation | Accepted |
| [11](0011-functions-host.md) | The Azure Functions host: configuration picks the adapter, and errors are handled in one place | Accepted |
| [12](0012-writes-call-the-legacy-procedure.md) | Writes call the legacy stored procedure in every adapter, with no shared SQL helper project yet | Accepted |
| [13](0013-new-modules-own-their-database.md) | New modules own their database: rules in C#, schema from EF Core migrations | Accepted |
| [14](0014-no-calls-between-modules-in-v1.md) | Modules don't call each other in v1 | Accepted |

New records copy the format of the existing ones: Status, Date, Context, Decision, Consequences. A record is never rewritten after it is accepted. A new record replaces it and marks it "Superseded by N".
