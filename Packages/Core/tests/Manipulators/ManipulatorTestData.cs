namespace ExpressionBuilder.Tests;

using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class ManipulatorTestData : TheoryData<TestBuilder<Product>>
{
    internal static Func<string, string>? ApplyReplacements = null;

    public ManipulatorTestData() => Add(ReplaceTest);

    static TestBuilder<Product> ReplaceTest =>
        new(
            "Replace Manipulator Test",
            x => x.Name != null && x.Name.Replace(" ", "") == "Product 2",
            x =>
                x.Equal(
                    "Name",
                    "Product 2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")]
                    }
                ),
            x =>
                x.Equal(
                    y => y.Name,
                    "Product 2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")]
                    }
                ),
            x =>
                x.Equal(
                    y => y.Name,
                    "Product 2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")]
                    }
                ),
            x =>
                x.Equal(
                    y => y.Name,
                    "Product 2",
                    new FilterStatementOptions
                    {
                        Manipulators = [new ReplaceManipulator(" ", "")]
                    }
                )
        );
}
