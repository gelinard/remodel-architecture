# 14. Modules don't call each other in v1

- Status: Accepted
- Date: 2026-11-25

## Context

A booking names a car (`CarId`), and the cars belong to the Fleet module. It is tempting to let Bookings check with Fleet that the car exists and is free. But a module that reaches into another module's Core can't be changed, moved or extracted on its own, and that is the main reason for having modules.

## Decision

- In v1, Bookings doesn't call Fleet. `CreateBookingCommand` accepts any `CarId` greater than 0.
- The architecture test `Modules_do_not_use_each_other` enforces this. It was skipped while there was only one module, and it checks every module now that Bookings exists.
- When a cross-module check is needed, it will be done like this:
  1. Bookings declares its own port, for example `ICarAvailabilityReader` in `Remodel.Bookings.Core`.
  2. Fleet's public queries and result records move into a small `Remodel.Fleet.Contracts` project.
  3. A new adapter, for example `Remodel.Bookings.Adapters.Fleet`, implements the port by sending Fleet's query through `IDispatcher`. It references only `Remodel.Fleet.Contracts`, never `Remodel.Fleet.Core`.
  4. The architecture test then allows `Remodel.Fleet.Contracts` and nothing else from Fleet.

## Consequences

- The modules stay independent, and a new module is checked by the same rule automatically.
- A booking can name a car that doesn't exist or is busy. That is accepted for the sample and noted on `CreateBookingCommand`.
- When the check is added, Bookings' handlers only see their own port, so they don't change when Fleet moves to a new database.
