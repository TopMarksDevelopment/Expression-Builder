namespace ExpressionBuilder.Tests;

[Collection("Ensure old fixes are not undone")]
public class FixTests : EfCoreTestBase
{
    [Fact(DisplayName = "Fixed: Nullable trim error")]
    public void CollectionTestNoLongerThrows()
    {
        var filter = new Filter<Category>();
        int? nullId = 1;

        filter
            .OpenCollection(x => x.Products)
            .Equal(x => x.Id, nullId.Value)
            .CloseCollection();

        var exception = Record.Exception(() => GetCategory().Where(filter));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "Fixed: Collection chaining closes historic filter")]
    public void CollectionChainingRetainsCorrectType()
    {
        var filter = new Filter<Category>();

        var group = filter.OpenCollection(x => x.Products);

        // We open and close a collection,
        //  so group should still be at Products
        group
            .OpenCollection(x => x.Categories)
            .Equal(x => x.Id, 1)
            .CloseCollection();

        // As we close, this should work
        group.Equal(x => x.OtherSearchField, "word1");

        Assert.Null(Record.Exception(() => GetCategory().Where(filter)));
    }
}
