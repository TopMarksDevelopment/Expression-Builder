namespace Consumer.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public decimal Price { get; set; }

    public DateTime Released { get; set; }

    public static List<Product> All { get; } =
    [
        .. Enumerable
            .Range(1, 30)
            .Select(i => new Product
            {
                Id = i,
                Name =
                    (
                        i % 3 == 0 ? "Road bike "
                        : i % 3 == 1 ? "Helmet "
                        : "Bike lock "
                    ) + i,
                Price = 5m * i,
                Released = new DateTime(2024, 1, 1).AddDays(i * 11),
            }),
    ];
}
