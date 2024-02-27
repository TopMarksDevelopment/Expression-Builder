
namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.EqualTest;

public class OperationTests
{
    public static IEnumerable<object[]> GetAll() =>
        AllTests.GetAllOperations();

    static string ApplyReplacements(string input) =>
        input.Replace("new [] {", "[")
            .Replace("}", "]");

    [Theory(DisplayName = "Factory")]
    [MemberData(nameof(GetAll))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            ApplyReplacements(match.Expected.ToMatchString()),
            match.Factory.ToMatchString<T>()
        );

    [Theory(DisplayName = "Filterable")]
    [MemberData(nameof(GetAll))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            ApplyReplacements(match.Expected.ToMatchString()),
            match.Filterable.ToMatchString<T>()
        );

    [Theory(DisplayName = "QueryFilterable")]
    [MemberData(nameof(GetAll))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            ApplyReplacements(match.Expected.ToMatchString()),
            match.QueryFilterable.ToMatchString<T>()
        );

    [Theory(DisplayName = "Fluent")]
    [MemberData(nameof(GetAll))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            ApplyReplacements(match.Expected.ToMatchString()),
            match.Fluent.ToMatchString<T>()
        );
}
