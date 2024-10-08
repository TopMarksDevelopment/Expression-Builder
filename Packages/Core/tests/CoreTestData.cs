namespace ExpressionBuilder.Tests;

internal class CoreTestData : TheoryData
{
    internal static string ApplyReplacements(string input) =>
        input
            .Replace(
                "ResultMatchSeed.CreatedDate.Year",
                ResultMatchSeed.CreatedDate.Year.ToString()
            )
            .Replace(
                "ResultMatchSeed.CreatedDate",
                ResultMatchSeed.CreatedDate.ToString()
            );

    public CoreTestData() =>
        AddRows(
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
                [ReplaceManipulatorCheck],
            ]
        );

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
                                p.Name == "Product 2"
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
            x => x.CreatedAt == ResultMatchSeed.CreatedDate,
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
            x => x.Name.Replace(" ", "") == "Product2",
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
            x => x.Name.Replace(" ", "") == "Product2",
            x =>
                x.Equal<Product, string>(
                    y => y.Name,
                    "Product2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")],
                    }
                ),
            x =>
                x.Equal(
                    y => y.Name,
                    "Product2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")],
                    }
                ),
            x =>
                x.Equal(
                    y => y.Name,
                    "Product2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")],
                    }
                ),
            x =>
                x.Equal(
                    y => y.Name,
                    "Product2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")],
                    }
                )
        );
}
