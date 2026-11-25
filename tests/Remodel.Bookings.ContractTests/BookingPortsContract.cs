using Microsoft.Extensions.DependencyInjection;
using Remodel.Bookings.Adapters.EfCore;
using Remodel.Bookings.Core;
using Remodel.Bookings.Core.Features.CancelBooking;
using Remodel.Bookings.Core.Features.CreateBooking;
using Remodel.Bookings.Core.Features.GetBooking;

namespace Remodel.Bookings.ContractTests;

/// <summary>
/// What every Bookings adapter must do. The three ports work on the same bookings,
/// so the tests use them together: store a booking with one port, check it with another.
/// </summary>
public abstract class BookingPortsContract(BookingsDatabase database, Action<IServiceCollection, string> addAdapter)
    : AdapterContract(database, addAdapter)
{
    private static readonly DateOnly December1 = new(2026, 12, 1);

    private IBookingWriter Writer => Port<IBookingWriter>();

    private IBookingReader Reader => Port<IBookingReader>();

    private IBookingCancellationWriter Cancellations => Port<IBookingCancellationWriter>();

    [Fact]
    public async Task A_new_booking_can_be_read_back()
    {
        var booking = NewBooking();

        await Writer.Add(booking, Ct);

        (await Reader.Read(booking.Id, Ct)).ShouldBe(new BookingDetails(
            booking.Id, booking.CarId, booking.CustomerName, booking.PickupDate, booking.ReturnDate, BookingStatus.Active));
    }

    [Fact]
    public async Task An_unknown_booking_reads_as_null()
    {
        (await Reader.Read(Guid.CreateVersion7(), Ct)).ShouldBeNull();
    }

    [Fact]
    public async Task A_new_booking_is_active()
    {
        var booking = NewBooking();
        await Writer.Add(booking, Ct);

        (await Cancellations.GetStatus(booking.Id, Ct)).ShouldBe(BookingStatus.Active);
    }

    [Fact]
    public async Task An_unknown_booking_has_no_status()
    {
        (await Cancellations.GetStatus(Guid.CreateVersion7(), Ct)).ShouldBeNull();
    }

    [Fact]
    public async Task Marking_a_booking_cancelled_changes_only_that_booking()
    {
        var cancelled = NewBooking();
        var other = NewBooking();
        await Writer.Add(cancelled, Ct);
        await Writer.Add(other, Ct);

        await Cancellations.MarkCancelled(cancelled.Id, Ct);

        (await Reader.Read(cancelled.Id, Ct))!.Status.ShouldBe(BookingStatus.Cancelled);
        (await Reader.Read(other.Id, Ct))!.Status.ShouldBe(BookingStatus.Active);
    }

    private static NewBooking NewBooking() =>
        new(Guid.CreateVersion7(), CarId: 7, "Åse Bø", December1, December1.AddDays(2));
}

// The same contract for every Bookings adapter. Today there is one; a second one would be one more line here.
public sealed class EfCoreBookingPortsTests(BookingsDatabase database)
    : BookingPortsContract(database, (services, connectionString) => services.AddBookingsEfCore(connectionString));
