namespace ExpressionBuilder.Tests;
using Xunit;

[Collection("Matching queries - Results")]
public class EfCoreTests : EfCoreTestBase
{
    [Theory(DisplayName = "Factory")]
    [ClassData(typeof(CoreTestData))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Factory.ToResults(items)
        );
    }

    [Theory(DisplayName = "Filterable")]
    [ClassData(typeof(CoreTestData))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Filterable.ToResults(items)
        );
    }

    [Theory(DisplayName = "QueryFilterable")]
    [ClassData(typeof(CoreTestData))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.QueryFilterable.ToResults(items)
        );
    }

    [Theory(DisplayName = "Fluent")]
    [ClassData(typeof(CoreTestData))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Fluent.ToResults(items)
        );
    }
}
