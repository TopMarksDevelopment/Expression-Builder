using ExpressionBuilder.Tests.Models;

namespace ExpressionBuilder.Tests.Base;

public class Tests
{
    [Theory(DisplayName = "Classes are IItemable")]
    [InlineData(typeof(Product))]
    [InlineData(typeof(StockLocation))]
    [InlineData(typeof(Category))]
    public void ImplementationTest(Type type) =>
        Assert.NotNull(type.GetInterface(nameof(IItemable)));
}
