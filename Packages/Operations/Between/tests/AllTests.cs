namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { BetweenXY } };

    static TestBuilder<Product> BetweenXY =>
        new(
            "Between",
            x => x.Id >= 1 && x.Id <= 2,
            x => x.Between<Product, int>(x => x.Id, 1, 2),
            x => x.Between(x => x.Id, 1, 2),
            x => x.Between(x => x.Id, 1, 2),
            x => x.Between(x => x.Id, 1, 2)
        );
}
