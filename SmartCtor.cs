#:property LangVersion=latest
#:property PackAsTool=false
#:property WarningsAsErrors=Nullable

#:include SmartCtor.Monadics/*.cs
#:ref SmartCtor.CtorGenerator/CtorGenerator.cs OutputItemType=Analyzer ReferenceOutputAssembly=false

// SmartCtor should be included not executed