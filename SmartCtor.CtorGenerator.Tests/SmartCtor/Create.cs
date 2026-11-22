using System.ComponentModel.DataAnnotations;
using SmartCtor.Attribute;
using SmartCtor.Monadics;

namespace SmartCtor.CtorGenerator.Tests;

[SmartCtor]
public partial record StubUser([Range(0, 100)] int Identification, [Required] string? Name);

[SmartCtor(GenerateOptional = true)]
public partial record StringTests([Required] string Item1, [Required] string? Item2);

[SmartCtor]
public partial record Transaction (
    [Required, Range(0, 100)] decimal Amount, 
    [Required, StringLength(maximumLength: 10, MinimumLength = 6)] string Description, 
    [Required] List<int> Categories,
    DateTime Date
);

[SmartCtor(GenerateOptional = true)]
public partial struct StubStruct(
    [Range(1, 10)] int Level,
    [Required, StringLength(maximumLength: 20, MinimumLength = 3)] string Name
);

[SmartCtor]
public partial class StubClass(
    [Required] string Title,
    [Range(0, 1000)] decimal Price,
    [Required] List<string> Tags
);

[SmartCtor(GenerateOptional = true)]
public partial class StubClassWithOptional(
    [Required] string Code,
    [EmailAddress] string? Email,
    int Quantity = 1
);

[SmartCtor]
public partial struct StubStructSimple([Range(0, 255)] int Value);

[SmartCtor]
public partial class StubClassSimple([Required] string Value);

[TestClass]
public class Create
{

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Create_Fail()
    {
        var stub = StubUser.Create(-1, string.Empty);
        if (stub is StubUser) Assert.Fail("Should not be okay");
        Assert.IsGreaterThanOrEqualTo(2, stub.UnwrapErr().Count);
    }

    [TestMethod]
    public void Create_Fail_Strings()
    {
        var stub = StringTests.Create(string.Empty, null);
        if (stub is StringTests) Assert.Fail("Should not be okay");
        Assert.IsGreaterThanOrEqualTo(2, stub.UnwrapErr().Count);
    }

    [TestMethod]
    public void Create_Transaction_Ok()
    {
        var stub = Transaction.Create(50, "123456", [], DateTime.Now);

        if (stub is List<ValidationResult> err)
        {
            err.ForEach(Console.WriteLine);
            Assert.Fail("Should not be failing");
        }

        var res = stub.Unwrap();
        Assert.AreEqual(50, res.Amount);
        Assert.AreEqual("123456", res.Description);
        Assert.IsEmpty(res.Categories);
    }

    [TestMethod]
    public void Create_Transaction_Err()
    {
        var stub = Transaction.Create(1000, "12345", null!, DateTime.MinValue);

        if (stub is Transaction) Assert.Fail("Should not be okay");
        if (stub is List<ValidationResult>{ Count : < 3 } err)
        {
            err.ForEach(e => TestContext.WriteLine($"{e}"));
            Assert.Fail($"Should be 3 errors. Give {err.Count}");
        }

        stub.UnwrapErr().ForEach(Console.WriteLine);
    }
}

[TestClass]
public class Create_Struct
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Create_Struct_Ok()
    {
        var stub = StubStruct.Create(5, "Valid");

        if (stub is List<ValidationResult> err)
        {
            err.ForEach(Console.WriteLine);
            Assert.Fail("Should not be failing");
        }

        var res = stub.Unwrap();
        Assert.AreEqual(5, res.Level);
        Assert.AreEqual("Valid", res.Name);
    }

    [TestMethod]
    public void Create_Struct_Fail_BothFields()
    {
        var stub = StubStruct.Create(0, "ab");

        if (stub is StubStruct) Assert.Fail("Should not be okay");
        if (stub is List<ValidationResult>{ Count: < 2 } err)
        {
            err.ForEach(e => TestContext.WriteLine($"{e}"));
            Assert.Fail($"Should be 2 errors. Got {err.Count}");
        }
    }

    [TestMethod]
    public void Create_Struct_Fail_NameOnly()
    {
        var stub = StubStruct.Create(5, "");

        if (stub is StubStruct) Assert.Fail("Should not be okay");
        var errors = stub.UnwrapErr();
        Assert.HasCount(1, errors);
    }
}

[TestClass]
public class Create_Class
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Create_Class_Ok()
    {
        var stub = StubClass.Create("Book", 19.99m, ["fiction", "bestseller"]);

        if (stub is List<ValidationResult> err)
        {
            err.ForEach(Console.WriteLine);
            Assert.Fail("Should not be failing");
        }

        var res = stub.Unwrap();
        Assert.AreEqual("Book", res.Title);
        Assert.AreEqual(19.99m, res.Price);
        Assert.HasCount(2, res.Tags);
    }

    [TestMethod]
    public void Create_Class_Fail_Price()
    {
        var stub = StubClass.Create("Book", 5000m, []);

        if (stub is StubClass) Assert.Fail("Should not be okay");
        var errors = stub.UnwrapErr();
        Assert.IsGreaterThanOrEqualTo(1, errors.Count);
        Assert.Contains(e => e.MemberNames.Contains("Price"), errors);
    }

    [TestMethod]
    public void Create_Class_Fail_All()
    {
        var stub = StubClass.Create("", -1m, null!);

        if (stub is StubClass) Assert.Fail("Should not be okay");
        if (stub is List<ValidationResult>{ Count: < 3 } err)
        {
            err.ForEach(e => TestContext.WriteLine($"{e}"));
            Assert.Fail($"Should be 3 errors. Got {err.Count}");
        }
    }

    [TestMethod]
    public void Create_Class_WithOptional_Ok_Defaults()
    {
        var stub = StubClassWithOptional.Create("ABC", null);

        if (stub is List<ValidationResult> err)
        {
            err.ForEach(Console.WriteLine);
            Assert.Fail("Should not be failing");
        }

        var res = stub.Unwrap();
        Assert.AreEqual("ABC", res.Code);
        Assert.AreEqual(1, res.Quantity);
    }
    
    [TestMethod]
    public void Create_Class_WithOptional_Fail_Email()
    {
        var stub = StubClassWithOptional.Create("ABC", "not-an-email", 5);

        if (stub is StubClassWithOptional) Assert.Fail("Should not be okay");
        var errors = stub.UnwrapErr();

        Assert.Contains(e => e.MemberNames.Contains("Email"), errors);
    }
}

[TestClass]
public class Create_SimpleTypes
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Create_SimpleStruct_Ok()
    {
        var stub = StubStructSimple.Create(128);

        if (stub is List<ValidationResult> err)
        {
            err.ForEach(Console.WriteLine);
            Assert.Fail("Should not be failing");
        }

        Assert.AreEqual(128, stub.Unwrap().Value);
    }

    [TestMethod]
    public void Create_SimpleStruct_Fail()
    {
        var stub = StubStructSimple.Create(200);
        // Range(0,255) aceita 200 — use um valor fora do range
        var stubInvalid = StubStructSimple.Create(300);
        _ = stub; // 200 é válido

        if (stubInvalid is StubStructSimple) Assert.Fail("Should not be okay");
        Assert.HasCount(1, stubInvalid.UnwrapErr());
    }

    [TestMethod]
    public void Create_SimpleClass_Ok()
    {
        var stub = StubClassSimple.Create("hello");

        if (stub is List<ValidationResult> err)
        {
            err.ForEach(Console.WriteLine);
            Assert.Fail("Should not be failing");
        }

        Assert.AreEqual("hello", stub.Unwrap().Value);
    }

    [TestMethod]
    public void Create_SimpleClass_Fail()
    {
        var stub = StubClassSimple.Create(string.Empty);

        if (stub is StubClassSimple) Assert.Fail("Should not be okay");
        Assert.HasCount(1, stub.UnwrapErr());
    }
}