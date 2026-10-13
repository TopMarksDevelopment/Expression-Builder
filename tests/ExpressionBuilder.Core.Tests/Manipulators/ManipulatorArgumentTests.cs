namespace ExpressionBuilder.Tests;

[Collection("Manipulator arguments")]
public class ManipulatorArgumentTests
{
    static readonly IEnumerable<Product> Products =
        new ResultMatchSeed().Products;

    [Fact(DisplayName = "A manipulator argument can be a variable")]
    public void ArgumentCanBeAVariable()
    {
        var space = " ";
        var filter = new Filter<Product>();

        filter.Equal(x => x.Name.Replace(space, ""), "Product2");

        Assert.Equal<int>([2], Products.Where(filter).Select(p => p.Id));
    }

    [Fact(DisplayName = "A manipulator argument can be a calculation")]
    public void ArgumentCanBeACalculation()
    {
        var spaces = new string(' ', 1);
        var filter = new Filter<Product>();

        filter.Equal(x => x.Name.Replace(spaces + "", ""), "Product3");

        Assert.Equal<int>([3], Products.Where(filter).Select(p => p.Id));
    }

    [Fact(
        DisplayName = "A manipulator argument can't use the item being filtered"
    )]
    public void ArgumentCantUseTheItem()
    {
        var filter = new Filter<Product>();

        Assert.Throws<NotSupportedException>(() =>
            filter.Equal(x => x.Name.Replace(x.Name, ""), "")
        );
    }

    [Fact(DisplayName = "A chain of method manipulators can use variables")]
    public void ChainOfManipulators()
    {
        var space = " ";
        var manipulators = ExpressionMethodManipulator
            .GetAll<string>(x => x.Replace(space, "").Trim())
            .ToArray();

        Assert.Equal<string>(
            ["Replace", "Trim"],
            manipulators.Select(m => m.Name)
        );
    }
}
