namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { NotBetweenExclusiveXY } };

    static TestBuilder<Product> NotBetweenExclusiveXY =>
        new(
            "NotBetweenExclusive",
            x => !(x.Id > 1 && x.Id < 3),
            x => x.NotBetweenExclusive<Product, int>(x => x.Id, 1, 3),
            x => x.NotBetweenExclusive(x => x.Id, 1, 3),
            x => x.NotBetweenExclusive(x => x.Id, 1, 3),
            x => x.NotBetweenExclusive(x => x.Id, 1, 3)
        );
}
