using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Remodel.Bookings.Core.Features.CancelBooking;
using Remodel.Bookings.Core.Features.CreateBooking;
using Remodel.Bookings.Core.Features.GetBooking;
using Remodel.Dispatching;
using Remodel.Host.Functions.Http;

namespace Remodel.Host.Functions.Bookings;

/// <summary>HTTP endpoints for the Bookings module. A function only turns HTTP into a message; the module does the work.</summary>
public sealed class BookingFunctions(IDispatcher dispatcher)
{
    /// <summary>
    /// <c>POST /api/bookings</c> with a body like
    /// <c>{ "carId": 2, "customerName": "Anna Berg", "pickupDate": "2026-12-01", "returnDate": "2026-12-04" }</c>.
    /// </summary>
    [Function("CreateBooking")]
    public async Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bookings")] HttpRequest request,
        CancellationToken ct)
    {
        var command = await JsonSerializer.DeserializeAsync<CreateBookingCommand>(request.Body, JsonSerializerOptions.Web, ct)
            ?? throw new ValidationException([new ValidationError("body", "A JSON body is required.")]);

        var id = await dispatcher.Send(command, ct);
        return new CreatedResult($"/api/bookings/{id}", new { id });
    }

    [Function("GetBooking")]
    public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bookings/{bookingId:guid}")] HttpRequest request,
        Guid bookingId,
        CancellationToken ct)
    {
        var booking = await dispatcher.Query(new GetBookingQuery(bookingId), ct);
        return booking is null ? Problems.NotFound("Booking not found.") : new OkObjectResult(booking);
    }

    [Function("CancelBooking")]
    public async Task<IActionResult> Cancel(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bookings/{bookingId:guid}/cancel")] HttpRequest request,
        Guid bookingId,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new CancelBookingCommand(bookingId), ct);
        return result.ToActionResult();
    }
}
