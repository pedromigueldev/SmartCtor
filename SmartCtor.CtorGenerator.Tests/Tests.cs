#!/usr/bin/env dotnet

#:property EmitCompilerGeneratedFiles=true
#:property CompilerGeneratedFilesOutputPath=gen
#:property OutputPath=bin
#:ref ../SmartCtor.CtorGenerator/CtorGenerator.cs OutputItemType=Analyzer
#:include ../SmartCtor.Monadics/Monadics.cs

using SmartCtor.Attribute;

var s = Something2.Create();
Console.WriteLine("Compilado com sucesso!");

[SmartCtor]
public class Something() {}
[SmartCtor]
public class Something2 {}
public class Something3 {}
public class Something4 {}