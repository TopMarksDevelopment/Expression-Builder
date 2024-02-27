namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public class AllManipulatorTests
{
    internal static Func<string, string>? ApplyReplacements = null;

    internal static IEnumerable<object[]> GetAllMatchers() =>
        [
            [ReplaceTest],
        ];

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
