namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [IsNotNullOnNullable]
            ]
        );

    static TestBuilder<Product> IsNotNullOnNullable =>
        new(
            "Nullable Check on nullable",
            x => x.CreatedAt != null,
            x => x.IsNotNull("CreatedAt"),
            x => x.IsNotNull(x => x.CreatedAt),
            x => x.IsNotNull(x => x.CreatedAt),
            x => x.IsNotNull(x => x.CreatedAt)
        );
}
