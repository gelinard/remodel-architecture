# 5. Contract tests run every adapter on a real database

- Status: Accepted
- Date: 2026-11-24

## Context

A port can have several adapters, for example Dapper calling a stored procedure and EF Core using LINQ. They must return exactly the same results. Mocks can't show that, and an in-memory database doesn't behave like SQL Server. Some business rules exist only inside the legacy stored procedures, such as hidden deleted rentals and a specific sort order.

## Decision

- For each port there is one abstract contract class (`CarHistoryReaderContract`) in `Remodel.<Module>.ContractTests`. It describes the expected behavior once.
- Each adapter gets a one-line subclass that registers the adapter with its real `AddFleet...` method, so the DI registration is tested too.
- The tests run on SQL Server in Docker with Testcontainers. They use the same image tag as `docker-compose.yml`, and the database is built from the same scripts in `db/fleet-legacy/`.
- One container is shared by the whole test assembly (an xUnit assembly fixture). Each test inserts its own cars and customers, so tests don't depend on each other and can run in parallel.

## Consequences

- Differences between adapters are caught. For example, removing the deleted-rental filter from the EF Core adapter fails exactly one test, for EF Core only.
- The database scripts are tested on every run.
- Docker must be running. Starting the container adds about 20 seconds.
- The tests check results, not SQL. A rule can pass by luck when the database already returns rows in the expected order. Without its tie-break, the EF Core query still passed, because the index returns ties in that order anyway. A *wrong* tie-break does fail.
