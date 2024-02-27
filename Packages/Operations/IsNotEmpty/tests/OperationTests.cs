namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.IsNotEmptyTest;

public class OperationTests
{
    IsNotEmpty? _new;
    IsNotEmpty Operation => _new ??= new();

    [Theory(DisplayName = "Having values throws")]
    [InlineData(new int[] {1})]
    [InlineData(new int[] {1, 2})]
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
