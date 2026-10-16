namespace ExpressionBuilder.Tests.Models;

public class Brand
{
    public int Id { get; set; }

    public virtual int SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; } = null!;
}
