using Microsoft.Extensions.Logging;

namespace Remodel.Dispatching.Tests;

public sealed class ValidationBehaviorTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static TestHost CreateHost() => new(services => services.AddPipelineBehavior(typeof(ValidationBehavior<,>)));

    [Fact]
    public async Task Valid_messages_reach_the_handler()
    {
        using var host = CreateHost();

        var result = await host.Dispatcher.Send(new Rename("Fleet"), Ct);

        result.ShouldBe(5);
    }

    [Fact]
    public async Task Invalid_messages_are_stopped_with_the_errors_of_every_validator()
    {
        using var host = CreateHost();

        var error = await Should.ThrowAsync<ValidationException>(() => host.Dispatcher.Send(new Rename("far too long name"), Ct));

        error.Errors.Select(e => e.Message).ShouldBe(
            ["Must start with a capital letter.", "Must be 10 characters or less."], ignoreOrder: true);
    }

    [Fact]
    public async Task Messages_without_validators_pass_through()
    {
        using var host = CreateHost();

        var result = await host.Dispatcher.Query(new Ping("hi"), Ct);

        result.ShouldBe("pong: hi");
    }
}

public sealed class LoggingBehaviorTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static TestHost CreateHost() => new(services => services.AddPipelineBehavior(typeof(LoggingBehavior<,>)));

    [Fact]
    public async Task Handled_messages_are_logged_with_their_name()
    {
        using var host = CreateHost();

        await host.Dispatcher.Send(new Rename("Fleet"), Ct);

        host.Logs.LatestRecord.Level.ShouldBe(LogLevel.Information);
        host.Logs.LatestRecord.Message.ShouldStartWith("Rename handled in");
    }

    [Fact]
    public async Task Expected_failures_are_logged_as_warnings()
    {
        using var host = CreateHost();

        var result = await host.Dispatcher.Send(new Reject(), Ct);

        result.IsSuccess.ShouldBeFalse();
        host.Logs.LatestRecord.Level.ShouldBe(LogLevel.Warning);
        host.Logs.LatestRecord.Message.ShouldEndWith("Conflict Already done.");
    }

    [Fact]
    public async Task Exceptions_are_left_to_the_host()
    {
        using var host = CreateHost();

        await Should.ThrowAsync<BoomException>(() => host.Dispatcher.Send(new Explode(), Ct));

        host.Logs.Count.ShouldBe(0);
    }
}

public sealed class ResultTests
{
    [Fact]
    public void Success_has_no_error()
    {
        Result.Success.IsSuccess.ShouldBeTrue();
        Result.Success.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_carries_the_kind_and_the_message()
    {
        var result = Result.Failure(ErrorKind.NotFound, "Rental not found.");

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(new Error(ErrorKind.NotFound, "Rental not found."));
    }
}
