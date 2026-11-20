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
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.TargetNode
        ).Where(e => e is not null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation, Execute);
    }

    private void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) tuple)
    {
        (var compilation, var list) = tuple;

        foreach (var syntax in list)
        { 
            var classSemanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

            var className = classSemanticModel.GetDeclaredSymbol(syntax)?.ToDisplayString();

            var classNamespace = classSemanticModel.GetTypeInfo(syntax).Type?.ContainingNamespace;

            var code = $$"""
                {{classNamespace}}
                using SmartCtor.Monadics;

                public static class {{className}}SmartCtor
                {
                    extension({{className}}) {
                        public static Result<{{className}}, FieldParseError[]> Create () {
                            throw new NotImplementedException();
                        }
                    }
                };

            """;

            context.AddSource($"{className}SmartCtor.g.cs", code);
        }
    }
}