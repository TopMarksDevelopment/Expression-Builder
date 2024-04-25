namespace ExpressionBuilder.Tests;
using System.Linq;
using ExpressionBuilder.Tests.Models;

public class ResultMatchSeed
{
    static DateTime? _createdDate;
    public static DateTime CreatedDate
    {
        get => _createdDate ??= DateTime.Now;
        set => _createdDate = value;
    }

    public ICollection<T> GetCollection<T>()
        where T : class =>
        typeof(T) == typeof(Product)
            ? (ICollection<T>)Products
            : typeof(T) == typeof(StockLocation)
                ? (ICollection<T>)StockLocations
                : (ICollection<T>)Categories;

    public HashSet<Product> Products { get; set; }
    public HashSet<StockLocation> StockLocations { get; set; }
    public HashSet<Category> Categories { get; set; }

    public ResultMatchSeed()
    {
        Products = ProductsBase;

        StockLocations = Products.SelectMany(x => x.StockLocations).ToHashSet();

        Categories = CategoriesBase(Products);

        foreach (var category in Categories)
        foreach (var product in category.Products)
        {
            product.Category ??= category;

            product.Categories.Add(category);

            if (product.Id == 1)
                product.Category = Categories.FirstOrDefault(x => x.Id == 2);
        }
    }

    public static List<Product> GetProds(
        int skip,
        IEnumerable<Product> source
    ) =>
        source.Skip(skip).Take(3).Append(source.First(x => x.Id == 1)).ToList();

    public static HashSet<Category> CategoriesBase(
        IEnumerable<Product>? source
    ) =>
        [
            new Category
            {
                Id = 1,
                Name = "Category 1",
                Products =
                    source == null
                        ? new HashSet<Product>()
                        : GetProds(1, source)
            },
            new Category
            {
                Id = 2,
                Name = "Category 2",
                Products =
                    source == null
                        ? new HashSet<Product>()
                        : GetProds(3, source)
            },
            new Category
            {
                Id = 3,
                Name = "Category 3",
                Products =
                    source == null
                        ? new HashSet<Product>()
                        : GetProds(6, source)
            },
        ];

    public static HashSet<Product> ProductsBase =>
        [
            new(1),
            new(2, null, CreatedDate),
            new(3, "Word1 Word2 Word3"),
            new(4, "1Word 2Word 3Word", null),
            new(5),
            new(6, "Word", CreatedDate),
            new(7),
            new(8),
            new(9, "Word123", CreatedDate),
        ];
}
