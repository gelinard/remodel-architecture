# 4. One port per use case, no generic repositories

- Status: Accepted
- Date: 2026-11-24

## Context

A generic `IRepository<T>` gives every caller every operation on every table and hides what a use case really needs. It is hard to implement with stored procedures, and it often leaks `IQueryable`, so database details end up in business code anyway.

## Decision

- Each use case gets its own small port, named after what it does: `I...Reader` for queries, `I...Writer` for commands.
- Port methods return the module's own records. Never `IQueryable`, dictionaries, `DataTable` or EF entities.
- One folder per use case in Core: `Features/GetCarHistory/` holds the query, its result record, the port and the handler.
- Fixed name suffixes: `...Query`, `...Command`, `...QueryHandler`, `...CommandHandler`, `...Reader`, `...Writer`. The architecture tests find types by these names and also check that the names are used correctly.
- Query handlers never use writers.

## Consequences

- A port is small and says exactly what it does. It often maps one-to-one to an existing stored procedure.
- Ports are easy to fake in unit tests and easy to check in architecture tests.
- There are more interfaces than with one repository per table. Two use cases that read similar data may have similar ports. That small duplication is accepted: it costs less than one shared interface that grows with every new caller.
