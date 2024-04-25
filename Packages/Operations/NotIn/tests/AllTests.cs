namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    public AllTests() =>
        AddRows(
            [
                [MultiMatcher],
                [MultiNullMatcher]
            ]
        );

    internal static string ApplyReplacements(string input) =>
        input
            .Replace("AllTests.values.", "[1, 2].")
            .Replace("AllTests.nullValues.", "[1, 2].");

    static TestBuilder<Product> MultiMatcher =>
        new(
            "Matches multiple values",
            x => !values.Contains(x.Id),
            x => x.NotIn("Id", values),
            x => x.NotIn(x => x.Id, values),
            x => x.NotIn(x => x.Id, values),
            x => x.NotIn(x => x.Id, values)
        );

    static TestBuilder<Product> MultiNullMatcher =>
        new(
            "Matches multiple values",
            x => x.BrandId == null || !nullValues.Contains(x.BrandId.Value),
            x => x.NotIn("BrandId", nullValues),
            x => x.NotIn(x => x.BrandId, nullValues),
            x => x.NotIn(x => x.BrandId, nullValues),
            x => x.NotIn(x => x.BrandId, nullValues)
        );

    static readonly int[] values = [1, 2];
    static readonly int?[] nullValues = [1, 2];
}
