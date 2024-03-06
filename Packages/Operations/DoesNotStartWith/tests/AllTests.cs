namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { DoesNotStartWith }, new[] { DoesNotStartWithEither } };

    static TestBuilder<Product> DoesNotStartWith =>
        new(
            "DoesNotStartWith",
            x =>
                x.OtherSearchField != null
                && !x.OtherSearchField
                    .Trim()
                    .ToLower()
                    .StartsWith("word1"),
            x => x.DoesNotStartWith<Product, string?>(x => x.OtherSearchField, "Word1"),
            x => x.DoesNotStartWith(x => x.OtherSearchField, "Word1"),
            x => x.DoesNotStartWith(x => x.OtherSearchField, "Word1"),
            x => x.DoesNotStartWith(x => x.OtherSearchField, "Word1")
        );

    static TestBuilder<Product> DoesNotStartWithEither =>
        new(
            "DoesNotStartWithEither",
            x =>
                x.OtherSearchField != null
                && !(
                    x.OtherSearchField
                        .Trim()
                        .ToLower()
                        .StartsWith("word1")
                    && x.OtherSearchField
                        .Trim()
                        .ToLower()
                        .StartsWith("1word")
                ),
            x =>
                x.DoesNotStartWith<Product, string?>(
                    x => x.OtherSearchField,
                    [ "Word1", "1Word" ]
                ),
            x =>
                x.DoesNotStartWith(x => x.OtherSearchField, [ "Word1", "1Word" ]),
            x =>
                x.DoesNotStartWith(x => x.OtherSearchField, [ "Word1", "1Word" ]),
            x => x.DoesNotStartWith(x => x.OtherSearchField, [ "Word1", "1Word" ])
        );
}
