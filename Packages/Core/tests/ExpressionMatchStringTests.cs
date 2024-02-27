namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using TopMarksDevelopment.ExpressionBuilder.Api;

[Collection("Matching Expressions to Strings")]
public class ExpressionMatchStringTests
{
    public static IEnumerable<object[]> GetAll() =>
        AllTests.GetAllStringMatchers();

    [Theory(DisplayName = "Factory")]
    [MemberData(nameof(GetAll))]
    internal void CheckFactoryStrings<T, T2>(ExpressionTestBuilder<T, T2> match)
        where T : class, IItemable
        where T2 : IFilterItem =>
        Assert.Equal(
            match.Expected,
            match.ToGet.Invoke((T2)match.Factory.Get<T>().Items.First())
        );

    [Theory(DisplayName = "Filterable")]
    [MemberData(nameof(GetAll))]
    internal void CheckFilterableStrings<T, T2>(
        ExpressionTestBuilder<T, T2> match
    )
        where T : class, IItemable
        where T2 : IFilterItem =>
        Assert.Equal(
            match.Expected,
            match.ToGet.Invoke(
                (T2)match.Filterable.Get(null).Filter.Items.First()
            )
        );

    [Theory(DisplayName = "QueryFilterable")]
    [MemberData(nameof(GetAll))]
    internal void CheckQueryableStrings<T, T2>(
        ExpressionTestBuilder<T, T2> match
    )
        where T : class, IItemable
        where T2 : IFilterItem =>
        Assert.Equal(
            match.Expected,
            match.ToGet.Invoke(
                (T2)match.QueryFilterable.Get(null).Filter.Items.First()
            )
        );

    [Theory(DisplayName = "Fluent")]
    [MemberData(nameof(GetAll))]
    internal void CheckFluentStrings<T, T2>(ExpressionTestBuilder<T, T2> match)
        where T : class, IItemable
        where T2 : IFilterItem =>
        Assert.Equal(
            match.Expected,
            match.ToGet.Invoke((T2)match.Fluent.Get().Items.First())
        );
}
