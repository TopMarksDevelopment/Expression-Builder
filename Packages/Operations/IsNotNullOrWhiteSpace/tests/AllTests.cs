namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [IsNotNullOrWhiteSpaceOnNullable]
            ]
        );

    static TestBuilder<Product> IsNotNullOrWhiteSpaceOnNullable =>
        new(
            "Nullable Check on nullable",
            x => x.Name != null && x.Name.Trim().ToLower() != "",
            x => x.IsNotNullOrWhiteSpace("Name"),
            x => x.IsNotNullOrWhiteSpace(x => x.Name),
            x => x.IsNotNullOrWhiteSpace(x => x.Name),
            x => x.IsNotNullOrWhiteSpace(x => x.Name)
        );
}
