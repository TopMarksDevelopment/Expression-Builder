namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [LessThanX],
            ]
        );

    static TestBuilder<Product> LessThanX =>
        new(
            "LessThan",
            x => x.Id < 3,
            x => x.LessThan<Product, int>(x => x.Id, 3),
            x => x.LessThan(x => x.Id, 3),
            x => x.LessThan(x => x.Id, 3),
            x => x.LessThan(x => x.Id, 3)
        );
}
