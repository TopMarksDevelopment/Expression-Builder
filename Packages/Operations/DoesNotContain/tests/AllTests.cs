namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [DoesNotContain],
                [DoesNotContainEither],
                [DoesNotContainEitherAny],
                [DoesNotContainEitherAll],
            ]
        );

    static TestBuilder<Product> DoesNotContain =>
        new(
            "DoesNotContain",
            x =>
                x.OtherSearchField == null
                || !x.OtherSearchField.Trim().ToLower().Contains("word1"),
            x =>
                x.DoesNotContain<Product, string?>(
                    x => x.OtherSearchField,
                    "Word1"
                ),
            x => x.DoesNotContain(x => x.OtherSearchField, "Word1"),
            x => x.DoesNotContain(x => x.OtherSearchField, "Word1"),
            x => x.DoesNotContain(x => x.OtherSearchField, "Word1")
        );

    static TestBuilder<Product> DoesNotContainEither =>
        new(
            "DoesNotContainEither",
            x =>
                x.OtherSearchField == null
                || (
                    !x.OtherSearchField.Trim().ToLower().Contains("word1")
                    && !x.OtherSearchField.Trim().ToLower().Contains("1word")
                ),
            x =>
                x.DoesNotContain<Product, string?>(
                    x => x.OtherSearchField,
                    wordArray
                ),
            x => x.DoesNotContain(x => x.OtherSearchField, wordArray),
            x => x.DoesNotContain(x => x.OtherSearchField, wordArray),
            x => x.DoesNotContain(x => x.OtherSearchField, wordArray)
        );

    static TestBuilder<Product> DoesNotContainEitherAny =>
        new(
            "DoesNotContainEither - Specify: Matches.Any ",
            x =>
                x.OtherSearchField == null
                || (
                    !x.OtherSearchField.Trim().ToLower().Contains("word1")
                    || !x.OtherSearchField.Trim().ToLower().Contains("1word")
                ),
            x =>
                x.DoesNotContain<Product, string?>(
                    x => x.OtherSearchField,
                    wordArray,
                    new FilterStatementOptions { Match = Matches.Any }
                ),
            x =>
                x.DoesNotContain(
                    x => x.OtherSearchField,
                    wordArray,
                    new FilterStatementOptions { Match = Matches.Any }
                ),
            x =>
                x.DoesNotContain(
                    x => x.OtherSearchField,
                    wordArray,
                    new FilterStatementOptions { Match = Matches.Any }
                ),
            x =>
                x.DoesNotContain(
                    x => x.OtherSearchField,
                    wordArray,
                    new FilterStatementOptions { Match = Matches.Any }
                )
        );

    static TestBuilder<Product> DoesNotContainEitherAll =>
        new(
            "DoesNotContainEither - Specify: Matches.All ",
            x =>
                x.OtherSearchField == null
                || (
                    !x.OtherSearchField.Trim().ToLower().Contains("word1")
                    && !x.OtherSearchField.Trim().ToLower().Contains("1word")
                ),
            x =>
                x.DoesNotContain<Product, string?>(
                    x => x.OtherSearchField,
                    wordArray,
                    new FilterStatementOptions { Match = Matches.All }
                ),
            x =>
                x.DoesNotContain(
                    x => x.OtherSearchField,
                    wordArray,
                    new FilterStatementOptions { Match = Matches.All }
                ),
            x =>
                x.DoesNotContain(
                    x => x.OtherSearchField,
                    wordArray,
                    new FilterStatementOptions { Match = Matches.All }
                ),
            x =>
                x.DoesNotContain(
                    x => x.OtherSearchField,
                    wordArray,
                    new FilterStatementOptions { Match = Matches.All }
                )
        );

    static readonly string[] wordArray = ["Word1", "1Word"];
}
