namespace ExpressionBuilder.Tests;

using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class SerializationTestData : TheoryData<Filter<Category>>
{
    public SerializationTestData() =>
        AddRange(ReplaceManipulator, ReplaceMethod, SkipNulls);

    static Filter<Category> ReplaceManipulator =>
        (Filter<Category>)
            new Filter<Category>()
                .OpenCollection(x => x.Products)
                .Equal(
                    x => x.Name,
                    "Product2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")]
                    }
                )
                .Or()
                .Equal(x => x.Id, 2)
                .Or()
                .OpenCollection(x => x.Categories)
                .Equal(x => x.Id, 1)
                .Or()
                .Equal(x => x.Id, 2)
                .CloseCollection<Category>()
                .CloseCollection<Category>()
                .And()
                .Equal(x => x.Id, 2)
                .And();

    static Filter<Category> ReplaceMethod =>
        (Filter<Category>)
            new Filter<Category>()
                .Equal(x => x.Name, "Category 2")
                .And()
                .Equal(x => x.Name.Replace(" ", ""), "Category2")
                .And()
                .OpenCollection(x => x.Products)
                .Equal(x => x.Category!.Name, "Category 2")
                .And()
                .Equal(x => x.Category!.Name.Replace(" ", ""), "Category2")
                .CloseCollection<Category>()
                .And();

    static Filter<Category> SkipNulls =>
        (Filter<Category>)
            new Filter<Category>()
                .Equal(
                    x => x.Name,
                    "Category 2",
                    new FilterStatementOptions { SkipNullChecks = true }
                )
                .And();
}
