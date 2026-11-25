using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Remodel.Dispatching;
using Remodel.Fleet.Core.Features.ReturnCar;
using Remodel.Host.Functions.Http;

namespace Remodel.Host.Functions.Fleet;

/// <summary>
/// One command, two triggers: <see cref="ReturnCarCommand"/> runs the same way from an HTTP request
/// and from a queue message. Validation, logging and the handler are shared.
/// </summary>
public sealed class ReturnCarFunctions(IDispatcher dispatcher)
{
    public const string QueueName = "fleet-car-returns";

    /// <summary><c>POST /api/fleet/rentals/{rentalId}/return</c> with a body like <c>{ "kmIn": 42500 }</c>.</summary>
    [Function("ReturnCar")]
    public async Task<IActionResult> FromHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fleet/rentals/{rentalId:int}/return")] HttpRequest request,
        int rentalId,
        CancellationToken ct)
    {
        var body = await JsonSerializer.DeserializeAsync<ReturnCarRequest>(request.Body, JsonSerializerOptions.Web, ct)
            ?? throw new ValidationException([new ValidationError("body", "A JSON body is required.")]);

        var result = await dispatcher.Send(new ReturnCarCommand(rentalId, body.KmIn, body.ReturnedAt), ct);
        return result.ToActionResult();
    }

    /// <summary>A message on the <c>fleet-car-returns</c> queue, like <c>{ "rentalId": 4, "kmIn": 42500 }</c>.</summary>
    [Function("ReturnCarFromQueue")]
    public async Task FromQueue([QueueTrigger(QueueName)] ReturnCarCommand command, CancellationToken ct)
    {
        // An expected failure (rental not found, already returned...) won't change if the message is retried,
        // so the LoggingBehavior logs it as a warning and the message is done.
        // Exceptions are not caught: the Functions runtime retries the message, then moves it to the poison queue.
        await dispatcher.Send(command, ct);
    }
}

/// <summary>The JSON body of <c>POST /api/fleet/rentals/{rentalId}/return</c>. Without <c>returnedAt</c>, "now" is used.</summary>
public sealed record ReturnCarRequest(int KmIn, DateTime? ReturnedAt = null);
