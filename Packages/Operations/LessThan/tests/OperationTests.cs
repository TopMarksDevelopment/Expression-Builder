namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.LessThanTest;

public class OperationTests
{
    LessThan? _new;
    LessThan Operation => _new ??= new LessThan();

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
