using Remodel.Dispatching;

namespace Remodel.Fleet.Core.Features.ReturnCar;

internal sealed class ReturnCarCommandHandler(ICarReturnWriter writer) : ICommandHandler<ReturnCarCommand, Result>
{
    public Task<Result> Handle(ReturnCarCommand command, CancellationToken ct)
        => writer.Return(command.RentalId, command.KmIn, command.ReturnedAt, ct);
}
