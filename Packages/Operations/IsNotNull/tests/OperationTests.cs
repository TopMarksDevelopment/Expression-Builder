namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.IsNotNullTest;

public class OperationTests
{
    [Fact(DisplayName = "Having values throws")]
    public void CheckFactoryStrings() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new IsNotNull().Validate(
                    new FilterStatement<string>(
                        "",
                        new IsNotNull(),
                        [""]
                    )
                )
        );
}
