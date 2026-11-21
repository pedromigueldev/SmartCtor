using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SmartCtor.Generators;

[Generator]
public class CtorGeneratorAttribute : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "SmartCtor.Attribute.SmartCtorAttribute",
            predicate: static (node, _) => node is TypeDeclarationSyntax,
            transform: static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
        ).Where(e => e is not null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(provider, Execute);
    }

    private static void Execute(SourceProductionContext context, INamedTypeSymbol type)
    {
        var namespaceName = type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace;
        var paramsList = type.TypeParameters.Select(e => $"{e.TypeKind} {e.Name}");

        var constructor = type.InstanceConstructors
            .FirstOrDefault(c => c.DeclaredAccessibility == Accessibility.Public);

        if (constructor is null)
            return;

        var constructorParams = JoinStringComma(constructor.Parameters.Select(e => $"{e.Type} {e.Name}"));
        var constructorParamsNames = JoinStringComma(constructor.Parameters.Select(e => $"{e.Name}"));
        var constructorParamsTypes = JoinStringComma(constructor.Parameters.Select(e => $"{e.Type}"));

        List<(string, string)> fieldAndValidator =
            [.. constructor.Parameters.Select(e => (e.Name, JoinStringComma(e.GetAttributes().Select(e => $"new {e.AttributeClass}({JoinStringComma(e.ConstructorArguments.Select(e => $"{e.Value}"))}) {(JoinStringComma(e.NamedArguments.Select(e => $"{e.Key} = {e.Value.Value}")) is { Length: > 0 } list ? $"{{ {list} }}" : "")}"))))];

        var tryCatches = fieldAndValidator.Select((n) => CreateTryBlock(n.Item1, n.Item2)).ToList();

        var code = $$"""
            {{(namespaceName?.ToDisplayString() is { Length: > 0 } ? $"namespace {namespaceName};" : "")}}
            using System.ComponentModel.DataAnnotations;
            using SmartCtor.Monadics;

            #nullable enable
            public static class {{type.Name}}SmartCtor
            {
                extension ({{type.Name}}) {
                    public static Result<{{type.Name}}, List<ValidationResult>> Create ({{constructorParams}}) {
                        var stub = new {{type.Name}}({{constructorParamsNames}});
                        var validationResults = new List<ValidationResult>();

                        {{string.Join("\n", tryCatches)}}

                        if (validationResults.Count > 0) return validationResults;
                        return stub;
                    }
                }
            }

            #nullable disable
        """;

        context.AddSource($"{type.Name}SmartCtor.g.cs", code);
    }

    public static string JoinStringComma(IEnumerable<string> strings) => string.Join(", ", strings);

    public static string CreateTryBlock(string member, string newAttributeExpression)
    {
        var code = $$"""
            try 
            {
                Validator.ValidateValue(stub.{{member}}, new (stub, nameof(stub.{{member}}), null, null), [{{newAttributeExpression}}]); 
            }
            catch (ValidationException ve)
            {
                validationResults.Add(ve.ValidationResult);
            }
        """;
    
        return code;
    }
}
