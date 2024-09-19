namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [NotBetweenXY],
                [NullCheck],
            ]
        );

    static TestBuilder<Product> NotBetweenXY =>
        new(
            "NotBetween",
            x => !(x.Id >= 1 && x.Id <= 2),
            x => x.NotBetween<Product, int>(x => x.Id, 1, 2),
            x => x.NotBetween(x => x.Id, 1, 2),
            x => x.NotBetween(x => x.Id, 1, 2),
            x => x.NotBetween(x => x.Id, 1, 2)
        );

    static TestBuilder<Product> NullCheck =>
        new(
            "NotBetween on nullable",
            x =>
                x.BrandId == null
                || !(x.BrandId.Value >= 1 && x.BrandId.Value <= 2),
            x => x.NotBetween<Product, int?>(x => x.BrandId, 1, 2),
            x => x.NotBetween(x => x.BrandId, 1, 2),
            x => x.NotBetween(x => x.BrandId, 1, 2),
            x => x.NotBetween(x => x.BrandId, 1, 2)
        );
}
