namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { NotBetweenXY } };

    static TestBuilder<Product> NotBetweenXY =>
        new(
            "NotBetween",
            x => !(x.Id >= 1 && x.Id <= 2),
            x => x.NotBetween<Product, int>(x => x.Id, 1, 2),
            x => x.NotBetween(x => x.Id, 1, 2),
            x => x.NotBetween(x => x.Id, 1, 2),
            x => x.NotBetween(x => x.Id, 1, 2)
        );
}
