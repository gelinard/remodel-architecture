# 7. Test stack: xUnit v3 on Microsoft Testing Platform, and Shouldly

- Status: Accepted
- Date: 2026-11-24

## Context

The test projects need a test framework and an assertion library. xUnit's current major version (4.x of `xunit.v3`) runs on Microsoft Testing Platform (MTP) by default. The .NET 10 SDK supports MTP in `dotnet test` when `global.json` asks for it. For assertions, FluentAssertions moved from Apache-2.0 to a custom commercial licence in version 8, which leaves Shouldly and AwesomeAssertions as the open source options.

## Decision

- **xUnit** `xunit.v3` 4.x on MTP, enabled in `global.json` (`"test": { "runner": "Microsoft.Testing.Platform" }`). No `Microsoft.NET.Test.Sdk` or `xunit.runner.visualstudio`.
- **Shouldly** (BSD-3-Clause) for assertions. It has a small API, and its failure messages include the code that failed. The README samples already use it.
- `tests/Directory.Build.props` adds xUnit and Shouldly, and their global usings, to every test project.
- Tests pass `TestContext.Current.CancellationToken` to async calls, so a cancelled test run stops quickly. The xUnit analyzers check this.
- NSubstitute is not added until a test needs a fake.

## Consequences

- Two fewer packages. Test projects are normal executables that can also run on their own.
- `dotnet test` needs the .NET 10 SDK, and IDEs need MTP support to discover the tests.
- AwesomeAssertions (Apache-2.0) would also have worked. The choice is mostly about style.
