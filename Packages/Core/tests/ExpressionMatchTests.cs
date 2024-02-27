namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using Xunit;

[Collection("Matching queries - Expressions")]
public class ExpressionAllTests
{
    public static IEnumerable<object[]> GetAll() => AllTests.GetAllMatchers();

    [Theory(DisplayName = "Factory")]
    [MemberData(nameof(GetAll))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(AllTests.ApplyReplacements),
            match.Factory.ToMatchString<T>()
        );

    [Theory(DisplayName = "Filterable")]
    [MemberData(nameof(GetAll))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(AllTests.ApplyReplacements),
            match.Filterable.ToMatchString()
        );

    [Theory(DisplayName = "QueryFilterable")]
    [MemberData(nameof(GetAll))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(AllTests.ApplyReplacements),
            match.QueryFilterable.ToMatchString()
        );

    [Theory(DisplayName = "Fluent")]
    [MemberData(nameof(GetAll))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable =>
        Assert.Equal(
            match.Expected.ToMatchString(AllTests.ApplyReplacements),
            match.Fluent.ToMatchString()
        );
}
