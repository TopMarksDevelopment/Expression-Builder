namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.IsEmptyTest;

public class OperationTests
{
    IsEmpty? _new;
    IsEmpty Operation => _new ??= new();

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
