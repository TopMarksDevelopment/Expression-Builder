using ExpressionBuilder.Tests.Models;

namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.BetweenTest;

public class OperationTests
{
    [Theory(DisplayName = "Anything other than 2 values throws")]
    [InlineData(null)]
    [InlineData(new int[] { })]
    [InlineData(new int[] { 1 })]
    [InlineData(new int[] { 1, 2, 3 })]
    public void CheckValidationThrows(int[]? values) =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new Between().Validate(
                    new FilterStatement<int>("", new Between(), values)
                )
        );

    [Fact(DisplayName = "2 values is fine")]
    public void CheckValidation()
    {
        var exception = Record.Exception(() =>
        {
            var filter = new Filter<Product>();

            filter.Between(
                x => x.CreatedAt!.Value,
                DateTime.Now.AddDays(-1),
                DateTime.Now
            );
        });

        Assert.Null(exception);
    }

    [Fact(DisplayName = "2 identical values are fine")]
    public void CheckIdentical()
    {
        var exception = Record.Exception(() =>
        {
            var filter = new Filter<Product>();

            filter.Between(x => x.Id, 1, 1);
        });

        Assert.Null(exception);
    }
}
