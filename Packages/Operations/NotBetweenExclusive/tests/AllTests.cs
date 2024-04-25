namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [NotBetweenExclusiveXY],
                [NullCheck]
            ]
        );

    static TestBuilder<Product> NotBetweenExclusiveXY =>
        new(
            "NotBetweenExclusive",
            x => !(x.Id > 1 && x.Id < 3),
            x => x.NotBetweenExclusive<Product, int>(x => x.Id, 1, 3),
            x => x.NotBetweenExclusive(x => x.Id, 1, 3),
            x => x.NotBetweenExclusive(x => x.Id, 1, 3),
            x => x.NotBetweenExclusive(x => x.Id, 1, 3)
        );

    static TestBuilder<Product> NullCheck =>
        new(
            "NotBetweenExclusive on nullable",
            x => x.BrandId == null || !(x.BrandId.Value > 1 && x.BrandId.Value < 2),
            x => x.NotBetweenExclusive<Product, int?>(x => x.BrandId, 1, 2),
            x => x.NotBetweenExclusive(x => x.BrandId, 1, 2),
            x => x.NotBetweenExclusive(x => x.BrandId, 1, 2),
            x => x.NotBetweenExclusive(x => x.BrandId, 1, 2)
        );
}
