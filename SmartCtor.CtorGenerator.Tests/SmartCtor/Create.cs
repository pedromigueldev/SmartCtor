using System.ComponentModel.DataAnnotations;
using SmartCtor.Attribute;
using SmartCtor.Monadics;

namespace SmartCtor.CtorGenerator.Tests;

[SmartCtor]
public record StubUser([Range(0, 100)] int Identification, [Required] string? Name);

[SmartCtor]
public record StringTests([Required] string Item1, [Required] string? Item2);

[SmartCtor]
public partial record Transaction (
    [Required, Range(0, 100)] decimal Amount, 
    [Required, StringLength(maximumLength: 10, MinimumLength = 6)] string Description, 
    [Required] List<int> Categories,
    DateTime Date
);

[TestClass]
public class Create
{

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Create_Fail()
    {
        var stub = StubUser.Create(-1, string.Empty);
        if (stub is Ok<StubUser>) Assert.Fail("Should not be okay");
        Assert.IsGreaterThanOrEqualTo(2, stub.UnwrapErr().Count);
    }

    [TestMethod]
    public void Create_Fail_Strings()
    {
        var stub = StringTests.Create(string.Empty, null);
        if (stub is Ok<StringTests>) Assert.Fail("Should not be okay");
        Assert.IsGreaterThanOrEqualTo(2, stub.UnwrapErr().Count);
    }

    [TestMethod]
    public void Create_Transaction_Ok()
    {
        var stub = Transaction.Create(50, "123456", [], DateTime.Now);

        if (stub is Err<List<ValidationResult>>(var err))
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

        if (stub is Ok<Transaction>) Assert.Fail("Should not be okay");
        if (stub is Err<List<ValidationResult>>({ Count : < 3 } err))
        {
            err.ForEach(e => TestContext.WriteLine($"{e}"));
            Assert.Fail($"Should be 3 errors. Give {err.Count}");
        }

        stub.UnwrapErr().ForEach(Console.WriteLine);
    }
}