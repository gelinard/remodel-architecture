# 10. A small own validator interface instead of FluentValidation

- Status: Accepted
- Date: 2026-11-25

## Context

Commands need input checks before anything reaches the database. FluentValidation is popular and still Apache-2.0 (version 12.2.0, checked 2026-11-25). But the checks here are simple, for example "RentalId must be greater than 0". This repository is meant to be easy to read and copy.

## Decision

- `IValidator<TMessage>` in `Remodel.Dispatching` has one method that returns a list of `ValidationError(Field, Message)`. An empty list means the message is valid.
- `ValidationBehavior` runs every validator registered for the message. If any of them reports an error, it throws a `ValidationException` with all the errors.
- Validators are `internal`, live next to their command (`Features/ReturnCar/ReturnCarCommandValidator.cs`) and are registered by `AddValidatorsFrom(assembly)`.
- Validators check the input only. Rules that need data, such as "is km in at least km out", stay where the data is: in the legacy stored procedure.

## Consequences

- No dependency, and a validator is plain C# that anyone can read.
- No rule library: no built-in rules or localized messages. If validators grow complex, a FluentValidation validator can be wrapped in an `IValidator<T>` without changing the behavior or the host.
