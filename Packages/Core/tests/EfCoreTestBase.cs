namespace ExpressionBuilder.Tests;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class EfCoreTestBase
{
    readonly DbContextOptions<EbContext> _contextOptions;

    public EfCoreTestBase()
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

    internal IQueryable<T> GetCollection<T>()
        where T : class =>
        typeof(T) == typeof(Product)
            ? (IQueryable<T>)CreateContext()
                .Products
                .Include(x => x.Categories)
                .ThenInclude(x => x.Products)
                .ThenInclude(x => x.Categories)
            : (typeof(T) == typeof(StockLocation)
            ? (IQueryable<T>)CreateContext()
                .StockLocations
                .Include(x => x.Product)
            : (IQueryable<T>)CreateContext()
                .Categories
                .Include(x => x.Products)
                .ThenInclude(x => x.Categories)
                .ThenInclude(x => x.Products));

    internal IQueryable<IItemable> GetCollection(Type testType) =>
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

    internal IQueryable<Category> GetCategory() =>
        CreateContext().Categories
            .Include(x => x.Products)
            .ThenInclude(x => x.Categories)
            .ThenInclude(x => x.Products)
            .ThenInclude(x => x.StockLocations);

    internal IQueryable<Product> GetProducts() =>
        CreateContext().Products
            .Include(x => x.Categories)
            .ThenInclude(x => x.Products)
            .ThenInclude(x => x.StockLocations);
}
