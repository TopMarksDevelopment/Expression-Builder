namespace ExpressionBuilder.Tests;

using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

internal class ExpressionStringTestData : TheoryData
{
    public ExpressionStringTestData() =>
        AddRows(
            [
                [CollectionExpressionMatchesString],
                [PropertyChainExpressionMatchesString]
            ]
        );

    static ExpressionTestBuilder<
        Product,
        FilterGroup
    > CollectionExpressionMatchesString =>
        new(
            "Collections declared by string or expression should match",
            "Categories[]",
            x => x.OpenCollection("Categories").CloseCollection(),
            x => x.OpenCollection(y => y.Categories).CloseCollection<Product>(),
            x => x.OpenCollection(y => y.Categories).CloseCollection<Product>(),
            x => x.OpenCollection(y => y.Categories).CloseCollection<Product>(),
            x => x.ParentPropertyExpression!
        );

    static ExpressionTestBuilder<
        Product,
        FilterStatement<int>
    > PropertyChainExpressionMatchesString =>
        new(
            "Property chains should match if called with expression notion",
            "Category.Id",
            x => x.Equal<Product, int>(x => x.Category!.Id, 2),
            x => x.Equal(x => x.Category!.Id, 2),
            x => x.Equal(x => x.Category!.Id, 2),
            x => x.Equal(x => x.Category!.Id, 2),
            x => x.PropertyId
        );
}
