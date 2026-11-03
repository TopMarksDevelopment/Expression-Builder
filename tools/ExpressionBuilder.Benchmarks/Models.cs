namespace ExpressionBuilder.Benchmarks;

using System.ComponentModel.DataAnnotations;

public enum Tier
{
    Basic,
    Silver,
    Gold,
    Platinum,
}

public enum Region
{
    North,
    South,
    East,
    West,
}

public enum Status
{
    New,
    Open,
    Closed,
}

/// <summary>A class with a handful of properties: the schema of a typical search form.</summary>
public class Small
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Email { get; set; }

    public DateTime Created { get; set; }

    public decimal Balance { get; set; }

    public bool Active { get; set; }

    public Tier Tier { get; set; }
}

public class Address
{
    public string Line1 { get; set; } = "";

    public string? Line2 { get; set; }

    public string City { get; set; } = "";

    public string? Region { get; set; }

    public string Postcode { get; set; } = "";

    public string Country { get; set; } = "";

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool Verified { get; set; }

    public DateTime? VerifiedOn { get; set; }
}

public class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Website { get; set; }

    public int Employees { get; set; }

    public decimal Turnover { get; set; }

    public DateOnly Founded { get; set; }

    public Tier Tier { get; set; }

    public Address HeadOffice { get; set; } = new();
}

public class OrderLine
{
    public int Id { get; set; }

    public string Sku { get; set; } = "";

    public string? Note { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public bool Backordered { get; set; }

    public Status Status { get; set; }
}

public class Order
{
    public int Id { get; set; }

    public string Reference { get; set; } = "";

    public DateTime Placed { get; set; }

    public DateTime? Shipped { get; set; }

    public decimal Total { get; set; }

    public decimal Tax { get; set; }

    public string? Notes { get; set; }

    public Status Status { get; set; }

    public bool Gift { get; set; }

    public Address ShipTo { get; set; } = new();

    public List<OrderLine> Lines { get; set; } = [];
}

public class Contact
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string Role { get; set; } = "";

    public bool Primary { get; set; }

    public DateTime? LastContacted { get; set; }
}

/// <summary>
/// A class big enough to be a real report row: dozens of properties of every kind, related objects, and collections
/// (one inside another). With the default settings the schema has well over a hundred fields.
/// </summary>
public class Large
{
    public int Id { get; set; }

    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Title { get; set; }

    public string? Department { get; set; }

    public string? Manager { get; set; }

    public string? CostCentre { get; set; }

    public string? Notes { get; set; }

    public string? Source { get; set; }

    [Display(Name = "Date of birth")]
    [DataType(DataType.Date)]
    public DateTime Born { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public DateTime? LastSeen { get; set; }

    public DateOnly Joined { get; set; }

    public DateOnly? Left { get; set; }

    public TimeOnly StartsAt { get; set; }

    public TimeSpan Shift { get; set; }

    public int Age { get; set; }

    public int? Rank { get; set; }

    public long Points { get; set; }

    public long? Credits { get; set; }

    public short Level { get; set; }

    public byte Grade { get; set; }

    public decimal Salary { get; set; }

    public decimal? Bonus { get; set; }

    public decimal Balance { get; set; }

    public decimal Limit { get; set; }

    public double Score { get; set; }

    public double? Rating { get; set; }

    public float Ratio { get; set; }

    public bool Active { get; set; }

    public bool? Verified { get; set; }

    public bool Newsletter { get; set; }

    public bool Staff { get; set; }

    public Tier Tier { get; set; }

    public Region Region { get; set; }

    public Status? Status { get; set; }

    public Guid Reference { get; set; }

    public Guid? Parent { get; set; }

    public Address Home { get; set; } = new();

    public Address? Work { get; set; }

    public Address? Billing { get; set; }

    public Company? Employer { get; set; }

    public List<Order> Orders { get; set; } = [];

    public List<Contact> Contacts { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    public List<int> Scores { get; set; } = [];
}
