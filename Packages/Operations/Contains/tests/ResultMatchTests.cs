namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;

[Collection("Matching queries - Results")]
public class ResultAllTests : IClassFixture<ResultMatchSeed>
{
    ICollection<Product> _products { get; set; }
    ICollection<Category> _categories { get; set; }

    public ResultAllTests(ResultMatchSeed seed)
    {
        _products = seed.Products;

        _categories = seed.Categories;
    }

    ICollection<T> GetCollection<T>()
        where T : class =>
        typeof(T) == typeof(Product)
            ? (ICollection<T>)_products
            : (ICollection<T>)_categories;

    public static IEnumerable<object[]> GetAll() => AllTests.GetAllMatchers();

    [Theory(DisplayName = "Factory")]
    [MemberData(nameof(GetAll))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = GetCollection<T>();
        
        var xx = match.Expected.ToResult(items);
        var yy = match.Factory.ToResults(items);

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Factory.ToResults(items)
        );
    }

    [Theory(DisplayName = "Filterable")]
    [MemberData(nameof(GetAll))]
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
    [MemberData(nameof(GetAll))]
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
    [MemberData(nameof(GetAll))]
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
