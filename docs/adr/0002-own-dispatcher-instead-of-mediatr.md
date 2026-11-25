# 2. A small own dispatcher instead of MediatR

- Status: Accepted
- Date: 2026-11-24

## Context

Hosts (HTTP and queue triggers) need one way to run a use case, and cross-cutting work like logging and validation should live in one place, not in every handler. MediatR is the usual answer, but recent versions need a commercial licence for many companies. What this project needs is small: find one handler, run a pipeline around it.

## Decision

Write the dispatcher in `Remodel.Dispatching`, about 150 lines:

- `IQuery<TResult>` / `ICommand<TResult>` and their handlers. Queries and commands are separate types, and the dispatcher has separate `Query(...)` and `Send(...)` methods, so reads and writes are visible at the call site.
- `IPipelineBehavior<TMessage, TResult>` for cross-cutting work. Behaviors run in registration order, the first one outermost. They are registered explicitly, because order matters.
- `AddHandlersFrom(assembly)` registers every handler in a module, including `internal` ones. A second handler for the same message throws at startup instead of silently replacing the first.
- Reflection is used once per message type, to create a typed runner that is then cached. Later calls are plain generic calls, so they are fast and handler exceptions are not wrapped in `TargetInvocationException`.

## Consequences

- No licence risk and no external dependency. The whole thing can be read in a few minutes and is fully unit tested.
- We own it: no notifications, streaming or other features unless we write them. This project doesn't need them.
- The first call per message type uses reflection. A source-generated version is on the v2 list.
- Handlers have the usual shape (`Handle(message, ct)`), so moving to MediatR or another library later is a mechanical change.
