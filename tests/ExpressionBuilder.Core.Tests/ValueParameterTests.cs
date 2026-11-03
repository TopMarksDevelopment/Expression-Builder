namespace ExpressionBuilder.Tests;

using TopMarksDevelopment.ExpressionBuilder.Extensions;

/// <summary>
/// A filter keeps the values it compares with in variables, so a database provider can send them as parameters (one
/// plan and one cache entry for every value). Everything else about the expression stays as it was.
/// </summary>
public class ValueParameterTests
{
    static readonly Product[] Products = [new(1), new(2), new(3)];

    [Fact(
        DisplayName = "A value is read from a variable, not written in as a constant"
    )]
    public void ValuesAreVariables()
    {
        var filter = new Filter<Product>();
        filter.Equal(p => p.Name, "Product 2");

        var text = filter.ToExpression().ToString();

        Assert.DoesNotContain("\"Product 2\"", text);
        Assert.Contains(".Value", text);
    }

    [Fact(
        DisplayName = "InlineValues writes them back as constants, which is the expression as it was"
    )]
    public void InlineValuesRestoresConstants()
    {
        var filter = new Filter<Product>();
        filter.Equal(p => p.Name, "Product 2");

        Assert.Equal(
            "x => (x.Name == \"Product 2\")",
            filter.ToExpression().InlineValues().ToString()
        );
    }

    [Fact(DisplayName = "The expression finds the same products either way")]
    public void SameResults()
    {
        var filter = new Filter<Product>();
        filter
            .Equal(p => p.Name, "Product 2")
            .Or()
            .LessThanOrEqual(p => p.Id, 1);

        var expression = filter.ToExpression();

        Assert.Equal<int>(
            [1, 2],
            Products
                .Where(expression.Compile())
                .Select(p => p.Id)
                .Order()
                .ToList()
        );
        Assert.Equal(
            Products.Where(expression.Compile()).Select(p => p.Id),
            Products
                .Where(expression.InlineValues().Compile())
                .Select(p => p.Id)
        );
    }

    [Fact(
        DisplayName = "A filter used on a list is compiled from constants: the compiler is slower with variables"
    )]
    public void ListFiltersAreCompiledFromConstants()
    {
        var filter = new Filter<Product>();
        filter
            .Equal(p => p.Name, "Product 2")
            .Or()
            .LessThanOrEqual(p => p.Id, 1);

        Func<Product, bool> func = filter;

        // Values the delegate has to fetch are held in its closure; there should be none
        var constants = (object[])
            func.Target!.GetType()
                .GetField("Constants")!
                .GetValue(func.Target)!;

        Assert.DoesNotContain(constants, c => c is IValueHolder);
        Assert.Equal<int>(
            [1, 2],
            Products.Where(func).Select(p => p.Id).Order().ToList()
        );
    }

    [Fact(
        DisplayName = "Filtering a list through IFilterable finds the same products"
    )]
    public void FilterableFindsTheSameProducts()
    {
        var filterable = Products.ToFilterable();
        filterable
            .Equal(p => p.Name, "Product 2")
            .Or()
            .LessThanOrEqual(p => p.Id, 1);

        Assert.Equal<int>(
            [1, 2],
            filterable.Select(p => p.Id).Order().ToList()
        );
    }

    [Fact(
        DisplayName = "The shape of the query stays in the expression: null and the empty text"
    )]
    public void ShapeStaysAsConstants()
    {
        var filter = new Filter<Product>();
        filter.IsNull(p => p.OtherSearchField).Or().Equal(p => p.Name, "");

        var text = filter.ToExpression().ToString();

        Assert.Contains("null", text);
        Assert.Contains("\"\"", text);
        Assert.DoesNotContain(".Value", text);
    }

    [Fact(DisplayName = "Dates and numbers are variables too")]
    public void DatesAndNumbers()
    {
        var filter = new Filter<Product>();
        filter
            .LessThanOrEqual(p => p.Id, 5)
            .Or()
            .LessThanOrEqual(p => p.CreatedAt, new DateTime(2020, 1, 1));

        var text = filter.ToExpression().ToString();

        Assert.DoesNotContain("2020", text);
        Assert.Equal(2, text.Split("ValueHolder").Length - 1);
    }

    [Fact(
        DisplayName = "An expression with nothing to move comes back as it was"
    )]
    public void NothingToMove()
    {
        Expression<Func<Product, bool>> expression = p =>
            p.OtherSearchField == null;

        Assert.Equal(
            expression.ToString(),
            ValueParameters.Parameterise(expression).ToString()
        );
        Assert.Equal(
            expression.ToString(),
            expression.InlineValues().ToString()
        );
    }

    [Fact(DisplayName = "A constant that is not a plain value is left alone")]
    public void OtherConstantsAreLeftAlone()
    {
        var parameter = Expression.Parameter(typeof(Product), "p");
        var thing = new object();
        var expression = Expression.Lambda<Func<Product, bool>>(
            Expression.Equal(
                Expression.Constant(thing),
                Expression.Constant(thing)
            ),
            parameter
        );

        Assert.DoesNotContain(
            ".Value",
            ValueParameters.Parameterise(expression).ToString()
        );
    }

    [Fact(DisplayName = "The values of a collection's items are variables too")]
    public void InsideCollections()
    {
        var filter = new Filter<Product>();
        filter
            .OpenCollection(p => p.StockLocations)
            .Equal(s => s.Location, "Warehouse")
            .CloseCollection<Product>();

        var text = filter.ToExpression().ToString();

        Assert.DoesNotContain("Warehouse", text);
        Assert.Contains(".Value", text);
    }
}
