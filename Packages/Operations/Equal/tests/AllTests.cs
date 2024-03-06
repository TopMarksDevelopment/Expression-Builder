namespace ExpressionBuilder.Tests;

using System.Collections;
using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllOperations() =>
        new List<object[]> { new[] { EqualsCallsIn } };

    static TestBuilder<Product> EqualsCallsIn =>
        new(
            "Equals with [] should call In",
            x => x.Category != null && new []{ 2, 4 }.Contains(x.Category.Id),
            x => x.Equal<Product, int>(x => x.Category!.Id, [2, 4]),
            x => x.Equal(x => x.Category!.Id, [2, 4]),
            x => x.Equal(x => x.Category!.Id, [2, 4]),
            x => x.Equal(x => x.Category!.Id, [2, 4])
        );
}
