namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { 
            new[] { NotEndWith },
            new[] { NotEndWithEither }
        };

    static TestBuilder<Product> NotEndWith =>
        new(
            "NotEndWith",
            x =>
                x.OtherSearchField != null
                && !(
                    x.OtherSearchField
                        .Trim()
                        .ToLower()
                        .EndsWith("word")
                ),
            x =>
                x.DoesNotEndWith<Product, string?>(
                    x => x.OtherSearchField,
                    "Word"
                ),
            x => x.DoesNotEndWith(x => x.OtherSearchField, "Word"),
            x => x.DoesNotEndWith(x => x.OtherSearchField, "Word"),
            x => x.DoesNotEndWith(x => x.OtherSearchField, "Word")
        );

    static TestBuilder<Product> NotEndWithEither =>
        new(
            "NotEndWithEither",
            x =>
                x.Category != null
                && !(
                    x.Category.Id.ToString().Trim().ToLower().EndsWith("2")
                    || x.Category.Id.ToString().Trim().ToLower().EndsWith("4")
                ),
            x =>
                x.DoesNotEndWith<Product, int>(
                    x => x.Category!.Id,
                    [2, 4]
                ),
            x => x.DoesNotEndWith(x => x.Category!.Id, [2, 4]),
            x => x.DoesNotEndWith(x => x.Category!.Id, [2, 4]),
            x => x.DoesNotEndWith(x => x.Category!.Id, [2, 4])
        );
}
