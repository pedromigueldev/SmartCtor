
using System;

namespace SmartCtor.Monadics;

public readonly record struct None;

[System.Runtime.CompilerServices.Union]
public readonly record struct Option<T> : System.Runtime.CompilerServices.IUnion
    where T : notnull
{
    public static Option<T> None => new(default(None));
    private readonly T _value = default!;
    private readonly None _none;
    private readonly byte _tag;

    public Option(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _value = value;
        _none = default;
        _tag = 1;
    }

    public Option(None value)
    {
        _value = default!;
        _none = value;
        _tag = 0;
    }

    public object? Value => _tag switch
    {
        1 => _value,
        0 => _none,
        _ => null
    };

    public bool HasValue => _tag == 1;

    public bool TryGetValue(out T value)
    {
        value = _value;
        return _tag == 1;
    }

    public bool TryGetValue(out None value)
    {
        value = _none;
        return _tag == 0;
    }
}