namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { IsNullOnNullable } };

    static TestBuilder<Product> IsNullOnNullable =>
        new(
            "Nullable Check on nullable",
            x => x.CreatedAt == null,
            x => x.IsNull("CreatedAt"),
            x => x.IsNull(x => x.CreatedAt),
            x => x.IsNull(x => x.CreatedAt),
            x => x.IsNull(x => x.CreatedAt)
        );
}
