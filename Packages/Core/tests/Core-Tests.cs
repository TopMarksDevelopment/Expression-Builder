namespace ExpressionBuilder.Tests;

[Collection("Checks for the core")]
public class CoreTests : EfCoreTestBase
{
    [Theory(DisplayName = "No filters works still")]
    [InlineData(typeof(Category))]
    [InlineData(typeof(Product))]
    public void MultiCollectionTest(Type testType)
    {
        var exception = Record.Exception(
            () => GetCollection(testType).Where(new Filter<IItemable>())
        );

        Assert.Null(exception);
    }

    [Fact(DisplayName = "Deeper collection chaining works")]
    public void CollectionChainDoubleCloseCheck()
    {
        var filter = new Filter<Product>();

        // Collection opened so filter is now at Products
        var group = filter.OpenCollection(x => x.Categories);

        // We open and sub-collection,
        var subgroup = group.OpenCollection(i => i.Products);

        // We open and close a collection,
        //  so subgroup should still be at Products
        subgroup
            .OpenCollection(i => i.StockLocations)
            .LessThanOrEqual(x => x.Quantity, 10)
            .CloseCollection<Product>();

        // Add statement to subgroup
        //  as we're in the right place it shouldn't error (later)
        subgroup.Equal(x => x.OtherSearchField, "word1");

        // Closes subgroup
        //  so group should still be at Category
        subgroup.CloseCollection<Category>();

        // Add statement to group
        //  as we're in the right place it shouldn't error (later)
        group.Equal(x => x.Name, "Category 1");

        // Collection closed
        //  filter should be reverted to Categories without errro
        filter.CloseCollection();

        // Add statement to filter
        //  as we're in the right place it shouldn't error (later)
        filter.Equal(x => x.Id, 1);

        Assert.Null(Record.Exception(() => GetProducts().Where(filter)));
    }
}
