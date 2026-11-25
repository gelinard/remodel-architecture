using Remodel.Dispatching;

namespace Remodel.Fleet.Core.Features.ReturnCar;

/// <summary>
/// Checks the input before anything reaches the database. Rules that need data
/// (does the rental exist, is km in at least km out) stay in the legacy stored procedure.
/// </summary>
internal sealed class ReturnCarCommandValidator : IValidator<ReturnCarCommand>
{
    public IEnumerable<ValidationError> Validate(ReturnCarCommand command)
    {
        if (command.RentalId <= 0)
        {
            yield return new(nameof(command.RentalId), "Must be greater than 0.");
        }

        if (command.KmIn <= 0)
        {
            yield return new(nameof(command.KmIn), "Must be greater than 0.");
        }
    }
}
