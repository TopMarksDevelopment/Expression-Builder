namespace ExpressionBuilder.Benchmarks;

using Microsoft.EntityFrameworkCore;

// The classes for the report builder experiments (docs/architecture/report-builder.md): an order with a related customer
// (a join), an owned address (no join) and about thirty-five properties to choose columns from.

public enum OrderStatus
{
    New,
    Packed,
    Shipped,
    Delivered,
    Returned,
}

public class ReportCustomer
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public Region Region { get; set; }

    public string Country { get; set; } = "";

    public decimal CreditLimit { get; set; }

    public string Notes { get; set; } = "";
}

public class ReportAddress
{
    public string Line1 { get; set; } = "";

    public string? Line2 { get; set; }

    public string City { get; set; } = "";

    public string? Region { get; set; }

    public string Postcode { get; set; } = "";

    public string Country { get; set; } = "";
}

public class ReportOrder
{
    public int Id { get; set; }

    public string Reference { get; set; } = "";

    public DateTime Placed { get; set; }

    public DateTime? Shipped { get; set; }

    public decimal Total { get; set; }

    public decimal Tax { get; set; }

    public decimal Discount { get; set; }

    public double Weight { get; set; }

    public OrderStatus Status { get; set; }

    public int Priority { get; set; }

    public bool Gift { get; set; }

    public string Channel { get; set; } = "";

    public string Currency { get; set; } = "";

    public string Carrier { get; set; } = "";

    public string? Tracking { get; set; }

    public string Department { get; set; } = "";

    public string CostCentre { get; set; } = "";

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public string CreatedBy { get; set; } = "";

    public DateTime? UpdatedOn { get; set; }

    public string Notes { get; set; } = "";

    public int CustomerId { get; set; }

    public ReportCustomer Customer { get; set; } = null!;

    public ReportAddress ShipTo { get; set; } = new();
}

/// <summary>What an app that registers its own result would write for six columns.</summary>
public sealed class OrderRow
{
    public string Reference { get; set; } = "";

    public DateTime Placed { get; set; }

    public decimal Total { get; set; }

    public OrderStatus Status { get; set; }

    public string? CustomerName { get; set; }

    public string? ShipToCity { get; set; }
}

public sealed class ReportDb(DbContextOptions<ReportDb> options)
    : DbContext(options)
{
    public DbSet<ReportOrder> Orders => Set<ReportOrder>();

    public DbSet<ReportCustomer> Customers => Set<ReportCustomer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<ReportOrder>().OwnsOne(o => o.ShipTo);
}
