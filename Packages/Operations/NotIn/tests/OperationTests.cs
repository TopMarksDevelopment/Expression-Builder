namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.NotInTest;

public class OperationTests
{
    NotIn? _new;
    NotIn Operation => _new ??= new();

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
