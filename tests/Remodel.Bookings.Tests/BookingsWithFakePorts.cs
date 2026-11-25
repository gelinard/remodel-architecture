using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Remodel.Bookings.Core;
using Remodel.Bookings.Core.Features.CancelBooking;
using Remodel.Bookings.Core.Features.CreateBooking;
using Remodel.Bookings.Core.Features.GetBooking;
using Remodel.Dispatching;

namespace Remodel.Bookings.Tests;

/// <summary>
/// The Bookings module with fake ports: the real handlers, validators and pipeline, but no database.
/// Tests send messages through the dispatcher, exactly like a host does, so handlers can stay internal.
/// </summary>
internal sealed class BookingsWithFakePorts : IDisposable
{
    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;

    public BookingsWithFakePorts()
    {
        var services = new ServiceCollection()
            .AddDispatcher()
            .AddPipelineBehavior(typeof(ValidationBehavior<,>))
            .AddBookingsCore()
            .AddSingleton(Writer)
            .AddSingleton(Reader)
            .AddSingleton(Cancellations);

        _root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        _scope = _root.CreateScope();
    }

    public IBookingWriter Writer { get; } = Substitute.For<IBookingWriter>();

    public IBookingReader Reader { get; } = Substitute.For<IBookingReader>();

    public IBookingCancellationWriter Cancellations { get; } = Substitute.For<IBookingCancellationWriter>();

    public IDispatcher Dispatcher => _scope.ServiceProvider.GetRequiredService<IDispatcher>();

    public void Dispose()
    {
        _scope.Dispose();
        _root.Dispose();
    }
}
