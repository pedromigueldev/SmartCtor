#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk

#:package Microsoft.CodeAnalysis.Analyzers@5.12.0
#:package Microsoft.CodeAnalysis.CSharp@5.12.0

#:property PublishAot=false
#:property TargetFramework=netstandard2.0
#:property LangVersion=latest
#:property OutputType=Library
#:property EnforceExtendedAnalyzerRules=true
#:property PackAsTool=false
#:property IncludeBuildOutput=false

#:include Generators/*.cs
#:include Monadics/*.cs
