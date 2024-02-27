namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.IsNullTest;

public class OperationTests
{
    [Fact(DisplayName = "Having values throws")]
    public void CheckFactoryStrings() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new IsNull().Validate(
                    new FilterStatement<string>(
                        "",
                        new IsNull(),
                        [""]
                    )
                )
        );
}
