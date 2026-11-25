using Microsoft.EntityFrameworkCore;
using Remodel.Bookings.Core;
using Remodel.Bookings.Core.Features.CancelBooking;

namespace Remodel.Bookings.Adapters.EfCore;

internal sealed class BookingCancellationWriter(IDbContextFactory<BookingsDbContext> factory) : IBookingCancellationWriter
{
    public async Task<BookingStatus?> GetStatus(Guid bookingId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        return await db.Bookings
            .Where(b => b.Id == bookingId)
            .Select(b => (BookingStatus?)b.Status)
            .SingleOrDefaultAsync(ct);
    }

    public async Task MarkCancelled(Guid bookingId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // One UPDATE statement, without loading the row first.
        await db.Bookings
            .Where(b => b.Id == bookingId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Status, BookingStatus.Cancelled), ct);
    }
}
