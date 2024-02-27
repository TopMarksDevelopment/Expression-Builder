namespace ExpressionBuilder.Tests.Models;

public class StockLocation : IItemable
{
    public int Id { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Quantity {get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}