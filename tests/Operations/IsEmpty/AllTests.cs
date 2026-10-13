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
            x => x.OtherSearchField != null && x.OtherSearchField == "",
            x => x.IsEmpty("OtherSearchField"),
            x => x.IsEmpty(x => x.OtherSearchField),
            x => x.IsEmpty(x => x.OtherSearchField),
            x => x.IsEmpty(x => x.OtherSearchField)
        );
}
