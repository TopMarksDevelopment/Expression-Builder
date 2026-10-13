namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.DoesNotEndWithTest;

public class OperationTests
{
    [Fact(DisplayName = "No values throws")]
    public void CheckFactoryStrings() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new Operations.DoesNotEndWith().Validate(
                    new FilterStatement<string>()
                )
        );
}
