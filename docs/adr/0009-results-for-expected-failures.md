# 9. Expected failures are results, unexpected failures are exceptions

- Status: Accepted
- Date: 2026-11-25

## Context

Returning a car can fail in ways that are part of normal business: the rental doesn't exist, the car is already returned, or the km reading is lower than at pickup. The legacy stored procedure reports these with a return code and an `@ErrMsg` text, not with SQL errors. Other failures are not expected at all: the database is down, or there is a bug.

The two kinds need different handling. Over HTTP, an expected failure is a 404, 409 or 422, while an unexpected one is a 500. On a queue, an expected failure must **not** be retried, because the answer won't change. An unexpected one should be retried.

## Decision

- Commands that can fail in an expected way return `Result` (from `Remodel.Dispatching`). A failed result carries an `Error` with a kind (`NotFound`, `Conflict`, `Rejected`) and a message.
- Everything else stays an exception and is not caught in handlers or adapters.
- Invalid input is reported by the validation behavior as a `ValidationException`. It is the caller's mistake, not a business answer.
- The host maps results and exceptions in one place (see ADR 11).
- `Result<T>` (a result with a value) will be added when the first command needs to return one.

## Consequences

- A handler's signature shows that it can fail and how. Exceptions are kept for real surprises.
- The queue trigger can complete a message with a business failure, while exceptions still go through retries and the poison queue.
- There are two error paths to understand instead of one. Both are mapped in one place in the host, which keeps that manageable.
