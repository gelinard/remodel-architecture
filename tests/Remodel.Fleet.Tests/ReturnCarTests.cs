using NSubstitute;
using Remodel.Dispatching;
using Remodel.Fleet.Core.Features.ReturnCar;

namespace Remodel.Fleet.Tests;

public sealed class ReturnCarTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_valid_return_is_saved_through_the_writer()
    {
        using var fleet = new FleetWithFakePorts();
        fleet.CarReturns.Return(4, 42500, null, Arg.Any<CancellationToken>()).Returns(Result.Success);

        var result = await fleet.Dispatcher.Send(new ReturnCarCommand(RentalId: 4, KmIn: 42500), Ct);

        result.ShouldBe(Result.Success);
        await fleet.CarReturns.Received(1).Return(4, 42500, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_failure_from_the_writer_is_returned_unchanged()
    {
        using var fleet = new FleetWithFakePorts();
        var alreadyReturned = Result.Failure(ErrorKind.Conflict, "The car has already been returned.");
        fleet.CarReturns.Return(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(alreadyReturned);

        var result = await fleet.Dispatcher.Send(new ReturnCarCommand(RentalId: 4, KmIn: 42500), Ct);

        result.ShouldBe(alreadyReturned);
    }

    [Theory]
    [InlineData(0, 42500, "RentalId")]
    [InlineData(4, 0, "KmIn")]
    public async Task Invalid_input_never_reaches_the_writer(int rentalId, int kmIn, string invalidField)
    {
        using var fleet = new FleetWithFakePorts();

        var error = await Should.ThrowAsync<ValidationException>(
            () => fleet.Dispatcher.Send(new ReturnCarCommand(rentalId, kmIn), Ct));

        error.Errors.ShouldHaveSingleItem().Field.ShouldBe(invalidField);
        await fleet.CarReturns.DidNotReceive()
            .Return(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }
}
