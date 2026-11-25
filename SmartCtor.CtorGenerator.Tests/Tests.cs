#!/usr/bin/env dotnet

#:property PublishAot=true
#:package MSTest@4.5.3
#:property EmitCompilerGeneratedFiles=true
#:property CompilerGeneratedFilesOutputPath=gen
#:property OutputPath=bin
#:property EnableMSTestRunner=true
#:property WarningsAsErrors=Nullable


#:include ../SmartCtor.cs
#:include SmartCtor/*.cs
