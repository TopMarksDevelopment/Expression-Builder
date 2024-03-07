namespace ExpressionBuilder.Tests;

using System.Linq;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;
using Xunit;

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
}
