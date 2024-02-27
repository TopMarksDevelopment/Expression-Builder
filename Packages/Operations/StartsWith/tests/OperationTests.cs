namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.StartsWithTest;

public class OperationTests
{
    [Fact(DisplayName = "No values throws")]
    public void NoValuesThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new StartsWith().Validate(
                    new FilterStatement<string>()
                )
        );

    [Fact(DisplayName = "Matches.All with more than 1 vaue throws")]
    public void AllWithManyValuesThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new StartsWith()
                {
                    Match = Api.Matches.All
                }.Validate(
                    new FilterStatement<string>(
                        "",
                        new StartsWith(),
                        [ "One", "Two" ]
                    )
                )
        );
}
