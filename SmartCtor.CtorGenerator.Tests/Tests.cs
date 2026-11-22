#!/usr/bin/env dotnet

#:property PublishAot=true
#:package MSTest@4.5.3
#:property EmitCompilerGeneratedFiles=true
#:property CompilerGeneratedFilesOutputPath=gen
#:property OutputPath=bin
#:property EnableMSTestRunner=true
#:property WarningsAsErrors=Nullable

#:ref ../SmartCtor.CtorGenerator/CtorGenerator.cs OutputItemType=Analyzer
#:include ../SmartCtor.Monadics/Monadics.cs

#:include SmartCtor/*.cs
