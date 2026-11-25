using Remodel.Dispatching;

namespace Remodel.Bookings.Core.Features.GetBooking;

internal sealed class GetBookingQueryHandler(IBookingReader reader) : IQueryHandler<GetBookingQuery, BookingDetails?>
{
    public Task<BookingDetails?> Handle(GetBookingQuery query, CancellationToken ct)
        => reader.Read(query.BookingId, ct);
}
