using Remodel.Dispatching;

namespace Remodel.Bookings.Core.Features.CancelBooking;

/// <summary>
/// In this new module the business rules live here, in C#, and the adapter only stores data.
/// (In the legacy Fleet module the rules live in stored procedures.)
/// </summary>
internal sealed class CancelBookingCommandHandler(IBookingCancellationWriter writer) : ICommandHandler<CancelBookingCommand, Result>
{
    public async Task<Result> Handle(CancelBookingCommand command, CancellationToken ct)
    {
        var status = await writer.GetStatus(command.BookingId, ct);

        if (status is null)
        {
            return Result.Failure(ErrorKind.NotFound, "Booking not found.");
        }

        if (status == BookingStatus.Cancelled)
        {
            return Result.Failure(ErrorKind.Conflict, "The booking is already cancelled.");
        }

        await writer.MarkCancelled(command.BookingId, ct);
        return Result.Success;
    }
}
