namespace ExpressionBuilder.Tests;

[Collection("Matching queries - Expressions")]
public class ExpressionAllTests
{
    [Theory(DisplayName = "Factory")]
    [ClassData(typeof(AllTests))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(),
            match.Factory.ToMatchString<T>()
        );

    [Theory(DisplayName = "Filterable")]
    [ClassData(typeof(AllTests))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(),
            match.Filterable.ToMatchString()
        );

    [Theory(DisplayName = "QueryFilterable")]
    [ClassData(typeof(AllTests))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(),
            match.QueryFilterable.ToMatchString()
        );

    [Theory(DisplayName = "Fluent")]
    [ClassData(typeof(AllTests))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(),
            match.Fluent.ToMatchString()
        );
}
