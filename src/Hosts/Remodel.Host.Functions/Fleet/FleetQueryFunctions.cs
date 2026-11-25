using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Remodel.Dispatching;
using Remodel.Fleet.Core.Features.GetAvailableCars;
using Remodel.Fleet.Core.Features.GetCarHistory;

namespace Remodel.Host.Functions.Fleet;

/// <summary>HTTP endpoints for the Fleet queries. A function only turns HTTP into a message; the module does the work.</summary>
public sealed class FleetQueryFunctions(IDispatcher dispatcher)
{
    [Function("GetCarHistory")]
    public async Task<IActionResult> GetCarHistory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fleet/cars/{carId:int}/history")] HttpRequest request,
        int carId,
        CancellationToken ct)
        => new OkObjectResult(await dispatcher.Query(new GetCarHistoryQuery(carId), ct));

    [Function("GetAvailableCars")]
    public async Task<IActionResult> GetAvailableCars(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fleet/cars/available")] HttpRequest request,
        CancellationToken ct)
        => new OkObjectResult(await dispatcher.Query(new GetAvailableCarsQuery(), ct));
}
