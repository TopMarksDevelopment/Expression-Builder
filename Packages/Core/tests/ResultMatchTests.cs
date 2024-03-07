namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using Xunit;

[Collection("Matching queries - Results")]
public class ResultAllTests(ResultMatchSeed seed)
    : IClassFixture<ResultMatchSeed>
{
    readonly ICollection<Product> _products = seed.Products;
    readonly ICollection<Category> _categories = seed.Categories;
    readonly ICollection<StockLocation> _stockLocations = seed.StockLocations;

    ICollection<T> GetCollection<T>()
        where T : class =>
        typeof(T) == typeof(Product)
            ? (ICollection<T>)_products
            : (
                typeof(T) == typeof(StockLocation)
                    ? (ICollection<T>)_stockLocations
                    : (ICollection<T>)_categories
            );

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
