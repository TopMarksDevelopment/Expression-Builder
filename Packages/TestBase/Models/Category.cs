namespace ExpressionBuilder.Tests.Models;

using System.Collections.Generic;

public class Category : IItemable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<Product> Products { get; set; } = [];
}
