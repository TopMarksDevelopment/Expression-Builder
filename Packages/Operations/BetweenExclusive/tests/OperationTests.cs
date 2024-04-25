namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.BetweenExclusiveTest;

public class OperationTests
{
    [Theory(DisplayName = "Having no values throws")]
    [InlineData(null)]
    [InlineData(new int[] {})]
    [InlineData(new int[] { 1 })]
    [InlineData(new int[] { 1, 2, 3 })]
    public void CheckFactoryStrings(int[]? values) =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new Operations.BetweenExclusive().Validate(
                    new FilterStatement<int>(
                        "",
                        new BetweenExclusive(),
                        values
                    )
                )
        );
}
