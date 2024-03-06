namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { EndsWith }, new[] { EndsWithEither } };

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
                    new[] { "Word3", "3Word" }
                ),
            x =>
                x.EndsWith(x => x.OtherSearchField, new[] { "Word3", "3Word" }),
            x =>
                x.EndsWith(x => x.OtherSearchField, new[] { "Word3", "3Word" }),
            x => x.EndsWith(x => x.OtherSearchField, new[] { "Word3", "3Word" })
        );
}
