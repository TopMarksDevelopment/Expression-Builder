namespace ExpressionBuilder.Tests;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// A group on a collection can ask for <c>Any</c> of the items to match (the default) or <c>All</c> of them.
/// </summary>
[Collection("Matching queries - Results")]
public class CollectionMatchTests : EfCoreTestBase
{
    static Filter<Category> ProductsWhereIdAtMost(int id, Matches? match)
    {
        var filter = new Filter<Category>();

        filter
            .OpenCollection(x => x.Products)
            .LessThanOrEqual(x => x.Id, id)
            .CloseCollection();

        filter.Items.OfType<FilterGroup>().Single().Match = match;

        return filter;
    }

    static int[] Ids(IEnumerable<Category> categories) =>
        [.. categories.Select(c => c.Id).Order()];

    [Fact(
        DisplayName = "Collection group: no match means Any, as it always did"
    )]
    public void NoMatchIsAny()
    {
        var filter = ProductsWhereIdAtMost(2, null);

        Assert.Contains(".Any(", filter.ToExpression().ToString());
        Assert.DoesNotContain(".All(", filter.ToExpression().ToString());
    }

    [Fact(DisplayName = "Collection group: All builds Enumerable.All")]
    public void AllBuildsAll()
    {
        var expression = ProductsWhereIdAtMost(2, Matches.All)
            .ToExpression()
            .ToString();

        Assert.Contains(".All(", expression);
        Assert.DoesNotContain(".Any(", expression);
    }

    [Theory(
        DisplayName = "Collection group: All and Any give the results LINQ gives"
    )]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(100)]
    public void QuantifiersMatchLinq(int id)
    {
        var categories = GetCategory().AsEnumerable().ToList();

        var any = ProductsWhereIdAtMost(id, Matches.Any);
        var all = ProductsWhereIdAtMost(id, Matches.All);

        Assert.Equal(
            Ids(categories.Where(c => c.Products.Any(p => p.Id <= id))),
            Ids(GetCategory().Where(any))
        );
        Assert.Equal(
            Ids(categories.Where(c => c.Products.All(p => p.Id <= id))),
            Ids(GetCategory().Where(all))
        );

        // Over a list, which is compiled rather than translated
        Assert.Equal(
            Ids(categories.Where(c => c.Products.All(p => p.Id <= id))),
            Ids(categories.Where(all))
        );
    }

    [Fact(
        DisplayName = "Collection group: All is true when there are no items, as in LINQ"
    )]
    public void AllIsTrueForNothing()
    {
        // The seed data has no category without products, so make one
        List<Category> categories =
        [
            new() { Id = 1 },
            new() { Id = 2, Products = [new Product(5)] },
        ];

        // Nothing has an id below this, so only the empty category can match
        var all = ProductsWhereIdAtMost(-1, Matches.All);

        Assert.Equal([1], Ids(categories.Where(all)));
        Assert.Equal(
            [1],
            Ids(categories.Where(c => c.Products.All(p => p.Id <= -1)))
        );
    }

    [Fact(
        DisplayName = "Collection group: All reaches the database as NOT EXISTS"
    )]
    public void AllIsTranslated()
    {
        var sql = GetCategory().Where(ProductsWhereIdAtMost(2, Matches.All));

        Assert.Contains("NOT EXISTS", sql.ToQueryString());
    }

    [Fact(DisplayName = "Collection group: All after a one-to-one chain")]
    public void AllAfterAChain()
    {
        // Product.Category is one object; its Products is the collection
        var filter = new Filter<Product>();

        filter
            .OpenCollection(x => x.Category!.Products)
            .LessThanOrEqual(x => x.Id, 2)
            .CloseCollection();

        filter.Items.OfType<FilterGroup>().Single().Match = Matches.All;

        var products = GetProducts().AsEnumerable().ToList();

        Assert.Equal(
            products
                .Where(p => p.Category != null)
                .Where(p => p.Category!.Products.All(q => q.Id <= 2))
                .Select(p => p.Id)
                .Order(),
            GetProducts().Where(filter).Select(p => p.Id).AsEnumerable().Order()
        );
    }

    [Fact(
        DisplayName = "Collection group: only the last collection in a chain is asked for All"
    )]
    public void OnlyTheLastCollectionIsAll()
    {
        var filter = new Filter<Category>();

        filter
            .OpenCollection(x => x.Products)
            .OpenCollection(x => x.StockLocations)
            .LessThanOrEqual(x => x.Quantity, 11)
            .CloseCollection<Product>()
            .CloseCollection();

        // The outer group stays on Any; the inner one is All
        var outer = filter.Items.OfType<FilterGroup>().Single();
        outer.Items.OfType<FilterGroup>().Single().Match = Matches.All;

        var expression = filter.ToExpression().ToString();

        Assert.Contains(".Any(", expression);
        Assert.Contains(".All(", expression);
    }

    [Fact(
        DisplayName = "Collection group: Match survives JSON, and is left out when it is Any"
    )]
    public void JsonKeepsMatch()
    {
        var all = ProductsWhereIdAtMost(2, Matches.All);
        var any = ProductsWhereIdAtMost(2, null);

        var allJson = JsonSerializer.Serialize(all);
        var anyJson = JsonSerializer.Serialize(any);

        // Filters saved before this existed read as Any, and Any is saved the way it always was
        Assert.DoesNotContain("\"Match\"", anyJson);
        Assert.Contains("\"Match\"", allJson);

        var back = JsonSerializer.Deserialize<Filter<Category>>(allJson)!;

        Assert.Equal(
            Matches.All,
            back.Items.OfType<FilterGroup>().Single().Match
        );
        Assert.Null(
            JsonSerializer
                .Deserialize<Filter<Category>>(anyJson)!
                .Items.OfType<FilterGroup>()
                .Single()
                .Match
        );
    }

    [Fact(DisplayName = "Collection group: Match survives protobuf")]
    public void ProtobufKeepsMatch()
    {
        foreach (var match in new Matches?[] { null, Matches.Any, Matches.All })
        {
            ProductsWhereIdAtMost(2, match).SerializeTo(out var bytes);

            var back = Filter<Category>.DeserializeFrom(bytes);

            Assert.Equal(
                match,
                back.Items.OfType<FilterGroup>().Single().Match
            );
        }
    }
}
