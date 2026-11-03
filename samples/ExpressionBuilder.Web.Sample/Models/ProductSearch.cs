namespace ExpressionBuilder.Web.Sample.Models;

using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// The model of both sample pages (the MVC one and the Razor Page): the posted filter, and what to show.
/// </summary>
public class ProductSearch
{
    /// <summary>
    /// The filter builder posts into this. Nothing else is needed on the model.
    /// </summary>
    public FilterBuilderModel Filter { get; set; } = new();

    public IReadOnlyList<Product> Results { get; set; } = [];

    /// <summary>
    /// The filter in words, as the library writes it.
    /// </summary>
    public string? Description { get; set; }

    public bool Searched { get; set; }

    /// <summary>
    /// A filter built in code, shown in the builder by <see cref="FilterBuilderModel.FromFilter{TClass}"/>.
    /// It is how a saved search would be loaded.
    /// </summary>
    public static FilterBuilderModel Preset(FilterBuilderOptions options)
    {
        var filter = new Filter<Product>();

        filter
            .Equal(p => p.Category, Category.Bikes)
            .And()
            .GreaterThan(p => p.Price, 500m)
            .And()
            .OpenGroup()
            .IsNotNull(p => p.Rating)
            .Or()
            .OpenCollection(p => p.Reviews)
            .GreaterThanOrEqual(r => r.Stars, 4)
            .CloseCollection<Product>()
            .CloseGroup();

        return FilterBuilderModel.FromFilter(filter, options);
    }
}
