namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [DoesNotStartWith],
                [DoesNotStartWithEither]
            ]
        );

    static TestBuilder<Product> DoesNotStartWith =>
        new(
            "DoesNotStartWith",
            x =>
                x.OtherSearchField == null
                || !x.OtherSearchField.Trim().ToLower().StartsWith("word1"),
            x =>
                x.DoesNotStartWith<Product, string?>(
                    x => x.OtherSearchField,
                    "Word1"
                ),
            x => x.DoesNotStartWith(x => x.OtherSearchField, "Word1"),
            x => x.DoesNotStartWith(x => x.OtherSearchField, "Word1"),
            x => x.DoesNotStartWith(x => x.OtherSearchField, "Word1")
        );

    static TestBuilder<Product> DoesNotStartWithEither =>
        new(
            "DoesNotStartWithEither",
            x =>
                x.OtherSearchField == null
                || (
                    !x.OtherSearchField.Trim().ToLower().StartsWith("word1")
                    && !x.OtherSearchField.Trim().ToLower().StartsWith("1word")
                ),
            x =>
                x.DoesNotStartWith<Product, string?>(
                    x => x.OtherSearchField,
                    ["Word1", "1Word"]
                ),
            x =>
                x.DoesNotStartWith(x => x.OtherSearchField, ["Word1", "1Word"]),
            x =>
                x.DoesNotStartWith(x => x.OtherSearchField, ["Word1", "1Word"]),
            x => x.DoesNotStartWith(x => x.OtherSearchField, ["Word1", "1Word"])
        );
}
