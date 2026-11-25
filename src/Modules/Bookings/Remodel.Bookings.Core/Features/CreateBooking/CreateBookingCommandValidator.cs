using Remodel.Dispatching;

namespace Remodel.Bookings.Core.Features.CreateBooking;

internal sealed class CreateBookingCommandValidator : IValidator<CreateBookingCommand>
{
    public IEnumerable<ValidationError> Validate(CreateBookingCommand command)
    {
        if (command.CarId <= 0)
        {
            yield return new(nameof(command.CarId), "Must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(command.CustomerName))
        {
            yield return new(nameof(command.CustomerName), "Is required.");
        }
        else if (command.CustomerName.Length > 100)
        {
            yield return new(nameof(command.CustomerName), "Must be 100 characters or less.");
        }

        if (command.ReturnDate < command.PickupDate)
        {
            yield return new(nameof(command.ReturnDate), "Can't be before the pickup date.");
        }
    }
}
