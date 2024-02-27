namespace ExpressionBuilder.Tests;

[Collection("Operation checks")]
public class OperationChecks
{
    public static IEnumerable<object[]> GetAllOperations() =>
        AllTests.GetAllOperations();

    [Theory(DisplayName = "Factory")]
    [MemberData(nameof(GetAllOperations))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(AllTests.ApplyReplacements),
            match.Factory.ToMatchString<T>()
        );

    [Theory(DisplayName = "Filterable")]
    [MemberData(nameof(GetAllOperations))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(AllTests.ApplyReplacements),
            match.Filterable.ToMatchString<T>()
        );

    [Theory(DisplayName = "QueryFilterable")]
    [MemberData(nameof(GetAllOperations))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(AllTests.ApplyReplacements),
            match.QueryFilterable.ToMatchString<T>()
        );

    [Theory(DisplayName = "Fluent")]
    [MemberData(nameof(GetAllOperations))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(AllTests.ApplyReplacements),
            match.Fluent.ToMatchString<T>()
        );
}
