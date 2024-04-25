namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [LessThanOrEqualX]
            ]
        );

    static TestBuilder<Product> LessThanOrEqualX =>
        new(
            "LessThanOrEqual",
            x => x.Id <= 3,
            x => x.LessThanOrEqual<Product, int>(x => x.Id, 3),
            x => x.LessThanOrEqual(x => x.Id, 3),
            x => x.LessThanOrEqual(x => x.Id, 3),
            x => x.LessThanOrEqual(x => x.Id, 3)
        );
}
