using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SmartCtor.Generators;

[Generator]
public class TestGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
        )
        .Where(m => m is not null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation, Execute);
    }

    private void Execute(SourceProductionContext context, 
        (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) tuple)
    {
        (var compilation, var list) = tuple;

        List<string> nameList = [.. list.Select(syntax => {
            var symbol = compilation
                .GetSemanticModel(syntax.SyntaxTree)
                .GetDeclaredSymbol(syntax) as INamedTypeSymbol;

            return $"\"{symbol!.ToDisplayString()}\"";
        })];

        var theCode = $$"""
        namespace ClassListGenerator;

        public static class ClassNames
        {
            public static List<String> Names = [{{string.Join(", ", nameList)}}];
        }
        """;

        context.AddSource("ClassListGenerator.g.cs", theCode);
    }
}