namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { Contains }, new[] { ContainsEither } };

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
                    new[] { "Word1", "1Word" }
                ),
            x =>
                x.Contains(x => x.OtherSearchField, new[] { "Word1", "1Word" }),
            x =>
                x.Contains(x => x.OtherSearchField, new[] { "Word1", "1Word" }),
            x => x.Contains(x => x.OtherSearchField, new[] { "Word1", "1Word" })
        );
}
