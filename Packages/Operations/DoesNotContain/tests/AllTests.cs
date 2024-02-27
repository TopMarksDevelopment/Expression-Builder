namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]>
        {
            new[] { DoesNotContain },
            new[] { DoesNotContainEither }
        };

    static TestBuilder<Product> DoesNotContain =>
        new(
            "DoesNotContain",
            x =>
                x.OtherSearchField != null
                && !x.OtherSearchField.Trim().ToLower().Contains("word1"),
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
                x.OtherSearchField != null
                && !(
                    x.OtherSearchField.Trim().ToLower().Contains("word1")
                    && x.OtherSearchField.Trim().ToLower().Contains("1word")
                ),
            x =>
                x.DoesNotContain<Product, string?>(
                    x => x.OtherSearchField,
                    new[] { "Word1", "1Word" }
                ),
            x =>
                x.DoesNotContain(
                    x => x.OtherSearchField,
                    new[] { "Word1", "1Word" }
                ),
            x =>
                x.DoesNotContain(
                    x => x.OtherSearchField,
                    new[] { "Word1", "1Word" }
                ),
            x =>
                x.DoesNotContain(
                    x => x.OtherSearchField,
                    new[] { "Word1", "1Word" }
                )
        );
}
