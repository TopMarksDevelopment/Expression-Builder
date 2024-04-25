namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [EndsWith],
                [EndsWithEither]
            ]
        );

    static TestBuilder<Product> EndsWith =>
        new(
            "EndsWith",
            x =>
                x.OtherSearchField != null
                && x.OtherSearchField.Trim().ToLower().EndsWith("word3"),
            x => x.EndsWith<Product, string?>(x => x.OtherSearchField, "Word3"),
            x => x.EndsWith(x => x.OtherSearchField, "Word3"),
            x => x.EndsWith(x => x.OtherSearchField, "Word3"),
            x => x.EndsWith(x => x.OtherSearchField, "Word3")
        );

    static TestBuilder<Product> EndsWithEither =>
        new(
            "EndsWithEither",
            x =>
                x.OtherSearchField != null
                && (
                    x.OtherSearchField.Trim().ToLower().EndsWith("word3")
                    || x.OtherSearchField.Trim().ToLower().EndsWith("3word")
                ),
            x =>
                x.EndsWith<Product, string?>(
                    x => x.OtherSearchField,
                    ["Word3", "3Word"]
                ),
            x => x.EndsWith(x => x.OtherSearchField, ["Word3", "3Word"]),
            x => x.EndsWith(x => x.OtherSearchField, ["Word3", "3Word"]),
            x => x.EndsWith(x => x.OtherSearchField, ["Word3", "3Word"])
        );
}
