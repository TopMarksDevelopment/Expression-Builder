namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [SmartSearchOneCharacter],
            ]
        );

    static TestBuilder<Product> SmartSearchOneCharacter =>
        new(
            "Matches \"Word1\" exactly (not \"Word123\")",
            x =>
                x.OtherSearchField != null
                && (
                    (
                        (
                            (
                                " " + x.OtherSearchField.Trim().ToLower() + " "
                            ).Contains(" Word1 ")
                        ) == true
                    )
                ),
            x =>
                x.SmartSearch<Product, string?>(
                    x => x.OtherSearchField,
                    ["\"Word1\""]
                ),
            x => x.SmartSearch(x => x.OtherSearchField, ["\"Word1\""]),
            x => x.SmartSearch(x => x.OtherSearchField, ["\"Word1\""]),
            x => x.SmartSearch(x => x.OtherSearchField, ["\"Word1\""])
        );
}
