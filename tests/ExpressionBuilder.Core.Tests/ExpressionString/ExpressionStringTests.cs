namespace ExpressionBuilder.Tests;

[Collection("Matching Expressions to Strings")]
public class ExpressionMatchStringTests
{
    [Theory(DisplayName = "Factory")]
    [ClassData(typeof(ExpressionStringTestData))]
    internal void CheckFactoryStrings<T, T2>(ExpressionTestBuilder<T, T2> match)
        where T : class, IItemable
        where T2 : IFilterItem =>
        Assert.Equal(
            match.Expected,
            match.ToGet.Invoke((T2)match.Factory.Get<T>().Items.First())
        );

    [Theory(DisplayName = "Filterable")]
    [ClassData(typeof(ExpressionStringTestData))]
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
    [ClassData(typeof(ExpressionStringTestData))]
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
    [ClassData(typeof(ExpressionStringTestData))]
    internal void CheckFluentStrings<T, T2>(ExpressionTestBuilder<T, T2> match)
        where T : class, IItemable
        where T2 : IFilterItem =>
        Assert.Equal(
            match.Expected,
            match.ToGet.Invoke((T2)match.Fluent.Get().Items.First())
        );
}
