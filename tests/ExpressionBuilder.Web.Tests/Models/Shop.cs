namespace ExpressionBuilder.Web.Tests.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// A small database to run builder-produced filters against: one of each kind of property, a related object, an
/// owned object, a collection, a collection inside a collection, and a collection of plain values.
/// </summary>
public class Account
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Email { get; set; }

    public int? Age { get; set; }

    public decimal Balance { get; set; }

    public double Score { get; set; }

    [DataType(DataType.Date)]
    public DateTime Born { get; set; }

    public DateTime? LastSeen { get; set; }

    public TimeOnly Opens { get; set; }

    public bool Active { get; set; }

    public bool? Verified { get; set; }

    public Tier Tier { get; set; }

    public Guid Reference { get; set; }

    public Location Home { get; set; } = new();

    public Employer? Employer { get; set; }

    public List<Order> Orders { get; set; } = [];

    public List<string> Tags { get; set; } = [];
}

public enum Tier
{
    Basic,
    Silver,
    Gold,
}

public class Location
{
    public string City { get; set; } = "";

    public string? Postcode { get; set; }
}

public class Employer
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
}

public class Order
{
    public int Id { get; set; }

    public string Reference { get; set; } = "";

    public DateTime Placed { get; set; }

    public decimal Total { get; set; }

    public List<OrderLine> Lines { get; set; } = [];
}

public class OrderLine
{
    public int Id { get; set; }

    public string Sku { get; set; } = "";

    public int Quantity { get; set; }

    public string? Note { get; set; }
}

public sealed class ShopContext(DbContextOptions<ShopContext> options)
    : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Account>().OwnsOne(a => a.Home);
}

/// <summary>An in-memory SQLite database with the same accounts as the list they are compared with.</summary>
public sealed class Shop : IDisposable
{
    readonly SqliteConnection _connection = new("Filename=:memory:");

    public Shop()
    {
        _connection.Open();

        Context = new ShopContext(
            new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(_connection)
                .Options
        );
        Context.Database.EnsureCreated();
        Context.Accounts.AddRange(Seed());
        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        // The same accounts in memory, for working out what the answer should be
        Accounts =
        [
            .. Context
                .Accounts.AsNoTracking()
                .AsSplitQuery()
                .Include(a => a.Home)
                .Include(a => a.Employer)
                .Include(a => a.Orders)
                    .ThenInclude(o => o.Lines),
        ];
    }

    public ShopContext Context { get; }

    public List<Account> Accounts { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }

    static List<Account> Seed() =>
        [
            new()
            {
                Id = 1,
                Name = "Alan Turing",
                Email = "alan@example.com",
                Age = 41,
                Balance = 120.5m,
                Score = 9.5,
                Born = new DateTime(1912, 6, 23),
                LastSeen = new DateTime(2025, 3, 1, 9, 30, 0),
                Opens = new TimeOnly(9, 0),
                Active = true,
                Verified = true,
                Tier = Tier.Gold,
                Reference = TestData.GuidOne,
                Home = new Location { City = "Wilmslow", Postcode = "SK9 1AA" },
                Employer = new Employer { Name = "Bletchley Park" },
                Tags = ["vip", "maths"],
                Orders =
                [
                    new Order
                    {
                        Reference = "A-1",
                        Placed = new DateTime(2025, 1, 10),
                        Total = 250m,
                        Lines =
                        [
                            new OrderLine
                            {
                                Sku = "BOMBE",
                                Quantity = 1,
                                Note = "fragile",
                            },
                            new OrderLine { Sku = "TAPE", Quantity = 12 },
                        ],
                    },
                    new Order
                    {
                        Reference = "A-2",
                        Placed = new DateTime(2025, 2, 20),
                        Total = 15m,
                        Lines = [new OrderLine { Sku = "TAPE", Quantity = 2 }],
                    },
                ],
            },
            new()
            {
                Id = 2,
                Name = "grace hopper",
                Email = null,
                Age = null,
                Balance = 0m,
                Score = 8,
                Born = new DateTime(1906, 12, 9),
                LastSeen = null,
                Opens = new TimeOnly(17, 30),
                Active = false,
                Verified = null,
                Tier = Tier.Silver,
                Reference = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Home = new Location { City = "Arlington", Postcode = null },
                Tags = ["navy"],
                Orders =
                [
                    new Order
                    {
                        Reference = "G-1",
                        Placed = new DateTime(2024, 11, 5),
                        Total = 1000m,
                        Lines =
                        [
                            new OrderLine
                            {
                                Sku = "COBOL",
                                Quantity = 5,
                                Note = "",
                            },
                        ],
                    },
                ],
            },
            new()
            {
                Id = 3,
                Name = "  Ada Lovelace ",
                Email = "ADA@EXAMPLE.COM",
                Age = 36,
                Balance = -5m,
                Score = 10,
                Born = new DateTime(1815, 12, 10),
                LastSeen = new DateTime(2024, 12, 25, 18, 0, 0),
                Opens = new TimeOnly(9, 0),
                Active = true,
                Verified = false,
                Tier = Tier.Basic,
                Reference = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Home = new Location { City = "London", Postcode = "W1 2AB" },
                Employer = new Employer { Name = "Babbage & Co" },
                Tags = [],
            },
            new()
            {
                Id = 4,
                Name = "",
                Email = "",
                Age = 5,
                Balance = 1m,
                Score = 0,
                Born = new DateTime(2020, 1, 1),
                LastSeen = new DateTime(2025, 6, 1),
                Opens = new TimeOnly(0, 0),
                Active = false,
                Verified = true,
                Tier = Tier.Basic,
                Reference = Guid.Empty,
                Home = new Location { City = "", Postcode = "" },
                Tags = ["vip"],
            },
        ];
}
