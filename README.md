<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/assets/logo-dark.svg">
    <img alt="Remodel Architecture for .NET" src="docs/assets/logo-light.svg" width="380">
  </picture>
</p>

<p align="center">
  <strong>Modernize legacy .NET + SQL Server systems one module at a time,<br>
  without rewriting the database you can't change.</strong>
</p>

<p align="center">
  <a href="https://github.com/gelinard/remodel-architecture/actions/workflows/ci.yml"><img alt="CI" src="https://github.com/gelinard/remodel-architecture/actions/workflows/ci.yml/badge.svg"></a>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512BD4">
  <img alt="Azure Functions isolated worker" src="https://img.shields.io/badge/Azure%20Functions-isolated%20worker-0062AD">
  <img alt="EF Core and Dapper" src="https://img.shields.io/badge/data-EF%20Core%20%2B%20Dapper-6A4C93">
  <img alt="SQL Server and PostgreSQL" src="https://img.shields.io/badge/databases-SQL%20Server%20%2B%20PostgreSQL-336791">
  <img alt="License: MIT" src="https://img.shields.io/badge/license-MIT-green">
  <img alt="Status: v1 preview" src="https://img.shields.io/badge/status-v1%20preview-orange">
</p>

<p align="center">
  <sub>Ports &amp; Adapters · Strangler Fig · Modular Monolith · Vertical Slices · CQRS · Contract Tests · Architecture Tests</sub>
</p>

<p align="center">
  <a href="#quick-start">Quick start</a> ·
  <a href="#how-a-module-works">How it works</a> ·
  <a href="#patterns-inside">Patterns inside</a> ·
  <a href="#modernize-your-own-system">Use it on your system</a> ·
  <a href="docs/adr/README.md">Design decisions</a>
</p>

---

**Remodel Architecture for .NET** is a reference architecture and a working template for one of the most common jobs in .NET: keeping a legacy system alive while you modernize it. Business code moves into clean, tested modules. The SQL Server database, the stored procedures and the other applications that use them stay exactly as they are, until each module is ready to move.

> **Clean modules. Legacy database. No big-bang rewrite.**

## The problem this solves

You inherited a system like this:

- A SQL Server database that is years old, with hundreds of stored procedures.
- Other applications, reports and jobs depend on it, so you can't change the schema freely.
- Business logic is spread across controllers, stored procedures and shared helper classes.
- Every change feels risky, and the "big rewrite" never gets approved.

Most architecture templates don't help here. They start from an empty, code-first database and assume you own the schema. **Remodel Architecture starts from the database you already have.**

## The idea

You remodel a house while you still live in it: one room at a time, and the foundation stays. Remodel Architecture does the same for software.

```text
  1. Today                   2. First module                3. Old and new, side by side

  ┌─────────────────┐        ┌─────────┐ ┌─────────┐        ┌────────────┐  ┌────────────┐
  │                 │        │  Fleet  │ │  legacy │        │   Fleet    │  │  Bookings  │
  │    legacy app   │  ───►  │  module │ │   code  │  ───►  │   module   │  │   module   │
  │                 │        └────┬────┘ └────┬────┘        └──────┬─────┘  └──────┬─────┘
  └────────┬────────┘             └─────┬─────┘                    │               │
           │                            │                          │               │
    ┌──────┴──────┐              ┌──────┴──────┐            ┌──────┴─────┐  ┌──────┴─────┐
    │  SQL Server │              │  SQL Server │            │ SQL Server │  │ PostgreSQL │
    └─────────────┘              └─────────────┘            └────────────┘  └────────────┘
```

1. **Wrap one business area in a module.** It gets clean, testable code and owns its use cases.
2. **The module declares the data it needs as small interfaces, called ports.** It never references a database library.
3. **Adapters implement the ports** with the database you already have: EF Core, Dapper or your stored procedures.
4. **Move when you're ready.** Give the module a new adapter on a new database. The same contract tests show that it behaves exactly like the old one.

Every step ships on its own. Nothing waits for a rewrite.

## What you get

- **Business code that never touches the database.** A module's Core has no reference to EF Core, Dapper or SqlClient. The compiler and the architecture tests guarantee it.
- **EF Core or Dapper, chosen with one setting.** Both adapters implement the same ports, and the host picks one from configuration. Business code doesn't change.
- **Proof that adapters behave the same.** One contract test suite runs against every adapter, on a real database in Docker.
- **Stored procedures as first-class citizens,** including `OUTPUT` parameters and the legacy "error message" convention.
- **Old and new side by side.** A legacy module on SQL Server and a new code-first module on PostgreSQL, in the same host.
- **One command, two triggers.** The same handler runs from an HTTP request and from a queue message.
- **Errors handled in one place.** Expected failures are results, unexpected ones are exceptions, and every HTTP error is a [Problem Details](https://www.rfc-editor.org/rfc/rfc9457) response.
- **Architecture rules as tests.** Break a boundary and the build goes red. A new module is checked automatically.
- **No commercial dependencies.** A small dispatcher you can read in five minutes replaces MediatR. Every package is MIT, Apache-2.0, BSD or PostgreSQL licensed.

## The six rules

| # | Rule | In one line |
|---|------|-------------|
| 1 | **Modules own their ports** | A module declares the data it needs as interfaces and never references a database library. |
| 2 | **Adapters plug in** | Each port has one or more adapters (EF Core, Dapper, stored procedures). The host picks which one runs. |
| 3 | **One port per use case** | `…Reader` ports serve queries, `…Writer` ports serve commands. No generic repositories. |
| 4 | **Prove every adapter** | One shared contract test suite runs against every adapter on a real database. |
| 5 | **Rules are tests** | Boundaries are enforced by the compiler and by architecture tests, not by a wiki page. |
| 6 | **Strangle, don't rewrite** | Legacy and new modules live side by side. You move one module at a time. |

## How a module works

One request, from the trigger down to the legacy database:

```text
       HTTP request          Queue message
             │                     │
             └──────────┬──────────┘
                        ▼
                   IDispatcher                    one entry point for every trigger
                        │
              logging → validation                pipeline behaviors for every message
                        │
            GetCarHistoryQueryHandler             the use case: business code, no SQL
                        │
                        ▼
                ICarHistoryReader                 the port: Fleet.Core owns it
                        ▲
             ┌──────────┴──────────┐
             │                     │
      Dapper adapter        EF Core adapter       the host picks one in configuration
     stored procedure       LINQ on tables
             │                     │
             └──────────┬──────────┘
                        ▼
                legacy SQL Server                 the database you keep
```

The sample is a small car rental business with two modules:

| | **Fleet** (legacy) | **Bookings** (new) |
|---|---|---|
| **Database** | SQL Server, an existing schema (`tbl_Rental`, `dtPickup`, `DelFlag`…) | PostgreSQL, owned by the module |
| **Schema comes from** | SQL scripts nobody may change ([`db/fleet-legacy`](db/fleet-legacy)) | EF Core code-first migrations |
| **Business rules live in** | stored procedures | C# handlers |
| **Adapters** | Dapper (stored procedures) **and** EF Core (LINQ), picked by configuration | EF Core |
| **Contract tests** | the same suite on both adapters, real SQL Server | real PostgreSQL, built by the real migrations |

## One use case, end to end

*"Show the rental history of a car."*

**1. The module declares what it needs.** The port lives next to the use case, in the module's Core.

```csharp
// Fleet.Core/Features/GetCarHistory/GetCarHistoryQuery.cs
public sealed record GetCarHistoryQuery(int CarId) : IQuery<IReadOnlyList<CarHistoryItem>>;

public sealed record CarHistoryItem(int RentalId, string Customer, DateTime PickedUp, DateTime? Returned);

// Fleet.Core/Features/GetCarHistory/ICarHistoryReader.cs  ← the port
public interface ICarHistoryReader
{
    Task<IReadOnlyList<CarHistoryItem>> Read(int carId, CancellationToken ct);
}
```

**2. The handler only knows the port.**

```csharp
internal sealed class GetCarHistoryQueryHandler(ICarHistoryReader reader)
    : IQueryHandler<GetCarHistoryQuery, IReadOnlyList<CarHistoryItem>>
{
    public Task<IReadOnlyList<CarHistoryItem>> Handle(GetCarHistoryQuery query, CancellationToken ct)
        => reader.Read(query.CarId, ct);
}
```

**3. Two adapters plug into the same port.** They translate the legacy schema into the module's clean records.

```csharp
// Fleet.Adapters.Dapper: calls the existing stored procedure
internal sealed class CarHistoryReader(LegacyFleetDb db) : ICarHistoryReader
{
    public async Task<IReadOnlyList<CarHistoryItem>> Read(int carId, CancellationToken ct)
    {
        await using var connection = await db.OpenAsync(ct);
        var rows = await connection.QueryAsync<CarHistoryItem>(new CommandDefinition(
            "dbo.CarHistoryGet", new { CarId = carId },
            commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }
}

// Fleet.Adapters.EfCore: the same result with LINQ over the legacy tables
internal sealed class CarHistoryReader(IDbContextFactory<FleetDbContext> factory) : ICarHistoryReader
{
    public async Task<IReadOnlyList<CarHistoryItem>> Read(int carId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Rentals
            .Where(r => r.CarId == carId && r.DelFlag == "N") // same rule as the stored procedure
            .OrderByDescending(r => r.PickedUp)
            .ThenByDescending(r => r.Id)
            .Select(r => new CarHistoryItem(
                r.Id, r.Customer.FirstName + " " + r.Customer.LastName, r.PickedUp, r.Returned))
            .ToListAsync(ct);
    }
}
```

**4. The host picks the adapter from configuration,** and functions only talk to the dispatcher.

```csharp
// FleetModule.cs: "Fleet:Adapter" is "Dapper" or "EfCore"
services.AddFleetCore();
return configuration["Fleet:Adapter"] switch
{
    "Dapper" => services.AddFleetDapper(connectionString),
    "EfCore" => services.AddFleetEfCore(connectionString),
    var other => throw new InvalidOperationException($"Fleet:Adapter must be \"Dapper\" or \"EfCore\", not \"{other}\"."),
};

// FleetQueryFunctions.cs
[Function("GetCarHistory")]
public async Task<IActionResult> GetCarHistory(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fleet/cars/{carId:int}/history")] HttpRequest request,
    int carId,
    CancellationToken ct)
    => new OkObjectResult(await dispatcher.Query(new GetCarHistoryQuery(carId), ct));
```

**5. One test suite proves both adapters,** on a real SQL Server in Docker. This test exists because "deleted rentals are hidden" is a rule that only lives inside the legacy stored procedure. If the EF Core adapter forgets it, this test fails for EF Core only.

```csharp
public abstract class CarHistoryReaderContract(LegacyFleetDatabase database, Action<IServiceCollection, string> addAdapter)
    : AdapterContract(database, addAdapter)
{
    [Fact]
    public async Task Hides_deleted_rentals()
    {
        var car = await Database.AddCar();
        var customer = await Database.AddCustomer("Per", "Vik");
        var kept = await Database.AddRental(car, customer, At(2026, 8, 1));
        await Database.AddRental(car, customer, At(2026, 8, 2), deleted: true);

        var history = await Reader.Read(car, Ct);

        history.ShouldHaveSingleItem().RentalId.ShouldBe(kept);
    }
}

// The same contract, once per adapter. A new adapter is one more line.
public sealed class DapperCarHistoryReaderTests(LegacyFleetDatabase database)
    : CarHistoryReaderContract(database, (services, connectionString) => services.AddFleetDapper(connectionString));

public sealed class EfCoreCarHistoryReaderTests(LegacyFleetDatabase database)
    : CarHistoryReaderContract(database, (services, connectionString) => services.AddFleetEfCore(connectionString));
```

**6. And the rules are tests too.** Each rule runs once per module, and modules are found by convention.

```csharp
[Theory]
[MemberData(nameof(Modules.All), MemberType = typeof(Modules))]
public void Query_handlers_never_use_writers(string module) =>
    Classes().That().ResideInAssembly(Modules.Core(module)).And().HaveNameEndingWith("QueryHandler")
        .Should().NotDependOnAnyTypesThat().HaveNameEndingWith("Writer")
        .WithoutRequiringPositiveResults()
        .Check(Modules.Architecture);
```

### One command, two triggers

The same `ReturnCarCommand` runs from HTTP and from a queue. Validation, logging and the handler are shared.

```csharp
[Function("ReturnCar")]
public async Task<IActionResult> FromHttp(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fleet/rentals/{rentalId:int}/return")] HttpRequest request,
    int rentalId,
    CancellationToken ct)
{
    var body = await JsonSerializer.DeserializeAsync<ReturnCarRequest>(request.Body, JsonSerializerOptions.Web, ct)
        ?? throw new ValidationException([new ValidationError("body", "A JSON body is required.")]);

    var result = await dispatcher.Send(new ReturnCarCommand(rentalId, body.KmIn, body.ReturnedAt), ct);
    return result.ToActionResult(); // 204, or a Problem Details response
}

[Function("ReturnCarFromQueue")]
public async Task FromQueue([QueueTrigger("fleet-car-returns")] ReturnCarCommand command, CancellationToken ct)
    => await dispatcher.Send(command, ct);
```

Expected failures, such as "the car has already been returned", come back as a `Result`. Over HTTP they become a 404, 409 or 422. On the queue the message completes, because retrying won't change the answer. Unexpected errors stay exceptions: a 500 over HTTP, and retries and then a poison queue for messages.

## Patterns inside

Remodel Architecture doesn't invent new building blocks. It combines proven patterns and applies them to one situation: a system that sits on a database you can't replace.

| Pattern | Where you'll find it |
|---------|----------------------|
| [Ports and Adapters (Hexagonal)](https://alistair.cockburn.us/hexagonal-architecture/) | `I…Reader` / `I…Writer` ports in each `Core`, implementations in `Adapters.*` |
| [Strangler Fig](https://martinfowler.com/bliki/StranglerFigApplication.html) | Fleet (legacy) and Bookings (new) running side by side in one host |
| [Modular Monolith](https://github.com/kgrzybek/modular-monolith-with-ddd) | One deployable, independent modules, boundaries enforced by tests |
| [Vertical Slices](https://www.jimmybogard.com/vertical-slice-architecture/) | `Features/<UseCase>/`: query or command, port, handler and validator together |
| [CQRS](https://martinfowler.com/bliki/CQRS.html), the light version | Separate queries and commands, and separate `Reader` and `Writer` ports, on one database per module |
| Mediator with a pipeline | `IDispatcher` with logging and validation behaviors around every handler |
| Result pattern | Expected failures returned as `Result`, exceptions kept for surprises |
| Contract tests | One abstract test class per port, run against every adapter ([Testcontainers](https://dotnet.testcontainers.org/)) |
| Architecture tests | Boundary, naming and dependency rules with [ArchUnitNET](https://github.com/TNG/ArchUnitNET) |
| [Problem Details (RFC 9457)](https://www.rfc-editor.org/rfc/rfc9457) | Every HTTP error response |
| Database-first and code-first | Fleet maps a schema it doesn't own; Bookings owns its schema with migrations |

## Quick start

**You need:** [.NET 10 SDK](https://dotnet.microsoft.com/download), Docker Desktop (or another Docker engine) and [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local). The queue example also uses the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli).

```bash
git clone https://github.com/gelinard/remodel-architecture
cd remodel-architecture

dotnet test              # unit, architecture and contract tests (Docker must be running)

docker compose up -d     # SQL Server with the legacy schema, PostgreSQL, Azurite
cd src/Hosts/Remodel.Host.Functions
func start
```

`docker compose` uses the ports 1433, 5432 and 10000–10002. In local development the host creates the Bookings schema at startup.

### Try it

```bash
# Fleet: the legacy module on SQL Server
curl http://localhost:7071/api/fleet/cars/1/history
curl http://localhost:7071/api/fleet/cars/available
curl -X POST http://localhost:7071/api/fleet/rentals/4/return \
     -H "Content-Type: application/json" -d '{"kmIn": 42600}'

# Bookings: the new module on PostgreSQL
curl -X POST http://localhost:7071/api/bookings \
     -H "Content-Type: application/json" \
     -d '{"carId": 2, "customerName": "Anna Berg", "pickupDate": "2026-12-01", "returnDate": "2026-12-04"}'
curl http://localhost:7071/api/bookings/<id>
curl -X POST http://localhost:7071/api/bookings/<id>/cancel

# The same ReturnCarCommand, from a queue message
az storage queue create --name fleet-car-returns --connection-string "UseDevelopmentStorage=true"
az storage message put --queue-name fleet-car-returns \
     --content '{"rentalId": 4, "kmIn": 42700}' --connection-string "UseDevelopmentStorage=true"
```

Rental 4 is the open rental in the sample data, so the second return of it is refused. Every error is a Problem Details response:

```json
{ "title": "Conflict", "status": 409, "detail": "The car has already been returned." }
```

**Switch adapters:** change `Fleet__Adapter` from `Dapper` to `EfCore` in `local.settings.json` and restart. The responses stay the same.

**Change the Bookings schema:** edit the model, then add a migration with the local `dotnet-ef` tool:

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project src/Modules/Bookings/Remodel.Bookings.Adapters.EfCore --output-dir Migrations
```

## Project structure

```text
remodel-architecture/
├─ src/
│  ├─ BuildingBlocks/
│  │  └─ Remodel.Dispatching/                 # queries, commands, pipeline behaviors, Result, validation
│  ├─ Modules/
│  │  ├─ Fleet/                               # legacy module on SQL Server
│  │  │  ├─ Remodel.Fleet.Core/               # use cases and ports, no database packages
│  │  │  ├─ Remodel.Fleet.Adapters.Dapper/    # stored procedures
│  │  │  └─ Remodel.Fleet.Adapters.EfCore/    # LINQ over the legacy tables
│  │  └─ Bookings/                            # new module on PostgreSQL
│  │     ├─ Remodel.Bookings.Core/
│  │     └─ Remodel.Bookings.Adapters.EfCore/ # code-first, with migrations
│  └─ Hosts/
│     └─ Remodel.Host.Functions/              # Azure Functions, isolated worker
├─ tests/
│  ├─ Remodel.Dispatching.Tests/
│  ├─ Remodel.ArchitectureTests/              # the rules, checked for every module
│  ├─ Remodel.Fleet.Tests/                    # use cases with fake ports
│  ├─ Remodel.Fleet.ContractTests/            # one suite, both adapters, real SQL Server
│  ├─ Remodel.Bookings.Tests/
│  └─ Remodel.Bookings.ContractTests/         # real PostgreSQL, real migrations
├─ db/fleet-legacy/                           # the legacy schema, stored procedures and sample data
├─ docs/adr/                                  # architecture decision records
└─ docker-compose.yml                         # SQL Server, PostgreSQL, Azurite
```

## Modernize your own system

1. **Pick one business area** with clear edges: a report, a list screen, one workflow.
2. **Create a module for it.** Add a port for each piece of data the area reads or writes today.
3. **Implement the ports with an adapter that calls your existing stored procedures.** A Dapper adapter is usually the fastest start.
4. **Write contract tests** against a copy of your schema in Docker. Put the rules you find in the stored procedures into the tests.
5. **Point the endpoint at the new module.** The old code for that area can now be deleted.
6. **Repeat.** When a module is ready to move, give it a new adapter on a new database. The contract tests tell you when it behaves the same.

### Start from this repository

- Click **Use this template** on GitHub, then delete the `Fleet` and `Bookings` sample modules and their tests. The architecture tests find your own modules by convention.
- *Planned:* install it as a `dotnet new` template (see the [roadmap](#roadmap)).

  ```bash
  dotnet new install Remodel.Templates
  dotnet new remodel -n Acme.Api --sample false
  dotnet new remodel-module -n Invoices
  ```

## How it compares

| | A typical template | Remodel Architecture |
|---|---|---|
| **Starting point** | A new, empty, code-first database | An existing database, stored procedures, a schema you don't own |
| **Data access** | A generic repository, or a `DbContext` inside handlers | One small port per use case, owned by the module |
| **ORM** | EF Core | EF Core **and** Dapper, chosen per module |
| **Adapter testing** | Mocks | The same contract tests on every adapter, on real databases |
| **Boundaries** | Folders and discipline | Project references (compiler) and architecture tests |
| **Migration path** | Not covered | Strangler fig: one module at a time |

## When not to use it

- **A small new app with a database you fully own.** A simpler setup is enough.
- **A rich domain with complex business rules.** Add a domain model inside your modules, or choose a DDD-focused template.
- **Microservices from day one.** This is a modular monolith. Modules can be extracted later, but that is not the starting point.

## FAQ

<details>
<summary><b>Why not MediatR?</b></summary>

MediatR is a great library, but recent versions need a commercial licence for many companies. This project needs to send a message, run a pipeline and call one handler, and that fits in about 150 readable lines you own. Moving to MediatR or another mediator later is a mechanical change. See [ADR 2](docs/adr/0002-own-dispatcher-instead-of-mediatr.md).
</details>

<details>
<summary><b>Why no generic repository?</b></summary>

`IRepository<T>` gives every caller every operation on every table and hides what a use case really needs. A port per use case is small, says exactly what it does, and maps naturally to a stored procedure. See [ADR 4](docs/adr/0004-one-port-per-use-case.md).
</details>

<details>
<summary><b>Why do writes call the stored procedure, even in the EF Core adapter?</b></summary>

The legacy rules for a write live in the procedure, and other applications use the same procedure. Calling it from every adapter keeps one version of the truth. Reads can be rewritten in LINQ, because the contract tests compare them with the procedures. See [ADR 12](docs/adr/0012-writes-call-the-legacy-procedure.md).
</details>

<details>
<summary><b>Why Azure Functions? Can I use another host?</b></summary>

Functions is a cheap and simple way to put an API next to a legacy system, and HTTP and queue triggers share the same handlers. Modules don't know the host: an architecture test forbids it. A Minimal API host is on the roadmap to prove it. See [ADR 11](docs/adr/0011-functions-host.md).
</details>

<details>
<summary><b>Is this Clean Architecture?</b></summary>

It follows the same dependency rule, but it doesn't claim a rich domain layer. It is ports and adapters, applied to a modular monolith that sits on a legacy database. If a module's business rules need a domain model, add one inside that module.
</details>

Every decision has a short record in [`docs/adr`](docs/adr/README.md).

## Roadmap

These are the next ideas, not promises. Is your use case missing? [Open an issue](https://github.com/gelinard/remodel-architecture/issues) and let's talk about it.

- **`dotnet new` template pack**, with a module scaffolder (`dotnet new remodel-module -n Invoices`)
- **Minimal API host** using the same modules, to show that the host doesn't matter
- **.NET Aspire** for a one-command local setup
- **Calls between modules** through a small contracts project ([ADR 14](docs/adr/0014-no-calls-between-modules-in-v1.md))
- **Source-generated dispatcher**, with no reflection

## Standing on the shoulders of

- [Hexagonal Architecture (Ports and Adapters)](https://alistair.cockburn.us/hexagonal-architecture/), Alistair Cockburn
- [Strangler Fig Application](https://martinfowler.com/bliki/StranglerFigApplication.html), Martin Fowler
- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/), Jimmy Bogard
- [CQRS Documents](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf), Greg Young
- [Modular Monolith with DDD](https://github.com/kgrzybek/modular-monolith-with-ddd), Kamil Grzybek

## Contributing

This project is provided as-is, as a reference and a starting point. Issues, questions and ideas are welcome, and so are stories about how you used it on your own legacy system. Support is best-effort.

If Remodel Architecture helps you, a star helps other teams find it.

## License

[MIT](LICENSE)
