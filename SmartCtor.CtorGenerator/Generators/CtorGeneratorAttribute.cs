using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SmartCtor.Generators;

[Generator]
public class CtorGeneratorAttribute : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {

        context.RegisterPostInitializationOutput(ctx =>
        {
            var theCode = $$"""
            namespace SmartCtor.Attribute;

            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class SmartCtorAttribute : System.Attribute {
                public bool GenerateOptional = false;
            };
            """;

            ctx.AddSource("SmartCtorAttribute.g.cs", theCode);
        });

        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "SmartCtor.Attribute.SmartCtorAttribute",
            predicate: static (node, _) => node is TypeDeclarationSyntax,
            transform: static (ctx, _) => {
            var attribute = ctx.Attributes[0];
            var generateOptional = attribute.NamedArguments.FirstOrDefault(x => x.Key == "GenerateOptional").Value.Value as bool? ?? false;
            return (Symbol: (INamedTypeSymbol)ctx.TargetSymbol, GenerateOptional: generateOptional);
        });

        var compilation = context.CompilationProvider.Combine(provider.Collect());
        context.RegisterSourceOutput(provider, Execute);
    }

    private static void Execute(SourceProductionContext context, (INamedTypeSymbol type, bool generateOptional) value) {
        (var type, var generateOptional) = value;

        var namespaceName = type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace;
        var paramsList = type.TypeParameters.Select(e => $"{e.TypeKind} {e.Name}");

        var constructor = type.InstanceConstructors
            .FirstOrDefault(c => c.DeclaredAccessibility == Accessibility.Public);

        if (constructor is null)
            return;

        var classType = type.IsRecord 
            ? type.IsReferenceType ? "record class" : "record struct"
            : type.IsReferenceType ? "class" : "struct";

        var constructorParams = JoinStringComma(constructor.Parameters.Select(e => $"{e.Type} {e.Name}"));
        var constructorParamsMayHaveDefault = JoinStringComma(constructor.Parameters.Select(e => $"{e.Type} {e.Name} {(e.HasExplicitDefaultValue ? $"= {e.ExplicitDefaultValue}" : "")}"));
        var constructorParamsNames = JoinStringComma(constructor.Parameters.Select(e => $"{e.Name}"));
        var constructorParamsTypes = JoinStringComma(constructor.Parameters.Select(e => $"{e.Type}"));

        List<(string, string)> fieldAndValidator =
            [.. constructor.Parameters.Select(e => (e.Name, JoinStringComma(e.GetAttributes().Select(e => $"new {e.AttributeClass}({JoinStringComma(e.ConstructorArguments.Select(e => $"{e.Value}"))}) {(JoinStringComma(e.NamedArguments.Select(e => $"{e.Key} = {e.Value.ToCSharpString()}")) is { Length: > 0 } list ? $"{{ {list} }}" : "")}"))))];

        var arrays = fieldAndValidator.Where(e => !string.IsNullOrWhiteSpace(e.Item2)).Select((n) => CreateStaticValidationArray(n.Item1, n.Item2)).ToList();
        var validations = fieldAndValidator.Where(e => !string.IsNullOrWhiteSpace(e.Item2)).Select((n) => CreateValidations(n.Item1)).ToList();
        var nonRecordFields = string.Join("\n", constructor.Parameters.Select(e => $"public {e.Type} {e.Name} = {e.Name};"));


        var code = $$"""
            {{(namespaceName?.ToDisplayString() is { Length: > 0 } ? $"namespace {namespaceName};" : "")}}
            using System.ComponentModel.DataAnnotations;
            using SmartCtor.Monadics;

            #nullable enable
            public partial {{classType}} {{type.Name}}
            {
                {{(!type.IsRecord ? nonRecordFields : "")}}

                {{(type.IsReferenceType ? CreateEmptyConstructor(type.Name, constructor) : "")}}

                public static Result<{{type.Name}}, List<ValidationResult>> Create ({{constructorParamsMayHaveDefault}}) {
                    var stub = new {{type.Name}}({{constructorParamsNames}});
                    var stub_context = new ValidationContext(stub, nameof({{type.Name}}), null, null);
                    var validationResults = new List<ValidationResult>();
                    {{string.Join("\n", arrays)}}
                    {{string.Join("\n", validations)}}
                    if (validationResults.Count > 0) return validationResults;
                    return stub;
                }
            }

            {{(generateOptional ? GenerateOptional(classType, type.Name, constructor) : "")}}

            #nullable disable
        """;

        context.AddSource($"{type.Name}SmartCtor.g.cs", code);
    }

    public static string JoinStringComma(IEnumerable<string> strings) => string.Join(", ", strings);

    public static string CreateValidations(string member) {
        return $$"""
            stub_context.MemberName = "{{member}}";
            Validator.TryValidateValue({{member}}, stub_context, validationResults, _{{member}});
        """;
    }

    public static string CreateStaticValidationArray(string member, string newAttributeExpression) {
        return $$"""
            ValidationAttribute[] _{{member}} = [{{newAttributeExpression}}];
        """;
    }

    public static string CreateContextArray(string member) {
        return $$"""
            ValidationContext _{{member}}_Context = new (stub, _stub_type, null, null) { MemberName = "{{member}}" };
        """;
    }

    public static string CreateEmptyConstructor(string name, IMethodSymbol namedType) {

        var defautls = string.Join(", ", namedType.Parameters.Select(e => $"{(e.HasExplicitDefaultValue ? $"{e.ExplicitDefaultValue}" : $"({e.Type.ToDisplayString()})default!")}"));
        return $$"""
            private {{name}} ()
                : this ({{defautls}}) { }
        """;
    }

    public static string GenerateOptional (string classType, string name, IMethodSymbol constructor) {
        static string GetOptionalOfType (IParameterSymbol e) {
            var type = e.Type is INamedTypeSymbol namedType &&
                namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T 
                ? namedType.TypeArguments[0]
                : e.Type;


            type = type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);

            return $"Option<Option<{type.ToDisplayString()}>> {e.Name}";
        };

        var optionalFields = string.Join("\n", constructor.Parameters.Select(e =>
            $"public {GetOptionalOfType(e)} = {e.Name};"));
            
        var constructorParams = JoinStringComma(constructor.Parameters.Select(e => $"{GetOptionalOfType(e)} = default!"));

        return $$"""
        public partial {{classType}} {{name}}Optional({{constructorParams}}) {
            {{optionalFields}}
        }
        """;
    }
}
