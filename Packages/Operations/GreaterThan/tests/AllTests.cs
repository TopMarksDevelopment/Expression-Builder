namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { GreaterThanX } };

    static TestBuilder<Product> GreaterThanX =>
        new(
            "GreaterThan",
            x => x.Id > 1,
            x => x.GreaterThan<Product, int>(x => x.Id, 1),
            x => x.GreaterThan(x => x.Id, 1),
            x => x.GreaterThan(x => x.Id, 1),
            x => x.GreaterThan(x => x.Id, 1)
        );
}
