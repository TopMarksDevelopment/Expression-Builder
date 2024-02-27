namespace ExpressionBuilder.Tests;

using System.Linq;
using ExpressionBuilder.Tests.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Operations;
using Xunit;

[Collection("Checks for the core")]
public class CoreTests
{
    DbContextOptions<EbContext> _contextOptions;

    IQueryable<IItemable> GetCollection(Type testType) =>
        testType == typeof(Product)
            ? CreateContext().Products
                .Include<
                    global::ExpressionBuilder.Tests.Models.Product,
                    global::System.Collections.Generic.ICollection<global::ExpressionBuilder.Tests.Models.Category>
                >(x => x.Categories)
                .ThenInclude<
                    global::ExpressionBuilder.Tests.Models.Product,
                    global::ExpressionBuilder.Tests.Models.Category,
                    global::System.Collections.Generic.ICollection<global::ExpressionBuilder.Tests.Models.Product>
                >(x => x.Products)
                .ThenInclude<
                    global::ExpressionBuilder.Tests.Models.Product,
                    global::ExpressionBuilder.Tests.Models.Product,
                    global::System.Collections.Generic.ICollection<global::ExpressionBuilder.Tests.Models.Category>
                >(x => x.Categories)
            : GetCategory();

    IQueryable<Category> GetCategory() =>
        CreateContext().Categories
            .Include(x => x.Products)
            .ThenInclude(x => x.Categories)
            .ThenInclude(x => x.Products);

    public CoreTests()
    {
        // Create and open a connection. This creates the SQLite in-memory database, which will persist until the connection is closed
        // at the end of the test (see Dispose below).
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        // These options will be used by the context instances in this test suite, including the connection opened above.
        _contextOptions = new DbContextOptionsBuilder<EbContext>()
            .UseSqlite(connection)
            .Options;

        // Create the schema and seed some data
        using var context = new EbContext(_contextOptions);

        if (context.Database.EnsureCreated())
        {
            using var viewCommand = context.Database
                .GetDbConnection()
                .CreateCommand();
            viewCommand.CommandText =
                @"
CREATE VIEW AllResources AS
SELECT Id
FROM Product;";
            viewCommand.ExecuteNonQuery();
        }

        context.AddRange(ResultMatchSeed.ProductsBase);
        context.SaveChanges();

        var categories = ResultMatchSeed.CategoriesBase(null);
        context.AddRange(categories);
        context.SaveChanges();

        var i = 0;
        foreach (var categ in context.Categories.AsTracking())
        {
            i = i switch
            {
                1 => 3,
                3 => 6,
                _ => 1
            };

            foreach (
                var p in ResultMatchSeed.GetProds(
                    i,
                    context.Products.AsTracking()
                )
            )
            {
                p.Category = categ;
                p.Categories.Add(categ);
                categ.Products.Add(p);
            }
        }

        context.SaveChanges();
    }

    EbContext CreateContext() => new(_contextOptions);

    [Theory(DisplayName = "No filters works still")]
    [InlineData(typeof(Category))]
    [InlineData(typeof(Product))]
    public void MultiCollectionTest(Type testType)
    {
        var exception = Record.Exception(
            () => GetCollection(testType).Where(new Filter<IItemable>())
        );

        Assert.Null(exception);
    }

    [Fact(DisplayName = "Fixed: Nullable trim error")]
    public void CollectionTestNoLongerThrows()
    {
        var filter = new Filter<Category>();
        int? nullId = 1;

        filter
            .OpenCollection(x => x.Products)
            .Equal(x => x.Id, nullId.Value)
            .CloseCollection();

        var exception = Record.Exception(() => GetCategory().Where(filter));

        Assert.Null(exception);
    }
}
