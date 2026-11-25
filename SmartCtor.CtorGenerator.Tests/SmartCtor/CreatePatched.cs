using System.ComponentModel.DataAnnotations;
using SmartCtor.Monadics;

namespace SmartCtor.CtorGenerator.Tests;

[TestClass]
public class CreatePatched
{
    private static Option<Option<T>> Set<T>(T value) where T : notnull => new(new Option<T>(value));
    private static Option<Option<T>> SetNull<T>() where T : notnull => new(Option<T>.None);

    private static T AssertOk<T>(Result<T, List<ValidationResult>> result) where T : notnull =>
        result.Match(
            ok => ok,
            err => { Assert.Fail($"Should be okay. Errors: {string.Join("; ", err)}"); return default!; });

    private static List<ValidationResult> AssertErr<T>(Result<T, List<ValidationResult>> result) where T : notnull =>
        result.Match(
            ok => { Assert.Fail($"Should not be okay. Got: {ok}"); return default!; },
            err => err);

    [TestMethod]
    public void Record_EmptyPatch_KeepsAllValues()
    {
        var original = AssertOk(StubRecord.Create("Ana", 30));

        var patched = AssertOk(original.CreatePathced(new StubRecordOptional()));

        Assert.AreEqual(original, patched);
    }

    [TestMethod]
    public void Record_PatchSingleField_KeepsOthers()
    {
        var original = AssertOk(StubRecord.Create("Ana", 30));

        var patched = AssertOk(original.CreatePathced(new StubRecordOptional(Name: Set("Bia"))));

        Assert.AreEqual("Bia", patched.Name);
        Assert.AreEqual(30, patched.Age);
    }

    [TestMethod]
    public void Record_PatchAllFields_DoesNotMutateOriginal()
    {
        var original = AssertOk(StubRecord.Create("Ana", 30));

        var patched = AssertOk(original.CreatePathced(new StubRecordOptional(Set("Bia"), Set(41))));

        Assert.AreEqual("Bia", patched.Name);
        Assert.AreEqual(41, patched.Age);
        Assert.AreEqual("Ana", original.Name);
        Assert.AreEqual(30, original.Age);
    }

    [TestMethod]
    public void Record_SetNull_BecomesNull()
    {
        var original = AssertOk(StubRecord.Create("Ana", 30));

        var patched = AssertOk(original.CreatePathced(new StubRecordOptional(Name: SetNull<string>())));

        Assert.IsNull(patched.Name);
        Assert.AreEqual(30, patched.Age);
    }

    [TestMethod]
    public void Record_SetNullNullableValueType_BecomesNull()
    {
        var original = AssertOk(StubRecordNullableValue.Create("Ana", 30));

        var patched = AssertOk(original.CreatePathced(new StubRecordNullableValueOptional(Age: SetNull<int>())));

        Assert.IsNull(patched.Age);
        Assert.AreEqual("Ana", patched.Name);
    }

    [TestMethod]
    public void RecordValidated_InvalidPatch_ReturnsErrorForField()
    {
        var original = AssertOk(StringTests.Create("a", "b"));

        var errors = AssertErr(original.CreatePathced(new StringTestsOptional(Item1: Set(string.Empty))));

        Assert.HasCount(1, errors);
        Assert.Contains(e => e.MemberNames.Contains("Item1"), errors);
    }

    [TestMethod]
    public void RecordValidated_SetNullOnRequired_ReturnsError()
    {
        var original = AssertOk(StringTests.Create("a", "b"));

        var errors = AssertErr(original.CreatePathced(new StringTestsOptional(Item2: SetNull<string>())));

        Assert.HasCount(1, errors);
        Assert.Contains(e => e.MemberNames.Contains("Item2"), errors);
    }

    [TestMethod]
    public void Struct_PatchSingleField_KeepsOthers()
    {
        var original = AssertOk(StubStruct.Create(5, "Valid"));

        var patched = AssertOk(original.CreatePathced(new StubStructOptional(Name: Set("Novo nome"))));

        Assert.AreEqual(5, patched.Level);
        Assert.AreEqual("Novo nome", patched.Name);
    }

    [TestMethod]
    public void Struct_InvalidPatch_ReturnsErrors()
    {
        var original = AssertOk(StubStruct.Create(5, "Valid"));

        var errors = AssertErr(original.CreatePathced(new StubStructOptional(Set(0), Set("ab"))));

        Assert.HasCount(2, errors);
        Assert.Contains(e => e.MemberNames.Contains("Level"), errors);
        Assert.Contains(e => e.MemberNames.Contains("Name"), errors);
    }

    [TestMethod]
    public void Class_PatchSingleField_KeepsOthers()
    {
        var original = AssertOk(StubClassWithOptional.Create("ABC", "a@b.com", 2));

        var patched = AssertOk(original.CreatePathced(new StubClassWithOptionalOptional(Quantity: Set(10))));

        Assert.AreEqual("ABC", patched.Code);
        Assert.AreEqual("a@b.com", patched.Email);
        Assert.AreEqual(10, patched.Quantity);
        Assert.AreEqual(2, original.Quantity);
    }
}
