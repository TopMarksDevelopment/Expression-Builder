namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.NotEqualTest;

public class OperationTests
{
    static string ApplyReplacements(string input) =>
        input.Replace("new [] {", "[").Replace("}", "]");

    [Theory(DisplayName = "Factory")]
    [ClassData(typeof(AllTests))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(ApplyReplacements),
            match.Factory.ToMatchString<T>()
        );

    [Theory(DisplayName = "Filterable")]
    [ClassData(typeof(AllTests))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(ApplyReplacements),
            match.Filterable.ToMatchString<T>()
        );

    [Theory(DisplayName = "QueryFilterable")]
    [ClassData(typeof(AllTests))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(ApplyReplacements),
            match.QueryFilterable.ToMatchString<T>()
        );

    [Theory(DisplayName = "Fluent")]
    [ClassData(typeof(AllTests))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(ApplyReplacements),
            match.Fluent.ToMatchString<T>()
        );
}
