namespace ExpressionBuilder.Web.Sample.Controllers;

using ExpressionBuilder.Web.Sample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// The filter builder in an MVC controller: show the form, then check and apply what it posts.
/// </summary>
public class ProductsController(
    ProductCatalog catalog,
    IOptions<FilterBuilderOptions> options
) : Controller
{
    [HttpGet]
    public IActionResult Index(bool preset = false) =>
        View(
            new ProductSearch
            {
                Filter = preset ? ProductSearch.Preset(options.Value) : new(),
            }
        );

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ProductSearch search)
    {
        // Turn what was posted into a filter. It is checked against Product on the way: a property that
        // isn't offered, a condition that doesn't suit it or a value that doesn't parse is refused.
        var result = search.Filter.Build<Product>(options.Value);

        if (!result.Succeeded)
        {
            ModelState.AddFilterErrors(nameof(search.Filter), result);

            return View(search);
        }

        // From here it is an ordinary filter: use it on a list, or on an EF Core query
        search.Results = [.. catalog.Products.Where(result.Filter!)];
        search.Description = result.Filter!.ToString();
        search.Searched = true;

        return View(search);
    }
}
