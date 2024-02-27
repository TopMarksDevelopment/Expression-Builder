namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public class AllTests
{
    internal static DateTime? createdDate = new(2023, 10, 1, 10, 0, 10);
    internal static DateTime? otherDate = createdDate.Value.AddMinutes(10);

    internal static string ApplyReplacements(string input) =>
        input
            .Replace("new [] {", "[")
            .Replace("}.", "].")
            .Replace("[AllTests.createdDate", "[" + createdDate.ToString())
            .Replace("AllTests.otherDate]", otherDate.ToString() + "]")
            .Replace(
                "ResultMatchSeed.CreatedDate.Year",
                ResultMatchSeed.CreatedDate.Year.ToString()
            )
            .Replace(
                "ResultMatchSeed.CreatedDate",
                ResultMatchSeed.CreatedDate.ToString()
            );

    #region Match tests

    public static IEnumerable<object[]> GetAllMatchers() =>
        [
            [DeepNullChecker],
            [SimpleAddMethod],
            [SimpleEqual],
            [SimpleGroupEqual],
            [SimpleEqualOrEqualCollectionAndEqual],
            [SimpleNullCheck],
            [VariousLevelNullChecks],
            [ComplexCollection],
            [NullableDateTimeCheck],
            [DateTimeYearCheck],
            [NullableDateTimeYearCheck],
            [ReplaceMethodCheck],
            [ReplaceManipulatorCheck]
        ];

    static TestBuilder<Product> DeepNullChecker =>
        new(
            "Deep Nullable",
            x => x.Brand != null && x.Brand.Supplier.Id == 2,
            x => x.Equal("Brand.Supplier.Id", 2),
            x => x.Equal(y => y.Brand!.Supplier.Id, 2),
            x => x.Equal(y => y.Brand!.Supplier.Id, 2),
            x => x.Equal(y => y.Brand!.Supplier.Id, 2)
        );

    static TestBuilder<Product> SimpleAddMethod =>
        new(
            "Simple equal",
            x => x.Id == 2,
            x => x.Add("Id", new Equal(), 2),
            x => x.Add(y => y.Id, new Equal(), 2),
            x => x.Add(y => y.Id, new Equal(), 2),
            x => x.Add(x => x.Id, new Equal(), 2)
        );

    static TestBuilder<Product> SimpleEqual =>
        new(
            "Simple equal",
            x => x.Id == 2,
            x => x.Equal("Id", 2),
            x => x.Equal(y => y.Id, 2),
            x => x.Equal(y => y.Id, 2),
            x => x.Equal(x => x.Id, 2)
        );

    static TestBuilder<Category> SimpleGroupEqual =>
        new(
            "Simple (Equal)Group Or Equal",
            x => (x.Id == 1 || x.Id == 2) || x.Id == 3,
            x =>
                x.OpenGroup()
                    .Equal("Id", 1)
                    .Or()
                    .Equal("Id", 2)
                    .CloseGroup()
                    .Or()
                    .Equal("Id", 3),
            x =>
                x.OpenGroup()
                    .Equal(x => x.Id, 1)
                    .Or()
                    .Equal(x => x.Id, 2)
                    .CloseGroup()
                    .Or()
                    .Equal(x => x.Id, 3),
            x =>
                x.OpenGroup()
                    .Equal(x => x.Id, 1)
                    .Or()
                    .Equal(x => x.Id, 2)
                    .CloseGroup()
                    .Or()
                    .Equal(x => x.Id, 3),
            x =>
                x.OpenGroup()
                    .Equal(x => x.Id, 1)
                    .Or()
                    .Equal(x => x.Id, 2)
                    .CloseGroup()
                    .Or()
                    .Equal(x => x.Id, 3)
        );

    static TestBuilder<Category> SimpleEqualOrEqualCollectionAndEqual =>
        new(
            "Simple (Equal Or Equal)Collection And Equal",
            x => x.Products.Any(p => p.Id == 1 || p.Id == 2) && x.Id == 2,
            x =>
                x.OpenCollection("Products")
                    .Equal("Id", 1)
                    .Or()
                    .Equal("Id", 2)
                    .CloseCollection()
                    .And()
                    .Equal("Id", 2),
            x =>
                x.OpenCollection(x => x.Products)
                    .Equal(p => p.Id, 1)
                    .Or()
                    .Equal(p => p.Id, 2)
                    .CloseCollection<Category>()
                    .And()
                    .Equal(x => x.Id, 2),
            x =>
                x.OpenCollection(x => x.Products)
                    .Equal(p => p.Id, 1)
                    .Or()
                    .Equal(p => p.Id, 2)
                    .CloseCollection<Category>()
                    .And()
                    .Equal(x => x.Id, 2),
            x =>
                x.OpenCollection(x => x.Products)
                    .Equal(p => p.Id, 1)
                    .Or()
                    .Equal(p => p.Id, 2)
                    .CloseCollection<Category>()
                    .And()
                    .Equal(x => x.Id, 2)
        );

    static TestBuilder<Product> SimpleNullCheck =>
        new(
            "Simple Null Check",
            x => x.Category != null && x.Category.Id == 2,
            x => x.Equal("Category.Id", 2),
            x => x.Equal(p => p.Category!.Id, 2),
            x => x.Equal(p => p.Category!.Id, 2),
            x => x.Equal(p => p.Category!.Id, 2)
        );

    static TestBuilder<Product> VariousLevelNullChecks =>
        new(
            "Null Checks At Various Levels",
            x =>
                (x.Category != null && x.Category.Id == 2)
                || (
                    x.Category != null
                    && (
                        x.Category.Products.Any(p =>
                            p.Category != null && p.Category.Id == 2
                        )
                    )
                ),
            x =>
                x.Equal("Category.Id", 2)
                    .Or()
                    .OpenCollection("Category.Products")
                    .Equal("Category.Id", 2),
            x =>
                x.Equal(p => p.Category!.Id, 2)
                    .Or()
                    .OpenCollection(x => x.Category!.Products)
                    .Equal(p => p.Category!.Id, 2),
            x =>
                x.Equal(p => p.Category!.Id, 2)
                    .Or()
                    .OpenCollection(x => x.Category!.Products)
                    .Equal(p => p.Category!.Id, 2),
            x =>
                x.Equal(p => p.Category!.Id, 2)
                    .Or()
                    .OpenCollection(x => x.Category!.Products)
                    .Equal(p => p.Category!.Id, 2)
        );

    static TestBuilder<Product> ComplexCollection =>
        new(
            "Complex collection",
            x =>
                (
                    (
                        (x.Category != null)
                        && (
                            x.Category.Products.Any(p =>
                                (p.Name != null && p.Name == "Product 2")
                                || p.Id == 1
                                || (
                                    p.Categories.Any(c =>
                                        c.Id == 1 || c.Id == 2
                                    )
                                )
                            )
                        )
                        && x.Id == 2
                    )
                ),
            x =>
                x.OpenCollection("Category.Products")
                    .Equal("Name", "Product 2")
                    .Or()
                    .Equal("Id", 1)
                    .Or()
                    .OpenCollection("Categories")
                    .Equal("Id", 1)
                    .Or()
                    .Equal("Id", 2)
                    .CloseCollection()
                    .CloseCollection()
                    .And()
                    .Equal("Id", 2),
            x =>
                x.OpenCollection(x => x.Category!.Products)
                    .Equal(x => x.Name, "Product 2")
                    .Or()
                    .Equal(x => x.Id, 1)
                    .Or()
                    .OpenCollection(x => x.Categories)
                    .Equal(x => x.Id, 1)
                    .Or()
                    .Equal(x => x.Id, 2)
                    .CloseCollection<Product>()
                    .CloseCollection<Product>()
                    .And()
                    .Equal(x => x.Id, 2),
            x =>
                x.OpenCollection(x => x.Category!.Products)
                    .Equal(x => x.Name, "Product 2")
                    .Or()
                    .Equal(x => x.Id, 1)
                    .Or()
                    .OpenCollection(x => x.Categories)
                    .Equal(x => x.Id, 1)
                    .Or()
                    .Equal(x => x.Id, 2)
                    .CloseCollection<Product>()
                    .CloseCollection<Product>()
                    .And()
                    .Equal(x => x.Id, 2),
            x =>
                x.OpenCollection(x => x.Category!.Products)
                    .Equal(x => x.Name, "Product 2")
                    .Or()
                    .Equal(x => x.Id, 1)
                    .Or()
                    .OpenCollection(x => x.Categories)
                    .Equal(x => x.Id, 1)
                    .Or()
                    .Equal(x => x.Id, 2)
                    .CloseCollection<Product>()
                    .CloseCollection<Product>()
                    .And()
                    .Equal(x => x.Id, 2)
        );

    internal static TestBuilder<Product> NullableDateTimeCheck =>
        new(
            "Nullable DateTime Check",
            x =>
                x.CreatedAt != null
                && x.CreatedAt.Value == ResultMatchSeed.CreatedDate,
            x =>
                x.Equal<Product, DateTime?>(
                    y => y.CreatedAt,
                    ResultMatchSeed.CreatedDate
                ),
            x => x.Equal(y => y.CreatedAt, ResultMatchSeed.CreatedDate),
            x => x.Equal(y => y.CreatedAt, ResultMatchSeed.CreatedDate),
            x => x.Equal(x => x.CreatedAt, ResultMatchSeed.CreatedDate)
        );

    internal static TestBuilder<StockLocation> DateTimeYearCheck =>
        new(
            "DateTime Year Check",
            x => x.CreatedAt.Year == ResultMatchSeed.CreatedDate.Year,
            x =>
                x.Equal<StockLocation, int>(
                    y => y.CreatedAt.Year,
                    ResultMatchSeed.CreatedDate.Year
                ),
            x =>
                x.Equal(
                    y => y.CreatedAt.Year,
                    ResultMatchSeed.CreatedDate.Year
                ),
            x =>
                x.Equal(
                    y => y.CreatedAt.Year,
                    ResultMatchSeed.CreatedDate.Year
                ),
            x =>
                x.Equal(y => y.CreatedAt.Year, ResultMatchSeed.CreatedDate.Year)
        );

    internal static TestBuilder<Product> NullableDateTimeYearCheck =>
        new(
            "Nullable DateTime Year Check",
            x =>
                x.CreatedAt != null
                && x.CreatedAt.Value.Year == ResultMatchSeed.CreatedDate.Year,
            x =>
                x.Equal<Product, int>(
                    y => y.CreatedAt!.Value.Year,
                    ResultMatchSeed.CreatedDate.Year
                ),
            x =>
                x.Equal(
                    y => y.CreatedAt!.Value.Year,
                    ResultMatchSeed.CreatedDate.Year
                ),
            x =>
                x.Equal(
                    y => y.CreatedAt!.Value.Year,
                    ResultMatchSeed.CreatedDate.Year
                ),
            x =>
                x.Equal(
                    y => y.CreatedAt!.Value.Year,
                    ResultMatchSeed.CreatedDate.Year
                )
        );

    internal static TestBuilder<Product> ReplaceMethodCheck =>
        new(
            "String.Replace method Check",
            x => x.Name != null && x.Name.Replace(" ", "") == "Product2",
            x =>
                x.Equal<Product, string>(
                    y => y.Name.Replace(" ", ""),
                    "Product2"
                ),
            x => x.Equal(y => y.Name.Replace(" ", ""), "Product2"),
            x => x.Equal(y => y.Name.Replace(" ", ""), "Product2"),
            x => x.Equal(y => y.Name.Replace(" ", ""), "Product2")
        );

    internal static TestBuilder<Product> ReplaceManipulatorCheck =>
        new(
            "Replace manipulator Check",
            x => x.Name != null && x.Name.Replace(" ", "") == "Product2",
            x =>
                x.Equal<Product, string>(
                    y => y.Name,
                    "Product2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")]
                    }
                ),
            x =>
                x.Equal(
                    y => y.Name,
                    "Product2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")]
                    }
                ),
            x =>
                x.Equal(
                    y => y.Name,
                    "Product2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")]
                    }
                ),
            x =>
                x.Equal(
                    y => y.Name,
                    "Product2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")]
                    }
                )
        );

    #endregion Match tests


    public static IEnumerable<object[]> GetAllStringMatchers() =>
        [
            [CollectionExpressionMatchesString],
            [PropertyChainExpressionMatchesString]
        ];

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

    #region OperationTests

    public static IEnumerable<object[]> GetAllOperations() =>
        [
            [NullPropertyHasNullCheck],
            [NonNullableHasNoNullCheck]
        ];

    static TestBuilder<Product> NullPropertyHasNullCheck =>
        new(
            "A null property should have null checks",
            x =>
                (x.CreatedAt != null)
                && (
                    new[] { createdDate, otherDate }.Contains(
                        (DateTime?)x.CreatedAt.Value
                    )
                ),
            x =>
                x.Equal<Product, DateTime?>(
                    x => x.CreatedAt,
                    [createdDate, otherDate]
                ),
            x => x.Equal(x => x.CreatedAt, [createdDate, otherDate]),
            x => x.Equal(x => x.CreatedAt, [createdDate, otherDate]),
            x => x.Equal(x => x.CreatedAt, [createdDate, otherDate])
        );

    static TestBuilder<Product> NonNullableHasNoNullCheck =>
        new(
            "No Null Check On Non-Nullables",
            x => (new[] { 1, 3 }.Contains(x.Id)),
            x => x.In<Product, int>(x => x.Id, [1, 3]),
            x => x.In(x => x.Id, [1, 3]),
            x => x.In(x => x.Id, [1, 3]),
            x => x.In(x => x.Id, [1, 3])
        );

    #endregion OperationTests
}
