namespace Consumer.Models;

using TopMarksDevelopment.ExpressionBuilder.Web;

public class ProductSearch
{
    public FilterBuilderModel Filter { get; set; } = new();

    public List<Product>? Results { get; set; }
}
