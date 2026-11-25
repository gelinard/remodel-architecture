using Microsoft.Extensions.DependencyInjection;

namespace Remodel.Dispatching.Tests;

public sealed class RegistrationTests
{
    private static readonly System.Reflection.Assembly ThisAssembly = typeof(RegistrationTests).Assembly;

    [Fact]
    public void Internal_handlers_are_found_and_registered_as_scoped()
    {
        var services = new ServiceCollection().AddHandlersFrom(ThisAssembly);

        var registration = services.Where(d => d.ServiceType == typeof(IQueryHandler<Ping, string>)).ShouldHaveSingleItem();
        registration.ImplementationType.ShouldBe(typeof(PingHandler));
        registration.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void Command_handlers_are_registered_too()
    {
        var services = new ServiceCollection().AddHandlersFrom(ThisAssembly);

        services.ShouldContain(d => d.ServiceType == typeof(ICommandHandler<Rename, int>));
    }

    [Fact]
    public void Scanning_the_same_assembly_twice_is_safe()
    {
        var services = new ServiceCollection()
            .AddHandlersFrom(ThisAssembly)
            .AddHandlersFrom(ThisAssembly);

        services.Count(d => d.ServiceType == typeof(IQueryHandler<Ping, string>)).ShouldBe(1);
    }

    [Fact]
    public void A_second_handler_for_the_same_message_is_rejected()
    {
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<Ping, string>>(_ => new PingHandler());

        var error = Should.Throw<InvalidOperationException>(() => services.AddHandlersFrom(ThisAssembly));

        error.Message.ShouldContain("Ping already has a handler");
    }

    [Fact]
    public void Validators_are_found_and_a_message_can_have_several()
    {
        var services = new ServiceCollection().AddValidatorsFrom(ThisAssembly);

        services.Count(d => d.ServiceType == typeof(IValidator<Rename>)).ShouldBe(2);
    }

    [Fact]
    public void Scanning_validators_twice_is_safe()
    {
        var services = new ServiceCollection()
            .AddValidatorsFrom(ThisAssembly)
            .AddValidatorsFrom(ThisAssembly);

        services.Count(d => d.ServiceType == typeof(IValidator<Rename>)).ShouldBe(2);
    }

    [Theory]
    [InlineData(typeof(PingHandler))]          // not a behavior
    [InlineData(typeof(CachedTrackBehavior))]  // a behavior, but not open generic
    public void AddPipelineBehavior_only_accepts_open_generic_behaviors(Type type)
    {
        Should.Throw<ArgumentException>(() => new ServiceCollection().AddPipelineBehavior(type));
    }

    [Fact]
    public void AddDispatcher_can_be_called_more_than_once()
    {
        var services = new ServiceCollection().AddDispatcher().AddDispatcher();

        services.Count(d => d.ServiceType == typeof(IDispatcher)).ShouldBe(1);
    }
}
