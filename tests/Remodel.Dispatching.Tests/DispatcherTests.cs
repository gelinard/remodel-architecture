using Microsoft.Extensions.DependencyInjection;

namespace Remodel.Dispatching.Tests;

public sealed class DispatcherTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Query_runs_the_registered_handler()
    {
        using var host = new TestHost();

        var result = await host.Dispatcher.Query(new Ping("hello"), Ct);

        result.ShouldBe("pong: hello");
    }

    [Fact]
    public async Task Send_runs_the_registered_command_handler()
    {
        using var host = new TestHost();

        var result = await host.Dispatcher.Send(new Rename("Fleet"), Ct);

        result.ShouldBe(5);
    }

    [Fact]
    public async Task The_cancellation_token_reaches_the_handler()
    {
        using var host = new TestHost();
        using var cts = new CancellationTokenSource();

        var received = await host.Dispatcher.Query(new CaptureToken(), cts.Token);

        received.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task A_message_without_a_handler_fails_with_a_clear_message()
    {
        using var host = new TestHost();

        var error = await Should.ThrowAsync<InvalidOperationException>(() => host.Dispatcher.Query(new Orphan(), Ct));

        error.Message.ShouldContain("No handler is registered for Orphan");
    }

    [Fact]
    public async Task Handler_exceptions_are_not_wrapped()
    {
        using var host = new TestHost();

        await Should.ThrowAsync<BoomException>(() => host.Dispatcher.Send(new Explode(), Ct));
    }

    [Fact]
    public async Task Behaviors_run_around_the_handler_in_registration_order()
    {
        using var host = new TestHost(services => services
            .AddPipelineBehavior(typeof(OuterBehavior<,>))
            .AddPipelineBehavior(typeof(InnerBehavior<,>)));

        var result = await host.Dispatcher.Query(new Track(), Ct);

        result.ShouldBe("done");
        host.Recorder.Steps.ShouldBe(["outer:before", "inner:before", "handler", "inner:after", "outer:after"]);
    }

    [Fact]
    public async Task Open_generic_behaviors_also_run_for_commands()
    {
        using var host = new TestHost(services => services
            .AddPipelineBehavior(typeof(OuterBehavior<,>)));

        await host.Dispatcher.Send(new Rename("x"), Ct);

        host.Recorder.Steps.ShouldBe(["outer:before", "outer:after"]);
    }

    [Fact]
    public async Task A_behavior_can_stop_the_pipeline_without_calling_the_handler()
    {
        using var host = new TestHost(services => services
            .AddScoped<IPipelineBehavior<Track, string>, CachedTrackBehavior>());

        var result = await host.Dispatcher.Query(new Track(), Ct);

        result.ShouldBe("from cache");
        host.Recorder.Steps.ShouldBeEmpty();
    }
}
