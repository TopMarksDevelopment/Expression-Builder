namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [BetweenXY],
                [NullCheck],
            ]
        );

    static TestBuilder<Product> BetweenXY =>
        new(
            "Between",
            x => x.Id >= 1 && x.Id <= 2,
            x => x.Between<Product, int>(x => x.Id, 1, 2),
            x => x.Between(x => x.Id, 1, 2),
            x => x.Between(x => x.Id, 1, 2),
            x => x.Between(x => x.Id, 1, 2)
        );

    static TestBuilder<Product> NullCheck =>
        new(
            "Between on nullable",
            x =>
                x.BrandId != null
                && (x.BrandId.Value >= 1 && x.BrandId.Value <= 2),
            x => x.Between<Product, int?>(x => x.BrandId, 1, 2),
            x => x.Between(x => x.BrandId, 1, 2),
            x => x.Between(x => x.BrandId, 1, 2),
            x => x.Between(x => x.BrandId, 1, 2)
        );
}
