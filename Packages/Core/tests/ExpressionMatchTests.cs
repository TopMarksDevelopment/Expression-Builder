namespace ExpressionBuilder.Tests;

using Xunit;

[Collection("Matching queries - Expressions")]
public class ExpressionAllTests
{
    [Theory(DisplayName = "Factory")]
    [ClassData(typeof(CoreTestData))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(CoreTestData.ApplyReplacements),
            match.Factory.ToMatchString<T>()
        );

    [Theory(DisplayName = "Filterable")]
    [ClassData(typeof(CoreTestData))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(CoreTestData.ApplyReplacements),
            match.Filterable.ToMatchString()
        );

    [Theory(DisplayName = "QueryFilterable")]
    [ClassData(typeof(CoreTestData))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(CoreTestData.ApplyReplacements),
            match.QueryFilterable.ToMatchString()
        );

    [Theory(DisplayName = "Fluent")]
    [ClassData(typeof(CoreTestData))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(CoreTestData.ApplyReplacements),
            match.Fluent.ToMatchString()
        );
}
