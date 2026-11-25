using Remodel.Dispatching;

namespace Remodel.Bookings.Core.Features.CreateBooking;

internal sealed class CreateBookingCommandHandler(IBookingWriter writer) : ICommandHandler<CreateBookingCommand, Guid>
{
    public async Task<Guid> Handle(CreateBookingCommand command, CancellationToken ct)
    {
        // The id is made here, not by the database, so it is known before the booking is saved.
        // Version 7 GUIDs start with a timestamp, so new rows are added at the end of the index.
        var id = Guid.CreateVersion7();

        await writer.Add(new NewBooking(id, command.CarId, command.CustomerName, command.PickupDate, command.ReturnDate), ct);
        return id;
    }
}
