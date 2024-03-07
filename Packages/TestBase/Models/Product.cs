namespace ExpressionBuilder.Tests.Models;

using System;
using System.Collections.Generic;

public class Product : IItemable
{
    private Product()
    {
        Categories = [];
        StockLocations = [];
    }

    public Product(
        int id,
        string? searchData = null,
        DateTime? createdAt = null
    )
        : base()
    {
        Id = id;
        Name = $"Product {id}";
        OtherSearchField = searchData;
        CreatedAt = createdAt;

        Categories = [];
        StockLocations =
        [
            new()
            {
                Id = id,
                Location = $"Location {id}",
                Quantity = id + 10,
                Product = this,
                CreatedAt = createdAt ?? ResultMatchSeed.CreatedDate
            }
        ];
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OtherSearchField { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual int? BrandId { get; set; }
    public virtual Brand? Brand { get; set; }

    public virtual Category? Category { get; set; }
    public virtual ICollection<Category> Categories { get; internal set; }
    public virtual ICollection<StockLocation> StockLocations
    {
        get;
        internal set;
    }
}
