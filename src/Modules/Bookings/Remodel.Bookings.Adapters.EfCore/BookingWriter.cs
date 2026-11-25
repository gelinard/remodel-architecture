using Microsoft.EntityFrameworkCore;
using Remodel.Bookings.Core;
using Remodel.Bookings.Core.Features.CreateBooking;

namespace Remodel.Bookings.Adapters.EfCore;

internal sealed class BookingWriter(IDbContextFactory<BookingsDbContext> factory) : IBookingWriter
{
    public async Task Add(NewBooking booking, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        db.Bookings.Add(new BookingRow
        {
            Id = booking.Id,
            CarId = booking.CarId,
            CustomerName = booking.CustomerName,
            PickupDate = booking.PickupDate,
            ReturnDate = booking.ReturnDate,
            Status = BookingStatus.Active,
        });

        await db.SaveChangesAsync(ct);
    }
}
