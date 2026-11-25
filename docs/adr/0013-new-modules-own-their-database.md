# 13. New modules own their database: rules in C#, schema from EF Core migrations

- Status: Accepted
- Date: 2026-11-25

## Context

The legacy Fleet module can't change its database. Its rules live in stored procedures, and SQL scripts own its schema. Bookings is the *new* module built next to it, and it owns its database (PostgreSQL). The ports and adapters look the same in both modules. The decision here is where the business rules and the schema live when we are free to choose.

## Decision

- **Business rules live in Core handlers.** `CancelBookingCommandHandler` decides "not found" and "already cancelled". The adapter only stores and loads data, so the rules are unit tested with fake ports, without a database.
- **The schema comes from EF Core code-first migrations** in `Remodel.Bookings.Adapters.EfCore/Migrations`, reviewed like any other code.
  - Everything sits in a `bookings` schema with snake_case names, the usual PostgreSQL style.
  - The names are set explicitly in `BookingsDbContext`, instead of with a naming-convention package.
  - The migrations history table sits in the same schema.
- **Ids are version 7 GUIDs made by the handler.** The id is known before saving, and new rows go to the end of the index.
- **Tooling:** `dotnet-ef` is a local tool (`dotnet-tools.json`, then `dotnet tool restore`). A design-time factory lets the adapter project work on its own:

  ```
  dotnet ef migrations add <Name> --project src/Modules/Bookings/Remodel.Bookings.Adapters.EfCore --output-dir Migrations
  ```

  Generated migration files are marked as generated code in `.editorconfig` and never edited by hand.
- **Applying migrations:** `BookingsMigrations.ApplyAsync` is used in two places:
  - The contract test fixture runs it, so every test run tests the migrations. EF Core refuses to migrate when the model has changes that no migration covers, so a forgotten migration fails the tests.
  - The host runs it at startup only when `Bookings:MigrateOnStartup` is true, which only `local.settings.json` sets. In production, migrations belong in a deployment step, for example a `dotnet ef migrations bundle`.
- **PostgreSQL 18.7** in Docker Compose and Testcontainers. Version 19.0 had just been released; 18 already has a year of patch releases.

## Consequences

- The difference between the two modules is visible in the code: rules in stored procedures (legacy) vs. rules in handlers (new).
- Schema changes are code changes, and they are tested against a real PostgreSQL on every run.
- Two cancels at the same moment can both pass the check and both mark the booking cancelled. The result is the same, so this is accepted. A concurrency token would close the gap if it ever mattered.
- There is one adapter today. The contract is still an abstract test class, so a second adapter would need one line.
