# 11. The Azure Functions host: configuration picks the adapter, and errors are handled in one place

- Status: Accepted
- Date: 2026-11-25

## Context

The host puts the modules on the network. It must stay thin: business code belongs in the modules. It also has to prove three promises from the README:

- the adapter can be switched without code changes;
- one command can run from different triggers;
- errors are handled in one place.

## Decision

- **Isolated worker with ASP.NET Core integration.** HTTP functions use the familiar `HttpRequest` and `IActionResult` types.
- **Thin functions.** A function turns a request or message into a query or command and sends it through `IDispatcher`. That's all it does.
- **Adapter from configuration.** `FleetModule.AddFleet` reads `Fleet:Adapter` (`Dapper` or `EfCore`) and the `FleetLegacy` connection string, and fails at startup on any other value.
- **One command, two triggers.** `ReturnCarCommand` runs from `POST /api/fleet/rentals/{id}/return` and from the `fleet-car-returns` queue. The queue message is the command itself as JSON. The same validation, logging and handler run for both.
- **Errors in one place.** `Http/Problems.cs` builds every HTTP error as problem details (RFC 9457):
  - a failed `Result` becomes 404, 409 or 422;
  - a `ValidationException` or broken JSON becomes 400;
  - anything else becomes a 500 with no internal details.

  `ErrorHandlingMiddleware` catches exceptions from HTTP functions and logs only the 500s. Queue exceptions are left to the Functions runtime: it retries 3 times (`host.json`), then moves the message to `fleet-car-returns-poison`.
- **Plain JSON queue messages** (`"messageEncoding": "none"`), so a message can be sent with the Azure CLI or read in a storage explorer.
- **Simple local setup.** `local.settings.json` is committed, because it only holds values for the local emulators and the Docker SQL Server. Real settings belong in the Function App's configuration.

## Consequences

- Switching between Dapper and EF Core is one setting, and the contract tests show that both behave the same.
- Functions stay a few lines each. A Minimal API host (planned for v2) can reuse everything except the trigger attributes.
- An invalid queue message is retried before it reaches the poison queue, even though retrying won't help. That is accepted for simplicity.
