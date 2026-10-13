namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.DoesNotStartWithTest;

public class OperationTests
{
    [Fact(DisplayName = "No values throws")]
    public void NoValuesThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new Operations.DoesNotStartWith().Validate(
                    new FilterStatement<string>()
                )
        );
}
