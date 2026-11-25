using Microsoft.EntityFrameworkCore;
using Remodel.Bookings.Core.Features.GetBooking;

namespace Remodel.Bookings.Adapters.EfCore;

internal sealed class BookingReader(IDbContextFactory<BookingsDbContext> factory) : IBookingReader
{
    public async Task<BookingDetails?> Read(Guid bookingId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        return await db.Bookings
            .Where(b => b.Id == bookingId)
            .Select(b => new BookingDetails(b.Id, b.CarId, b.CustomerName, b.PickupDate, b.ReturnDate, b.Status))
            .SingleOrDefaultAsync(ct);
    }
}
