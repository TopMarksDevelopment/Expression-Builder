namespace ExpressionBuilder.Web.Sample.Models;

/// <summary>
/// Some products to search. A real app would use a database; the filter works on one just the same
/// (<c>db.Products.Where(filter)</c>).
/// </summary>
public class ProductCatalog
{
    static readonly string[] Names =
    [
        "Mountain bike",
        "Road bike",
        "Child's bike",
        "Folding bike",
        "Electric bike",
        "Cycling jersey",
        "Waterproof jacket",
        "Padded shorts",
        "Winter gloves",
        "Wool socks",
        "Brake pads",
        "Inner tube",
        "Chain",
        "Saddle",
        "Pedals",
        "Floor pump",
        "Multi-tool",
        "Chain lock",
        "Bike light",
        "Puncture kit",
    ];

    static readonly string[] Words =
    [
        "Pro",
        "Lite",
        "Classic",
        "Sport",
        "Trail",
    ];

    static readonly (string Name, string Country)[] Suppliers =
    [
        ("Pennine Cycles", "UK"),
        ("Velo Bretagne", "France"),
        ("Rhein Rad", "Germany"),
        ("Tienda Rueda", "Spain"),
    ];

    public IReadOnlyList<Product> Products { get; } = Create();

    static List<Product> Create()
    {
        var today = new DateTime(2026, 1, 1);
        var products = new List<Product>();

        for (var i = 0; i < Names.Length * 2; i++)
        {
            var name = Names[i % Names.Length];
            var supplier = Suppliers[i % Suppliers.Length];
            var price = 4.5m + (i * 13 % 47) * 12.25m;

            products.Add(
                new Product
                {
                    Id = i + 1,
                    Name =
                        i < Names.Length
                            ? name
                            : $"{name} {Words[i % Words.Length]}",
                    Description =
                        i % 4 == 0
                            ? null
                            : $"A dependable {name.ToLowerInvariant()} for everyday use.",
                    Price = price,
                    Cost = Math.Round(price * 0.6m, 2),
                    Stock = i * 7 % 40,
                    Released = today.AddDays(-(i * 37 % 900)),
                    LastOrdered =
                        i % 3 == 0 ? null : today.AddDays(-(i * 11 % 120)),
                    Discontinued = i % 9 == 0,
                    Rating = i % 5 == 0 ? null : 2.5 + (i * 3 % 6) * 0.5,
                    Category = i < 5 ? Category.Bikes : (Category)(i % 4),
                    Supplier =
                        i % 7 == 0
                            ? null
                            : new Supplier
                            {
                                Name = supplier.Name,
                                Country = supplier.Country,
                            },
                    Tags =
                        i % 2 == 0 ? ["popular", "new"]
                        : i % 3 == 0 ? ["clearance"]
                        : [],
                    Reviews =
                    [
                        .. Enumerable
                            .Range(0, i % 4)
                            .Select(r => new Review
                            {
                                Stars = 1 + (i + r) % 5,
                                Reviewer = r % 2 == 0 ? "Sam" : "Alex",
                                Comment = r == 0 ? null : "Does the job.",
                            }),
                    ],
                }
            );
        }

        return products;
    }
}
