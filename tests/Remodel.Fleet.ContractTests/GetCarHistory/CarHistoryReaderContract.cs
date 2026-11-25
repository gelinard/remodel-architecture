using Microsoft.Extensions.DependencyInjection;
using Remodel.Fleet.Core.Features.GetCarHistory;

namespace Remodel.Fleet.ContractTests.GetCarHistory;

/// <summary>
/// What every <see cref="ICarHistoryReader"/> must do, whatever technology it uses.
/// The rules come from the legacy stored procedure <c>dbo.CarHistoryGet</c>.
/// </summary>
public abstract class CarHistoryReaderContract(LegacyFleetDatabase database, Action<IServiceCollection, string> addAdapter)
    : AdapterContract(database, addAdapter)
{
    private ICarHistoryReader Reader => Port<ICarHistoryReader>();

    [Fact]
    public async Task Returns_every_rental_of_the_car_newest_first()
    {
        var car = await Database.AddCar();
        var ida = await Database.AddCustomer("Ida", "Sand");
        var first = await Database.AddRental(car, ida, At(2026, 3, 1), returned: At(2026, 3, 4));
        var latest = await Database.AddRental(car, ida, At(2026, 5, 1));
        var middle = await Database.AddRental(car, ida, At(2026, 4, 1), returned: At(2026, 4, 2));

        var history = await Reader.Read(car, Ct);

        history.ShouldBe(
        [
            new CarHistoryItem(latest, "Ida Sand", At(2026, 5, 1), Returned: null),
            new CarHistoryItem(middle, "Ida Sand", At(2026, 4, 1), At(2026, 4, 2)),
            new CarHistoryItem(first, "Ida Sand", At(2026, 3, 1), At(2026, 3, 4)),
        ]);
    }

    [Fact]
    public async Task Rentals_picked_up_at_the_same_time_are_ordered_newest_rental_first()
    {
        var car = await Database.AddCar();
        var customer = await Database.AddCustomer("Ola", "Strand");
        var older = await Database.AddRental(car, customer, At(2026, 6, 1));
        var newer = await Database.AddRental(car, customer, At(2026, 6, 1));

        var history = await Reader.Read(car, Ct);

        history.Select(h => h.RentalId).ShouldBe([newer, older]);
    }

    [Fact]
    public async Task Shows_the_customer_as_first_and_last_name()
    {
        var car = await Database.AddCar();
        var customer = await Database.AddCustomer("Karin", "Dahl");
        await Database.AddRental(car, customer, At(2026, 7, 1));

        var history = await Reader.Read(car, Ct);

        history.ShouldHaveSingleItem().Customer.ShouldBe("Karin Dahl");
    }

    [Fact]
    public async Task Hides_deleted_rentals()
    {
        var car = await Database.AddCar();
        var customer = await Database.AddCustomer("Per", "Vik");
        var kept = await Database.AddRental(car, customer, At(2026, 8, 1));
        await Database.AddRental(car, customer, At(2026, 8, 2), deleted: true);

        var history = await Reader.Read(car, Ct);

        history.ShouldHaveSingleItem().RentalId.ShouldBe(kept);
    }

    [Fact]
    public async Task Only_returns_rentals_of_the_requested_car()
    {
        var car = await Database.AddCar();
        var otherCar = await Database.AddCar();
        var customer = await Database.AddCustomer("Eva", "Ek");
        var mine = await Database.AddRental(car, customer, At(2026, 9, 1));
        await Database.AddRental(otherCar, customer, At(2026, 9, 2));

        var history = await Reader.Read(car, Ct);

        history.ShouldHaveSingleItem().RentalId.ShouldBe(mine);
    }

    [Fact]
    public async Task Returns_an_empty_list_for_a_car_without_rentals()
    {
        var car = await Database.AddCar();

        var history = await Reader.Read(car, Ct);

        history.ShouldBeEmpty();
    }

    [Fact]
    public async Task Returns_an_empty_list_for_an_unknown_car()
    {
        var history = await Reader.Read(carId: -1, Ct);

        history.ShouldBeEmpty();
    }

    // Whole seconds only: the legacy DATETIME type rounds milliseconds.
    private static DateTime At(int year, int month, int day) => new(year, month, day, 9, 30, 0);
}
