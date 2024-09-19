namespace ExpressionBuilder.Tests;

[Collection("Matching queries - Expressions (w/ manipulators)")]
public class ManipulatorExpressionMatchTests
{
    [Theory(DisplayName = "Factory")]
    [ClassData(typeof(ManipulatorTestData))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(),
            match.Factory.ToMatchString<T>()
        );

    [Theory(DisplayName = "Filterable")]
    [ClassData(typeof(ManipulatorTestData))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(),
            match.Filterable.ToMatchString()
        );

    [Theory(DisplayName = "QueryFilterable")]
    [ClassData(typeof(ManipulatorTestData))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(ManipulatorTestData.ApplyReplacements),
            match.QueryFilterable.ToMatchString()
        );

    [Theory(DisplayName = "Fluent")]
    [ClassData(typeof(ManipulatorTestData))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(ManipulatorTestData.ApplyReplacements),
            match.Fluent.ToMatchString()
        );
}
