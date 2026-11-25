using NSubstitute;
using Remodel.Fleet.Core.Features.GetAvailableCars;
using Remodel.Fleet.Core.Features.GetCarHistory;

namespace Remodel.Fleet.Tests;

public sealed class QueryTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Car_history_is_read_for_the_requested_car()
    {
        using var fleet = new FleetWithFakePorts();
        CarHistoryItem[] history = [new(RentalId: 7, "Anna Berg", new DateTime(2026, 3, 2, 9, 0, 0), Returned: null)];
        fleet.CarHistory.Read(1, Arg.Any<CancellationToken>()).Returns(history);

        var result = await fleet.Dispatcher.Query(new GetCarHistoryQuery(CarId: 1), Ct);

        result.ShouldBe(history);
    }

    [Fact]
    public async Task Available_cars_come_from_the_reader()
    {
        using var fleet = new FleetWithFakePorts();
        AvailableCar[] cars = [new(CarId: 2, "KX-2077", "Family estate, automatic", Odometer: 18300)];
        fleet.AvailableCars.Read(Arg.Any<CancellationToken>()).Returns(cars);

        var result = await fleet.Dispatcher.Query(new GetAvailableCarsQuery(), Ct);

        result.ShouldBe(cars);
    }
}
