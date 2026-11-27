using SmartCtor.Attribute;

namespace SmartCtor.CtorGenerator.Tests;

[SmartCtor(GenerateOptional = true)]
public partial record StubRecord(string? Name, int Age);

[SmartCtor(GenerateOptional = true)]
public partial record StubRecordNullableValue(string Name, int? Age);

