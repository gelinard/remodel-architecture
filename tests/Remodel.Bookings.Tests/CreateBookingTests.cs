using NSubstitute;
using Remodel.Bookings.Core.Features.CreateBooking;
using Remodel.Dispatching;

namespace Remodel.Bookings.Tests;

public sealed class CreateBookingTests
{
    private static readonly DateOnly December1 = new(2026, 12, 1);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_valid_booking_is_stored_and_its_new_id_is_returned()
    {
        using var bookings = new BookingsWithFakePorts();

        var id = await bookings.Dispatcher.Send(new CreateBookingCommand(2, "Anna Berg", December1, December1.AddDays(3)), Ct);

        id.ShouldNotBe(Guid.Empty);
        await bookings.Writer.Received(1).Add(
            new NewBooking(id, 2, "Anna Berg", December1, December1.AddDays(3)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_one_day_booking_is_allowed()
    {
        using var bookings = new BookingsWithFakePorts();

        await bookings.Dispatcher.Send(new CreateBookingCommand(2, "Anna Berg", December1, December1), Ct);

        await bookings.Writer.Received(1).Add(Arg.Any<NewBooking>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, "Anna Berg", 3, "CarId")]
    [InlineData(2, " ", 3, "CustomerName")]
    [InlineData(2, "Anna Berg", -1, "ReturnDate")]
    public async Task Invalid_input_is_never_stored(int carId, string customerName, int days, string invalidField)
    {
        using var bookings = new BookingsWithFakePorts();
        var command = new CreateBookingCommand(carId, customerName, December1, December1.AddDays(days));

        var error = await Should.ThrowAsync<ValidationException>(() => bookings.Dispatcher.Send(command, Ct));

        error.Errors.ShouldHaveSingleItem().Field.ShouldBe(invalidField);
        await bookings.Writer.DidNotReceive().Add(Arg.Any<NewBooking>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_customer_name_longer_than_100_characters_is_rejected()
    {
        using var bookings = new BookingsWithFakePorts();
        var command = new CreateBookingCommand(2, new string('x', 101), December1, December1.AddDays(1));

        var error = await Should.ThrowAsync<ValidationException>(() => bookings.Dispatcher.Send(command, Ct));

        error.Errors.ShouldHaveSingleItem().Field.ShouldBe("CustomerName");
    }
}
