namespace SmartCtor.Monadics;
public readonly record struct Some<T>(T Value) where T : notnull;
public readonly record struct None;

public readonly union Option<T>(None, Some<T>) where T : notnull
{
    public static implicit operator Option<T>(T? value) => value is null ? new Option<T>(new None()) : new Option<T>(new Some<T>(value));
    public static implicit operator Option<T>(None none) => new(none);
    public override string ToString() => this switch
    {
        Some<T>(var value) => $"Some({value?.ToString() ?? "null"})",
        None => "None"
    };
}