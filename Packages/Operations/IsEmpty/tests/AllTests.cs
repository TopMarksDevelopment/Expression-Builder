namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { BasicIsEmpty } };

    static TestBuilder<Product> BasicIsEmpty =>
        new(
            "Matches multiple values",
            x => x.OtherSearchField != null && x.OtherSearchField == "",
            x => x.IsEmpty("OtherSearchField"),
            x => x.IsEmpty(x => x.OtherSearchField),
            x => x.IsEmpty(x => x.OtherSearchField),
            x => x.IsEmpty(x => x.OtherSearchField)
        );
}
