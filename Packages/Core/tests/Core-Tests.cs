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
}
