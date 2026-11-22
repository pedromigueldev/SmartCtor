using System.Diagnostics;

namespace SmartCtor.Monadics;
public record FieldParseError(string Field, string Error);

public readonly union Result<T, E>(T, E) where T : notnull  where E : notnull
{
    public readonly Result<U, E> Map<U>(Func<T, U> f) where U : notnull => this switch
    {
        T some => f(some),
        E error => error
    };

    public readonly Result<U, E> AndThen<U>(Func<T, Result<U, E>> f) where U : notnull => this switch
    {
        T some => f(some),
        E error => error
    };

    public readonly async Task<Result<U, E>> MapAsync<U>(Func<T, Task<U>> f) where U : notnull => this switch
    {
        T some => await f(some),
        E error => error
    };

    public readonly Result<T, U> MapError<U>(Func<E, U> f) where U : notnull => this switch
    {
        T some => some,
        E error => f(error),
    };

    public readonly U Match<U>(Func<T, U> onSuccess, Func<E, U> onError) => this switch
    {
        T some => onSuccess(some),
        E error => onError(error)
    };

    public readonly T Unwrap() => this switch
    {
        T some => some,
        E error => Panic<T>($"Called Unwrap on an Err: {error}")
    };

    public readonly E UnwrapErr() => this switch
    {
        T some => Panic<E>($"Called UnwrapErr on an Ok: {some}"),
        E error => error
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