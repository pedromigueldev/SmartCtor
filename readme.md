# SmartCtor

An AOT safe C# file-based source generator designed to simplify constructor generation and reduce boilerplate, alongside reusable monadic utilities.

## Features

- Roslyn-based source generation.
- Simplified constructor-related code.
- Functional programming utilities (`Option` and `Result`).
- Modern C# implementation.

## Exemple

This exemple will fail successfully with 3 errors.
```cs
using System.ComponentModel.DataAnnotations;
using SmartCtor.Monadics;
using SmartCtor.Attribute;

Result<Transaction, List<ValidationResult>> stub = Transaction.Create(1000, "12345", null!, DateTime.MinValue);

if (stub is Ok<Transaction>) throw new Exception("Should not be okay");
if (stub is Err<List<ValidationResult>>({ Count : < 3 } err))
{
    err.ForEach(Console.WriteLine);
    throw new Exception($"Should be 3 errors. Give {err.Count}");
}

stub.UnwrapErr().ForEach(Console.WriteLine);

[SmartCtor]
public record Transaction (
    [Required, Range(0, 100)] decimal Amount, 
    [Required, StringLength(maximumLength: 10, MinimumLength = 6)] string Description, 
    [Required] List<int> Categories,
    DateTime Date
);
```
## Clone the repository:

```bash
git clone https://github.com/pedromigueldev/SmartCtor.git
cd SmartCtor
```

## Test the project 

```bash
dotnet SmartCtor.CtorGenerator.Tests/Tests.cs
```

To use this project in a file based c# projet just include the path to the SmartCtor.cs

```cs
#:include Vendor/SmartCtor/SmartCtor.cs
```

## Project Structure

- `SmartCtor.CtorGenerator` — Source generator.
- `SmartCtor.Monadics` — Monadic utilities.
- `SmartCtor.CtorGenerator.Tests` — Generator tests.

## License

This project is licensed under the LGPL-3.0 License.

---

Made with ❤️ by Pedro Miguel.