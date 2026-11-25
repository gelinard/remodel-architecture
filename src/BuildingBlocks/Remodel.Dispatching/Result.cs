using System.Diagnostics.CodeAnalysis;

namespace Remodel.Dispatching;

/// <summary>The kind of an expected failure. Hosts turn it into a response, for example an HTTP status code.</summary>
public enum ErrorKind
{
    /// <summary>The thing the request is about doesn't exist.</summary>
    NotFound,

    /// <summary>The request clashes with the current state, for example "the car has already been returned".</summary>
    Conflict,

    /// <summary>A business rule says no, for example "km in can't be lower than km out".</summary>
    Rejected,
}

/// <summary>An expected failure: something the caller can understand and react to.</summary>
public sealed record Error(ErrorKind Kind, string Message);

/// <summary>
/// The outcome of a command that can fail in an expected way.
/// Expected failures are returned as a result. Unexpected ones (database down, bugs) stay exceptions.
/// </summary>
public sealed record Result
{
    public static readonly Result Success = new(error: null);

    private Result(Error? error) => Error = error;

    /// <summary>The failure, or null when the command succeeded.</summary>
    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static Result Failure(ErrorKind kind, string message) => new(new Error(kind, message));
}
