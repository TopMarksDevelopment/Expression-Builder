namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [EqualsCallsIn]
            ]
        );

    static TestBuilder<Product> EqualsCallsIn =>
        new(
            "Equals with [] should call In",
            x => x.Category != null && new[] { 2, 4 }.Contains(x.Category.Id),
            x => x.Equal<Product, int>(x => x.Category!.Id, [2, 4]),
            x => x.Equal(x => x.Category!.Id, [2, 4]),
            x => x.Equal(x => x.Category!.Id, [2, 4]),
            x => x.Equal(x => x.Category!.Id, [2, 4])
        );
}
