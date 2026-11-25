using NSubstitute;
using Remodel.Bookings.Core;
using Remodel.Bookings.Core.Features.CancelBooking;
using Remodel.Dispatching;

namespace Remodel.Bookings.Tests;

public sealed class CancelBookingTests
{
    private static readonly Guid BookingId = Guid.CreateVersion7();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task An_active_booking_is_cancelled()
    {
        using var bookings = new BookingsWithFakePorts();
        bookings.Cancellations.GetStatus(BookingId, Arg.Any<CancellationToken>()).Returns(BookingStatus.Active);

        var result = await bookings.Dispatcher.Send(new CancelBookingCommand(BookingId), Ct);

        result.ShouldBe(Result.Success);
        await bookings.Cancellations.Received(1).MarkCancelled(BookingId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_unknown_booking_is_not_found()
    {
        using var bookings = new BookingsWithFakePorts();
        bookings.Cancellations.GetStatus(BookingId, Arg.Any<CancellationToken>()).Returns((BookingStatus?)null);

        var result = await bookings.Dispatcher.Send(new CancelBookingCommand(BookingId), Ct);

        result.Error.ShouldBe(new Error(ErrorKind.NotFound, "Booking not found."));
        await bookings.Cancellations.DidNotReceive().MarkCancelled(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancelling_twice_is_a_conflict()
    {
        using var bookings = new BookingsWithFakePorts();
        bookings.Cancellations.GetStatus(BookingId, Arg.Any<CancellationToken>()).Returns(BookingStatus.Cancelled);

        var result = await bookings.Dispatcher.Send(new CancelBookingCommand(BookingId), Ct);

        result.Error.ShouldBe(new Error(ErrorKind.Conflict, "The booking is already cancelled."));
        await bookings.Cancellations.DidNotReceive().MarkCancelled(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
