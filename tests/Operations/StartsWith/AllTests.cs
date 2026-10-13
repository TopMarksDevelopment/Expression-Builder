namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [StartsWith],
                [StartsWithEither],
            ]
        );

    static TestBuilder<Product> StartsWith =>
        new(
            "StartsWith",
            x =>
                x.OtherSearchField != null
                && x.OtherSearchField.Trim().ToLower().StartsWith("word1"),
            x =>
                x.StartsWith<Product, string?>(
                    x => x.OtherSearchField,
                    "Word1"
                ),
            x => x.StartsWith(x => x.OtherSearchField, "Word1"),
            x => x.StartsWith(x => x.OtherSearchField, "Word1"),
            x => x.StartsWith(x => x.OtherSearchField, "Word1")
        );

    static TestBuilder<Product> StartsWithEither =>
        new(
            "StartsWithEither",
            x =>
                x.OtherSearchField != null
                && (
                    x.OtherSearchField.Trim().ToLower().StartsWith("word1")
                    || x.OtherSearchField.Trim().ToLower().StartsWith("1word")
                ),
            x =>
                x.StartsWith<Product, string?>(
                    x => x.OtherSearchField,
                    ["Word1", "1Word"]
                ),
            x => x.StartsWith(x => x.OtherSearchField, ["Word1", "1Word"]),
            x => x.StartsWith(x => x.OtherSearchField, ["Word1", "1Word"]),
            x => x.StartsWith(x => x.OtherSearchField, ["Word1", "1Word"])
        );
}
