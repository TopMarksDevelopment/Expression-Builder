namespace ExpressionBuilder.Tests.Enumerables;

using System.Collections;

/// <summary>
/// The non-generic <see cref="IFilterable"/>/<see cref="IQueryFilterable"/> members are used when the
/// type of the list is only known at run time. Several of them used to call themselves forever.
/// </summary>
[Collection("Non-generic filterables")]
public class NonGenericFilterableTests
{
    static readonly ResultMatchSeed Seed = new();

    static List<Category> Expected(Func<Category, bool> predicate) =>
        Seed.Categories.Where(predicate).OrderBy(c => c.Id).ToList();

    static List<Category> Run(IEnumerable results) =>
        results.Cast<Category>().OrderBy(c => c.Id).ToList();

    #region IFilterable

    [Fact(
        DisplayName = "IFilterable: a collection can be opened and closed by name"
    )]
    public void FilterableCollectionByName()
    {
        IFilterable filterable = Seed.Categories.ToFilterable();

        filterable.OpenCollection("Products");
        filterable.Add("Id", new Equal(), [1]);
        filterable.CloseCollection();

        Assert.Equal(
            Expected(c => c.Products.Any(p => p.Id == 1)),
            Run(filterable)
        );
    }

    [Fact(DisplayName = "IFilterable: a group can be opened and closed")]
    public void FilterableGroup()
    {
        IFilterable filterable = Seed.Categories.ToFilterable();

        filterable.OpenGroup();
        filterable.Add("Id", new Equal(), [1]).Or().Add("Id", new Equal(), [2]);
        filterable.CloseGroup().And().Add("Name", new Equal(), ["Category 1"]);

        Assert.Equal(
            Expected(c => (c.Id == 1 || c.Id == 2) && c.Name == "Category 1"),
            Run(filterable)
        );
    }

    [Fact(DisplayName = "IFilterable: a connection can close a collection")]
    public void FilterableConnectionClosesCollection()
    {
        IFilterable filterable = Seed.Categories.ToFilterable();

        filterable.OpenCollection("Products");
        var connection = filterable.Add("Id", new Equal(), [1]);
        connection.CloseCollection().And().Add("Id", new Equal(), [1]);

        Assert.Equal(
            Expected(c => c.Id == 1 && c.Products.Any(p => p.Id == 1)),
            Run(filterable)
        );
    }

    #endregion IFilterable

    #region IQueryFilterable

    [Fact(
        DisplayName = "IQueryFilterable: a collection can be opened and closed by name"
    )]
    public void QueryFilterableCollectionByName()
    {
        IQueryFilterable filterable = Seed
            .Categories.AsQueryable()
            .AsFilterable();

        filterable.OpenCollection("Products");
        filterable.Add("Id", new Equal(), [1]);
        filterable.CloseCollection();

        Assert.Equal(
            Expected(c => c.Products.Any(p => p.Id == 1)),
            Run(filterable)
        );
    }

    [Fact(DisplayName = "IQueryFilterable: a group can be opened and closed")]
    public void QueryFilterableGroup()
    {
        IQueryFilterable filterable = Seed
            .Categories.AsQueryable()
            .AsFilterable();

        filterable.OpenGroup();
        filterable.Add("Id", new Equal(), [1]).Or().Add("Id", new Equal(), [2]);
        filterable.CloseGroup().And().Add("Name", new Equal(), ["Category 1"]);

        Assert.Equal(
            Expected(c => (c.Id == 1 || c.Id == 2) && c.Name == "Category 1"),
            Run(filterable)
        );
    }

    [Fact(
        DisplayName = "IQueryFilterable: a connection can close a collection"
    )]
    public void QueryFilterableConnectionClosesCollection()
    {
        IQueryFilterable filterable = Seed
            .Categories.AsQueryable()
            .AsFilterable();

        filterable.OpenCollection("Products");
        var connection = filterable.Add("Id", new Equal(), [1]);
        connection.CloseCollection().And().Add("Id", new Equal(), [1]);

        Assert.Equal(
            Expected(c => c.Id == 1 && c.Products.Any(p => p.Id == 1)),
            Run(filterable)
        );
    }

    #endregion IQueryFilterable
}
