namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { GreaterThanOrEqualX } };

    static TestBuilder<Product> GreaterThanOrEqualX =>
        new(
            "GreaterThanOrEqual",
            x => x.Id >= 2,
            x => x.GreaterThanOrEqual<Product, int>(x => x.Id, 2),
            x => x.GreaterThanOrEqual(x => x.Id, 2),
            x => x.GreaterThanOrEqual(x => x.Id, 2),
            x => x.GreaterThanOrEqual(x => x.Id, 2)
        );
}
