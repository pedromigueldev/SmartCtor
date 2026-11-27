# SmartCtor

An AOT-safe C# source generator that creates validated smart constructors from DataAnnotations, alongside reusable monadic utilities.

## Features

- Roslyn incremental source generator.
- Generates a validated `Create(...)` factory returning `Result<T, List<ValidationResult>>`.
- Optional patch support (`GenerateOptional = true`) with `CreatePathced(...)`.
- Functional programming utilities (`Option` and `Result`).
- Modern C# (requires .NET 11 / C# unions).

## Example

This example will fail successfully with 3 errors.

```cs
using System.ComponentModel.DataAnnotations;
using SmartCtor.Monadics;
using SmartCtor.Attribute;

Result<Transaction, List<ValidationResult>> stub = Transaction.Create(1000, "12345", null!, DateTime.MinValue);

if (stub is Transaction) throw new Exception("Should not be okay");
if (stub is List<ValidationResult> { Count: < 3 } err)
{
    err.ForEach(Console.WriteLine);
    throw new Exception($"Should be 3 errors. Give {err.Count}");
}

stub.UnwrapErr().ForEach(Console.WriteLine);

[SmartCtor]
public partial record Transaction(
    [Required, Range(0, 100)] decimal Amount,
    [Required, StringLength(maximumLength: 10, MinimumLength = 6)] string Description,
    [Required] List<int> Categories,
    DateTime Date
);
```

> Types annotated with `[SmartCtor]` must be `partial`.

## Requirements

- .NET 11 SDK.
- Consuming projects must target `net11.0` or later.

## Clone the repository

```bash
git clone https://github.com/pedromigueldev/SmartCtor.git
cd SmartCtor
```

## Build and test

```bash
dotnet build SmartCtor.slnx
dotnet run --project SmartCtor.CtorGenerator.Tests/Tests.csproj
```

The generated sources of the test project are written to `SmartCtor.CtorGenerator.Tests/gen/` for inspection.

## Use it in another project

Import `SmartCtor.props` in your `.csproj`. It references both the monadics library and the source generator:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
  </PropertyGroup>

  <Import Project="path/to/SmartCtor/SmartCtor.props" />

</Project>
```

## License

This project is licensed under the LGPL-3.0 License.

---

Made with ❤️ by Pedro Miguel.
