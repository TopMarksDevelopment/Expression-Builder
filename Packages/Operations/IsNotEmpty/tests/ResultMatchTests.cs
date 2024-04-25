namespace ExpressionBuilder.Tests;
using ExpressionBuilder.Tests.Models;

[Collection("Matching queries - Results")]
public class ResultAllTests(ResultMatchSeed seed) : IClassFixture<ResultMatchSeed>
{
    readonly ICollection<Product> _products = seed.Products;
    readonly ICollection<Category> _categories = seed.Categories;

    ICollection<T> GetCollection<T>()
        where T : class =>
        typeof(T) == typeof(Product)
            ? (ICollection<T>)_products
            : (ICollection<T>)_categories;

    [Theory(DisplayName = "Factory")]
    [ClassData(typeof(AllTests))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = seed.GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Factory.ToResults(items)
        );
    }

    [Theory(DisplayName = "Filterable")]
    [ClassData(typeof(AllTests))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = seed.GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Filterable.ToResults(items)
        );
    }

    [Theory(DisplayName = "QueryFilterable")]
    [ClassData(typeof(AllTests))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = seed.GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.QueryFilterable.ToResults(items)
        );
    }

    [Theory(DisplayName = "Fluent")]
    [ClassData(typeof(AllTests))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = seed.GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Fluent.ToResults(items)
        );
    }
}
