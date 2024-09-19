namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.GreaterThanOrEqualTest;

public class OperationTests
{
    GreaterThanOrEqual? _new;
    GreaterThanOrEqual Operation => _new ??= new GreaterThanOrEqual();

    [Theory(DisplayName = "Having no values throws")]
    [InlineData(null)]
    [InlineData(new int[] { })]
    public void CheckFactoryStrings(int[]? values) =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                Operation.Validate(
                    new FilterStatement<int>("", Operation, values)
                )
        );
}
