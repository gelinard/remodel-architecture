using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remodel.Bookings.Adapters.EfCore;
using Remodel.Dispatching;
using Remodel.Host.Functions;
using Remodel.Host.Functions.Http;

var builder = FunctionsApplication.CreateBuilder(args);

// HTTP functions use the ASP.NET Core types: HttpRequest and IActionResult.
builder.ConfigureFunctionsWebApplication();

// The one place where exceptions from HTTP functions become problem details responses.
builder.UseMiddleware<ErrorHandlingMiddleware>();

// Enums are sent as text ("Active"), not as numbers.
builder.Services.Configure<JsonOptions>(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddDispatcher()
    .AddPipelineBehavior(typeof(LoggingBehavior<,>))     // behaviors run in this order around every handler
    .AddPipelineBehavior(typeof(ValidationBehavior<,>))
    .AddFleet(builder.Configuration)
    .AddBookings(builder.Configuration);

var app = builder.Build();

// Local development only (local.settings.json): create or update the Bookings schema when the host starts.
// In production, run the migrations as a deployment step instead, so several instances don't migrate at the same time.
if (builder.Configuration.GetValue<bool>("Bookings:MigrateOnStartup"))
{
    await BookingsMigrations.ApplyAsync(app.Services, CancellationToken.None);
}

await app.RunAsync();
