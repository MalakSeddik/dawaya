using System.Diagnostics.CodeAnalysis;

namespace Dawaya.Api.Infrastructure.Results;

public sealed class Result<T>
{
    private Result(T? value, AppError? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }
    public AppError? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    // Services return a value or an AppError directly; these conversions wrap them.
    public static implicit operator Result<T>(T value) => new(value, null);

    public static implicit operator Result<T>(AppError error) => new(default, error);
}

public sealed class Result
{
    private Result(AppError? error) => Error = error;

    public AppError? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static Result Success() => new(null);

    public static Result Failure(AppError error) => new(error);

    public static implicit operator Result(AppError error) => Failure(error);
}
