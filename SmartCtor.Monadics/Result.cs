using System.Diagnostics;

namespace SmartCtor.Monadics;
public record FieldParseError(string Field, string Error);
public readonly record struct Ok<T> (in T Some);
public readonly record struct Err<T> (in T Error);

public readonly union Result<T, E>(Ok<T>, Err<E>)
{
    public static implicit operator Result<T, E>(in T value) => new Ok<T>(value);
    public static implicit operator Result<T, E>(in E error) => new Err<E>(error);
    public readonly Result<U, E> Map<U>(Func<T, U> f) => this switch
    {
        Ok<T>(var some) => f(some),
        Err<E> error => error
    };

    public readonly Result<U, E> AndThen<U>(Func<T, Result<U, E>> f) => this switch
    {
        Ok<T>(var some) => f(some),
        Err<E> error => error
    };

    public readonly async Task<Result<U, E>> MapAsync<U>(Func<T, Task<U>> f) => this switch
    {
        Ok<T>(var some) => await f(some),
        Err<E> error => error
    };

    public readonly Result<T, U> MapError<U>(Func<E, U> f) => this switch
    {
        Ok<T>(var some) => some,
        Err<E>(var error) => f(error),
    };

    public readonly U Match<U>(Func<T, U> onSuccess, Func<E, U> onError) => this switch
    {
        Ok<T>(var some) => onSuccess(some),
        Err<E>(var error) => onError(error)
    };

    public readonly T Unwrap() => this switch
    {
        Ok<T>(var some) => some,
        Err<E>(var error) => Panic<T>($"Called Unwrap on an Err: {error}")
    };

    public readonly E UnwrapErr() => this switch
    {
        Ok<T>(var some) => Panic<E>($"Called UnwrapErr on an Ok: {some}"),
        Err<E>(var error) => error
    };

    private static Some Panic<Some>(string exception)
    {
        try
        {
            throw new UnreachableException(exception);
        }
        catch
        {
            Environment.Exit(0);
            return default;
        }
    }
};