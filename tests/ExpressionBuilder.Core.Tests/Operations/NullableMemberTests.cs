namespace ExpressionBuilder.Tests;

/// <summary>
/// Properties such as <c>int?</c> are compared to plain values (<c>5</c>). Those used to fail with
/// "the binary operator Equal is not defined for Nullable&lt;int&gt; and int".
/// </summary>
[Collection("Nullable members")]
public class NullableMemberTests
{
    static readonly DateTime Created = new(2024, 5, 6, 7, 8, 9);

    static List<Product> Products() =>
        [
            new(1, createdAt: Created) { BrandId = 10 },
            new(2, createdAt: null) { BrandId = null },
            new(3, createdAt: Created.AddDays(1)) { BrandId = 20 },
            new(4, createdAt: Created) { BrandId = 10 },
        ];

    static List<int> Ids(IEnumerable<Product> products) =>
        [.. products.Select(p => p.Id).Order()];

    [Fact(DisplayName = "Equal: a nullable number equals a number")]
    public void EqualNumber()
    {
        var filter = new Filter<Product>();
        filter.Equal(x => x.BrandId, 10);

        Assert.Equal<int>([1, 4], Ids(Products().Where(filter)));
    }

    [Fact(DisplayName = "Equal: a nullable date equals a date")]
    public void EqualDate()
    {
        var filter = new Filter<Product>();
        filter.Equal(x => x.CreatedAt, Created);

        Assert.Equal<int>([1, 4], Ids(Products().Where(filter)));
    }

    [Fact(DisplayName = "Equal: a nullable number equals null")]
    public void EqualNull()
    {
        var filter = new Filter<Product>();
        filter.Equal<Product, int?>(x => x.BrandId, (int?)null);

        Assert.Equal<int>([2], Ids(Products().Where(filter)));
    }

    [Fact(
        DisplayName = "Equal: a nullable number equals one of several numbers"
    )]
    public void EqualSeveral()
    {
        var filter = new Filter<Product>();
        filter.Equal(x => x.BrandId, [10, 20]);

        Assert.Equal<int>([1, 3, 4], Ids(Products().Where(filter)));
    }

    [Fact(DisplayName = "NotEqual: a nullable number does not equal a number")]
    public void NotEqualNumber()
    {
        var filter = new Filter<Product>();
        filter.NotEqual(x => x.BrandId, 10);

        // Nulls are not equal to 10 either
        Assert.Equal<int>([2, 3], Ids(Products().Where(filter)));
    }

    [Fact(DisplayName = "NotEqual: a nullable date does not equal a date")]
    public void NotEqualDate()
    {
        var filter = new Filter<Product>();
        filter.NotEqual(x => x.CreatedAt, Created);

        Assert.Equal<int>([2, 3], Ids(Products().Where(filter)));
    }

    [Fact(
        DisplayName = "NotEqual: a nullable number is none of several numbers"
    )]
    public void NotEqualSeveral()
    {
        var filter = new Filter<Product>();
        filter.NotEqual(x => x.BrandId, [10, 20]);

        Assert.Equal<int>([2], Ids(Products().Where(filter)));
    }

    [Fact(DisplayName = "By name, as a string reference")]
    public void ByName()
    {
        var filter = new Filter<Product>();
        filter
            .Equal("BrandId", 20)
            .Or()
            .NotEqual("CreatedAt", Created.AddDays(1));

        Assert.Equal<int>([1, 2, 3, 4], Ids(Products().Where(filter)));
    }

    [Fact(DisplayName = "It also builds for a database query")]
    public void AsAQuery()
    {
        var filter = new Filter<Product>();
        filter
            .Equal(x => x.BrandId, 10)
            .And()
            .NotEqual(x => x.CreatedAt, Created.AddDays(1));

        Assert.Equal<int>([1, 4], Ids(Products().AsQueryable().Where(filter)));
    }
}
