namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{

    internal static string ApplyReplacements(string input) =>
        input
            .Replace("AllTests.values.", "[1, 2].");

    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { MultiMatcher } };

    static TestBuilder<Product> MultiMatcher =>
        new(
            "Matches multiple values",
            x => values.Contains(x.Id),
            x => x.In("Id", values),
            x => x.In(x => x.Id, values),
            x => x.In(x => x.Id, values),
            x => x.In(x => x.Id, values)
        );

    static readonly int[] values = [1, 2];
}
