using ExpressionBuilder.Tests;
using ExpressionBuilder.Tests.Models;

public class Tests
{
    [Theory(DisplayName = "Classes are IItemable")]
    [InlineData(typeof(Product))]
    [InlineData(typeof(StockLocation))]
    [InlineData(typeof(Category))]
    public void ImplementationTest(Type type) =>
        Assert.True(type.GetInterface(nameof(IItemable)) != null);
}
