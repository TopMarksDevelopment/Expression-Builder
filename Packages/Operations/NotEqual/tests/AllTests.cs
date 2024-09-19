namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [NotEqualsCheck],
                [NotEqualsCallsIn],
            ]
        );

    static TestBuilder<Product> NotEqualsCheck =>
        new(
            "NotEquals check",
            x => x.Id != 1,
            x => x.NotEqual<Product, int>(x => x.Id, 1),
            x => x.NotEqual(x => x.Id, 1),
            x => x.NotEqual(x => x.Id, 1),
            x => x.NotEqual(x => x.Id, 1)
        );

    static TestBuilder<Product> NotEqualsCallsIn =>
        new(
            "NotEquals with [] should call In",
            x => x.Category != null && !new[] { 2, 4 }.Contains(x.Category.Id),
            x => x.NotEqual<Product, int>(x => x.Category!.Id, [2, 4]),
            x => x.NotEqual(x => x.Category!.Id, [2, 4]),
            x => x.NotEqual(x => x.Category!.Id, [2, 4]),
            x => x.NotEqual(x => x.Category!.Id, [2, 4])
        );
}
