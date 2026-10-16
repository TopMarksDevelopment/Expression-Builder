namespace ExpressionBuilder.Tests;

[Collection("Case manipulators")]
public class CaseManipulatorTests
{
    static readonly IEnumerable<Product> Products =
        new ResultMatchSeed().Products;

    static int[] Ids(Filter<Product> filter) =>
        [.. Products.Where(filter).Select(p => p.Id).Order()];

    [Fact(
        DisplayName = "Contains still ignores case when the member is upper-cased"
    )]
    public void ContainsWithToUpper()
    {
        var filter = new Filter<Product>();

        filter.Contains(x => x.Name.ToUpper(), "PRODUCT 2");

        Assert.Equal<int>([2], Ids(filter));
    }

    [Fact(
        DisplayName = "Contains still ignores case when the member is lower-cased"
    )]
    public void ContainsWithToLower()
    {
        var filter = new Filter<Product>();

        filter.Contains(x => x.Name.ToLower(), "PRODUCT 2");

        Assert.Equal<int>([2], Ids(filter));
    }

    [Fact(
        DisplayName = "Does not contain still ignores case when the member is upper-cased"
    )]
    public void DoesNotContainWithToUpper()
    {
        var filter = new Filter<Product>();

        filter.DoesNotContain(x => x.Name.ToUpper(), "Product 2");

        Assert.DoesNotContain(2, Ids(filter));
        Assert.Contains(3, Ids(filter));
    }

    [Fact(
        DisplayName = "Starts with still ignores case when the member is upper-cased"
    )]
    public void StartsWithWithToUpper()
    {
        var filter = new Filter<Product>();

        filter.StartsWith(x => x.Name.ToUpper(), "product 2");

        Assert.Contains(2, Ids(filter));
        Assert.DoesNotContain(3, Ids(filter));
    }

    [Fact(
        DisplayName = "Ends with still ignores case when the member is upper-cased"
    )]
    public void EndsWithWithToUpper()
    {
        var filter = new Filter<Product>();

        filter.EndsWith(x => x.Name.ToUpper(), "Product 2");

        Assert.Contains(2, Ids(filter));
        Assert.DoesNotContain(3, Ids(filter));
    }
}
