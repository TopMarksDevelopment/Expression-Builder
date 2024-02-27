namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.ContainsTest;

public class OperationTests
{
    [Fact(DisplayName = "No values throws")]
    public void NoValuesThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new Operations.Contains().Validate(
                    new FilterStatement<string>()
                )
        );
}
