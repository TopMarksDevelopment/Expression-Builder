namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.GreaterThanTest;

public class OperationTests
{
    GreaterThan? _new;
    GreaterThan Operation => _new ??= new GreaterThan();

    [Theory(DisplayName = "Having no values throws")]
    [InlineData(null)]
    [InlineData(new int[] {})]
    public void CheckFactoryStrings(int[]? values) =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                Operation.Validate(
                    new FilterStatement<int>(
                        "",
                        Operation,
                        values
                    )
                )
        );
}
