namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { BetweenExclusiveXY } };

    static TestBuilder<Product> BetweenExclusiveXY =>
        new(
            "BetweenExclusive",
            x => x.Id > 1 && x.Id < 3,
            x => x.BetweenExclusive<Product, int>(x => x.Id, 1, 3),
            x => x.BetweenExclusive(x => x.Id, 1, 3),
            x => x.BetweenExclusive(x => x.Id, 1, 3),
            x => x.BetweenExclusive(x => x.Id, 1, 3)
        );
}
