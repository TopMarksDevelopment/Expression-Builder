namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [IsNullOnNullable],
            ]
        );

    static TestBuilder<Product> IsNullOnNullable =>
        new(
            "Nullable Check on nullable",
            x => x.CreatedAt == null,
            x => x.IsNull("CreatedAt"),
            x => x.IsNull(x => x.CreatedAt),
            x => x.IsNull(x => x.CreatedAt),
            x => x.IsNull(x => x.CreatedAt)
        );
}
