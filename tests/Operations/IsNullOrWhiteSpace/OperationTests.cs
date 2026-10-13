namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.IsNullOrWhiteSpaceTest;

public class OperationTests
{
    [Fact(DisplayName = "Having values throws")]
    public void CheckFactoryStrings() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new IsNullOrWhiteSpace().Validate(
                    new FilterStatement<string>(
                        "",
                        new IsNullOrWhiteSpace(),
                        [""]
                    )
                )
        );
}
