namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.DoesNotContainTest;

public class OperationTests
{
    [Fact(DisplayName = "No values throws")]
    public void NoValuesThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DoesNotContain().Validate(new FilterStatement<string>())
        );
}
