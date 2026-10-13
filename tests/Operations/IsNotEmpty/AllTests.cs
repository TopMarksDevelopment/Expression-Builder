namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [BasicIsEmpty],
            ]
        );

    static TestBuilder<Product> BasicIsEmpty =>
        new(
            "Matches multiple values",
            x => x.OtherSearchField == null || x.OtherSearchField != "",
            x => x.IsNotEmpty("OtherSearchField"),
            x => x.IsNotEmpty(x => x.OtherSearchField),
            x => x.IsNotEmpty(x => x.OtherSearchField),
            x => x.IsNotEmpty(x => x.OtherSearchField)
        );
}
