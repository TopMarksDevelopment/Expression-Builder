namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [IsNullOrWhiteSpaceOnNullable]
            ]
        );

    static TestBuilder<Product> IsNullOrWhiteSpaceOnNullable =>
        new(
            "Nullable Check on nullable",
            x => x.Name == null || x.Name.Trim().ToLower() == "",
            x => x.IsNullOrWhiteSpace("Name"),
            x => x.IsNullOrWhiteSpace(x => x.Name),
            x => x.IsNullOrWhiteSpace(x => x.Name),
            x => x.IsNullOrWhiteSpace(x => x.Name)
        );
}
