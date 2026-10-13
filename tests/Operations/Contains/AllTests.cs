namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [Contains],
                [ContainsEither],
            ]
        );

    static TestBuilder<Product> Contains =>
        new(
            "Contains",
            x =>
                x.OtherSearchField != null
                && x.OtherSearchField.Trim().ToLower().Contains("word1"),
            x => x.Contains<Product, string?>(x => x.OtherSearchField, "Word1"),
            x => x.Contains(x => x.OtherSearchField, "Word1"),
            x => x.Contains(x => x.OtherSearchField, "Word1"),
            x => x.Contains(x => x.OtherSearchField, "Word1")
        );

    static TestBuilder<Product> ContainsEither =>
        new(
            "ContainsEither",
            x =>
                x.OtherSearchField != null
                && (
                    x.OtherSearchField.Trim().ToLower().Contains("word1")
                    || x.OtherSearchField.Trim().ToLower().Contains("1word")
                ),
            x =>
                x.Contains<Product, string?>(
                    x => x.OtherSearchField,
                    ["Word1", "1Word"]
                ),
            x => x.Contains(x => x.OtherSearchField, ["Word1", "1Word"]),
            x => x.Contains(x => x.OtherSearchField, ["Word1", "1Word"]),
            x => x.Contains(x => x.OtherSearchField, ["Word1", "1Word"])
        );
}
