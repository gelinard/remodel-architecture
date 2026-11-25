using Microsoft.Extensions.DependencyInjection;
using Remodel.Fleet.Core.Features.GetAvailableCars;

namespace Remodel.Fleet.ContractTests.GetAvailableCars;

/// <summary>
/// What every <see cref="IAvailableCarsReader"/> must do. The rules come from <c>dbo.AvailableCarsGet</c>.
/// </summary>
public abstract class AvailableCarsReaderContract(LegacyFleetDatabase database, Action<IServiceCollection, string> addAdapter)
    : AdapterContract(database, addAdapter)
{
    [Fact]
    public async Task Lists_a_free_car_with_its_details()
    {
        var regNo = Database.NewRegNo();
        var car = await Database.AddCar(regNo, status: "A", odometer: 12345);

        var cars = await ReadAvailable(car);

        cars.ShouldBe([new AvailableCar(car, regNo, "Test car", Odometer: 12345)]);
    }

    // Legacy data isn't always consistent: a car can be marked "available" while a rental is still open.
    [Fact]
    public async Task Hides_a_car_that_is_out_on_a_rental_even_if_marked_available()
    {
        var car = await Database.AddCar(status: "A");
        var customer = await Database.AddCustomer("Nils", "Aas");
        await Database.AddRental(car, customer, new DateTime(2026, 10, 1, 9, 0, 0));

        var cars = await ReadAvailable(car);

        cars.ShouldBeEmpty();
    }

    [Fact]
    public async Task Hides_a_car_that_is_not_marked_available()
    {
        var car = await Database.AddCar(status: "S");

        var cars = await ReadAvailable(car);

        cars.ShouldBeEmpty();
    }

    [Fact]
    public async Task Returned_and_deleted_rentals_dont_block_a_car()
    {
        var car = await Database.AddCar(status: "A");
        var customer = await Database.AddCustomer("Siri", "Bø");
        await Database.AddRental(car, customer, new DateTime(2026, 9, 1, 9, 0, 0), returned: new DateTime(2026, 9, 3, 9, 0, 0));
        await Database.AddRental(car, customer, new DateTime(2026, 9, 5, 9, 0, 0), deleted: true);

        var cars = await ReadAvailable(car);

        cars.ShouldHaveSingleItem().CarId.ShouldBe(car);
    }

    [Fact]
    public async Task Sorts_by_registration_number()
    {
        var regNo = Database.NewRegNo();
        var second = await Database.AddCar(regNo + "B"); // added first, so it has the lower id
        var first = await Database.AddCar(regNo + "A");

        var cars = await ReadAvailable(first, second);

        cars.Select(c => c.CarId).ShouldBe([first, second]);
    }

    // Other tests add cars to the same database at the same time, so each test only looks at its own cars.
    private async Task<IReadOnlyList<AvailableCar>> ReadAvailable(params int[] carIds)
    {
        var all = await Port<IAvailableCarsReader>().Read(Ct);
        return all.Where(car => carIds.Contains(car.CarId)).ToList();
    }
}
