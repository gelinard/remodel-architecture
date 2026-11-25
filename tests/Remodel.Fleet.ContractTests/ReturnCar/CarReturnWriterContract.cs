using Microsoft.Extensions.DependencyInjection;
using Remodel.Dispatching;
using Remodel.Fleet.Core.Features.ReturnCar;

namespace Remodel.Fleet.ContractTests.ReturnCar;

/// <summary>
/// What every <see cref="ICarReturnWriter"/> must do. The rules and the error codes come from <c>dbo.CarReturn</c>.
/// </summary>
public abstract class CarReturnWriterContract(LegacyFleetDatabase database, Action<IServiceCollection, string> addAdapter)
    : AdapterContract(database, addAdapter)
{
    // In the past, so "now" is always after the pickup.
    private static readonly DateTime PickedUp = new(2020, 1, 6, 9, 0, 0);

    private ICarReturnWriter Writer => Port<ICarReturnWriter>();

    [Fact]
    public async Task Returning_a_car_closes_the_rental_and_makes_the_car_available()
    {
        var (car, rental) = await AddOpenRental(kmOut: 1000);

        var result = await Writer.Return(rental, kmIn: 1250, returnedAt: PickedUp.AddDays(2), Ct);

        result.ShouldBe(Result.Success);
        (await Database.GetRental(rental)).ShouldBe(new RentalState(PickedUp.AddDays(2), KmIn: 1250));
        (await Database.GetCar(car)).ShouldBe(new CarState("A", Odometer: 1250));
    }

    [Fact]
    public async Task Without_a_return_time_the_current_time_is_used()
    {
        var (_, rental) = await AddOpenRental();

        var result = await Writer.Return(rental, kmIn: 1100, returnedAt: null, Ct);

        result.ShouldBe(Result.Success);
        (await Database.GetRental(rental)).Returned.ShouldNotBeNull();
    }

    [Fact]
    public async Task An_unknown_rental_is_not_found()
    {
        var result = await Writer.Return(rentalId: -1, kmIn: 1100, returnedAt: null, Ct);

        ShouldFailWith(result, ErrorKind.NotFound);
    }

    [Fact]
    public async Task A_deleted_rental_is_not_found()
    {
        var car = await Database.AddCar(status: "R");
        var customer = await Database.AddCustomer("Tor", "Lie");
        var rental = await Database.AddRental(car, customer, PickedUp, deleted: true);

        var result = await Writer.Return(rental, kmIn: 1100, returnedAt: null, Ct);

        ShouldFailWith(result, ErrorKind.NotFound);
    }

    [Fact]
    public async Task Returning_the_same_rental_twice_is_a_conflict()
    {
        var (_, rental) = await AddOpenRental();
        await Writer.Return(rental, kmIn: 1100, returnedAt: null, Ct);

        var result = await Writer.Return(rental, kmIn: 1200, returnedAt: null, Ct);

        ShouldFailWith(result, ErrorKind.Conflict);
    }

    [Fact]
    public async Task Km_in_lower_than_km_out_is_rejected_and_nothing_changes()
    {
        var (car, rental) = await AddOpenRental(kmOut: 1000);

        var result = await Writer.Return(rental, kmIn: 900, returnedAt: null, Ct);

        ShouldFailWith(result, ErrorKind.Rejected);
        (await Database.GetRental(rental)).ShouldBe(new RentalState(Returned: null, KmIn: null));
        (await Database.GetCar(car)).Status.ShouldBe("R");
    }

    [Fact]
    public async Task A_return_time_before_the_pickup_is_rejected()
    {
        var (_, rental) = await AddOpenRental();

        var result = await Writer.Return(rental, kmIn: 1100, returnedAt: PickedUp.AddHours(-1), Ct);

        ShouldFailWith(result, ErrorKind.Rejected);
    }

    private async Task<(int Car, int Rental)> AddOpenRental(int kmOut = 1000)
    {
        var car = await Database.AddCar(status: "R", odometer: kmOut);
        var customer = await Database.AddCustomer("Lena", "Moe");
        var rental = await Database.AddRental(car, customer, PickedUp, kmOut: kmOut);
        return (car, rental);
    }

    private static void ShouldFailWith(Result result, ErrorKind kind)
    {
        result.Error.ShouldNotBeNull();
        result.Error.Kind.ShouldBe(kind);
        result.Error.Message.ShouldNotBeNullOrWhiteSpace();
    }
}
