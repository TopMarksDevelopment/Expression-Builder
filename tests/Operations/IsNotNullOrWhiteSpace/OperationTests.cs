namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.IsNotNullOrWhiteSpaceTest;

public class OperationTests
{
    [Fact(DisplayName = "Having values throws")]
    public void CheckFactoryStrings() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new IsNotNullOrWhiteSpace().Validate(
                    new FilterStatement<string>(
                        "",
                        new IsNotNullOrWhiteSpace(),
                        [""]
                    )
                )
        );
}
