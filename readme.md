# SmartCtor

A C# file-based source generator designed to simplify constructor generation and reduce boilerplate, alongside reusable monadic utilities.

## Features

- Roslyn-based source generation.
- Simplified constructor-related code.
- Functional programming utilities (`Option` and `Result`).
- Modern C# implementation.

##  Clone the repository:

```bash
git clone https://github.com/pedromigueldev/SmartCtor.git
cd SmartCtor
```

##  Test the project 

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