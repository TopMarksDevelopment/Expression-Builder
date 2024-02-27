namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("Matching queries - Results")]
public class EfCoreTests
{
    DbContextOptions<EbContext> _contextOptions;

    public EfCoreTests()
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

    IQueryable<T> GetCollection<T>()
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

    public static IEnumerable<object[]> GetAll() => AllTests.GetAllMatchers();

    [Theory(DisplayName = "Factory")]
    [MemberData(nameof(GetAll))]
    public void CheckFactoryStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Factory.ToResults(items)
        );
    }

    [Theory(DisplayName = "Filterable")]
    [MemberData(nameof(GetAll))]
    public void CheckFilterableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Filterable.ToResults(items)
        );
    }

    [Theory(DisplayName = "QueryFilterable")]
    [MemberData(nameof(GetAll))]
    public void CheckQueryableStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.QueryFilterable.ToResults(items)
        );
    }

    [Theory(DisplayName = "Fluent")]
    [MemberData(nameof(GetAll))]
    public void CheckFluentStrings<T>(TestBuilder<T> match)
        where T : class, IItemable
    {
        var items = GetCollection<T>();

        Assert.Equal(
            match.Expected.ToResult(items),
            match.Fluent.ToResults(items)
        );
    }
}
