namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [GreaterThanOrEqualX]
            ]
        );

    static TestBuilder<Product> GreaterThanOrEqualX =>
        new(
            "GreaterThanOrEqual",
            x => x.Id >= 2,
            x => x.GreaterThanOrEqual<Product, int>(x => x.Id, 2),
            x => x.GreaterThanOrEqual(x => x.Id, 2),
            x => x.GreaterThanOrEqual(x => x.Id, 2),
            x => x.GreaterThanOrEqual(x => x.Id, 2)
        );
}
