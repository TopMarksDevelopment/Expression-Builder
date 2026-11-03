namespace ExpressionBuilder.Web.Sample.Models;

using System.ComponentModel.DataAnnotations;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// The class the sample pages filter. It has a bit of everything the filter builder deals with: text,
/// numbers, dates, an enum, a yes/no, a related object and collections.
/// </summary>
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    [DataType(DataType.Date)]
    public DateTime Released { get; set; }

    public DateTime? LastOrdered { get; set; }

    public bool Discontinued { get; set; }

    public double? Rating { get; set; }

    public Category Category { get; set; }

    public Supplier? Supplier { get; set; }

    public List<string> Tags { get; set; } = [];

    public List<Review> Reviews { get; set; } = [];

    /// <summary>
    /// What we pay for it. Customers must not be able to search on this, even though it is never shown:
    /// a search for "cost over 10" would reveal it.
    /// </summary>
    [FilterBuilderIgnore]
    public decimal Cost { get; set; }
}

public enum Category
{
    Bikes,
    Clothing,

    [Display(Name = "Parts & accessories")]
    Parts,
    Tools,
}

public class Supplier
{
    public string Name { get; set; } = "";

    public string Country { get; set; } = "";
}

public class Review
{
    public int Stars { get; set; }

    public string Reviewer { get; set; } = "";

    public string? Comment { get; set; }
}
