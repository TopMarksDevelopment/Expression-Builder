namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.InTest;

public class OperationTests
{
    In? _new;
    In Operation => _new ??= new();

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
