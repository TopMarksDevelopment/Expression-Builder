namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [GreaterThanX]
            ]
        );

    static TestBuilder<Product> GreaterThanX =>
        new(
            "GreaterThan",
            x => x.Id > 1,
            x => x.GreaterThan<Product, int>(x => x.Id, 1),
            x => x.GreaterThan(x => x.Id, 1),
            x => x.GreaterThan(x => x.Id, 1),
            x => x.GreaterThan(x => x.Id, 1)
        );
}
