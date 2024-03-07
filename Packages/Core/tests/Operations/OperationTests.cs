namespace ExpressionBuilder.Tests;

[Collection("Operation checks")]
public class OperationTests
{
    [Theory(DisplayName = "Factory")]
    [ClassData(typeof(OperationTestData))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(OperationTestData.ApplyReplacements),
            match.Factory.ToMatchString<T>()
        );

    [Theory(DisplayName = "Filterable")]
    [ClassData(typeof(OperationTestData))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(OperationTestData.ApplyReplacements),
            match.Filterable.ToMatchString()
        );

    [Theory(DisplayName = "QueryFilterable")]
    [ClassData(typeof(OperationTestData))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var xx = match.Expected.ToMatchString(
            OperationTestData.ApplyReplacements
        );
        var yy = match.QueryFilterable.ToMatchString();

        Assert.Equal(
            match.Expected.ToMatchString(OperationTestData.ApplyReplacements),
            match.QueryFilterable.ToMatchString()
        );
    }

    [Theory(DisplayName = "Fluent")]
    [ClassData(typeof(OperationTestData))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(OperationTestData.ApplyReplacements),
            match.Fluent.ToMatchString()
        );
}
