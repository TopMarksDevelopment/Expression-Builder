namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.LessThanOrEqualTest;

public class OperationTests
{
    LessThanOrEqual? _new;
    LessThanOrEqual Operation => _new ??= new LessThanOrEqual();

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
