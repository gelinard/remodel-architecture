using Remodel.Bookings.Core;

namespace Remodel.Bookings.Adapters.EfCore;

/// <summary>A row in <c>bookings.bookings</c>. It never leaves this project: readers project it into the module's records.</summary>
internal sealed class BookingRow
{
    public Guid Id { get; set; }

    public int CarId { get; set; }

    public string CustomerName { get; set; } = "";

    public DateOnly PickupDate { get; set; }

    public DateOnly ReturnDate { get; set; }

    public BookingStatus Status { get; set; }
}
