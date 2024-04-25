namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [BetweenExclusiveXY],
                [NullCheck]
            ]
        );

    static TestBuilder<Product> BetweenExclusiveXY =>
        new(
            "BetweenExclusive",
            x => x.Id > 1 && x.Id < 3,
            x => x.BetweenExclusive<Product, int>(x => x.Id, 1, 3),
            x => x.BetweenExclusive(x => x.Id, 1, 3),
            x => x.BetweenExclusive(x => x.Id, 1, 3),
            x => x.BetweenExclusive(x => x.Id, 1, 3)
        );

    static TestBuilder<Product> NullCheck =>
        new(
            "BetweenExclusive - nullable",
            x => x.BrandId != null && (x.BrandId.Value > 1 && x.BrandId.Value < 2),
            x => x.BetweenExclusive<Product, int?>(x => x.BrandId, 1, 2),
            x => x.BetweenExclusive(x => x.BrandId, 1, 2),
            x => x.BetweenExclusive(x => x.BrandId, 1, 2),
            x => x.BetweenExclusive(x => x.BrandId, 1, 2)
        );
}
