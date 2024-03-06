namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { StartsWith }, new[] { StartsWithEither } };

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
                    new[] { "Word1", "1Word" }
                ),
            x =>
                x.StartsWith(
                    x => x.OtherSearchField,
                    new[] { "Word1", "1Word" }
                ),
            x =>
                x.StartsWith(
                    x => x.OtherSearchField,
                    new[] { "Word1", "1Word" }
                ),
            x =>
                x.StartsWith(
                    x => x.OtherSearchField,
                    new[] { "Word1", "1Word" }
                )
        );
}
