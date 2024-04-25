namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.EndsWithTest;

public class OperationTests
{
    [Fact(DisplayName = "No values throws")]
    public void NoValuesThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new EndsWith().Validate(new FilterStatement<string>())
        );

    [Fact(DisplayName = "Matches.All with more than 1 throws")]
    public void AllWithManyValuesThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new EndsWith().Validate(
                    new FilterStatement<string>(
                        "",
                        new EndsWith(),
                        ["One", "Two"],
                        new FilterStatementOptions { Match = Matches.All }
                    )
                )
        );
}
