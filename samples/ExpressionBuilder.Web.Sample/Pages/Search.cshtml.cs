namespace ExpressionBuilder.Web.Sample.Pages;

using ExpressionBuilder.Web.Sample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// The filter builder in a Razor Page: the same two steps as the MVC controller.
/// </summary>
public class SearchModel(
    ProductCatalog catalog,
    IOptions<FilterBuilderOptions> options
) : PageModel
{
    [BindProperty]
    public ProductSearch Search { get; set; } = new();

    public void OnGet(bool preset = false)
    {
        if (preset)
            Search.Filter = ProductSearch.Preset(options.Value);
    }

    public void OnPost()
    {
        var result = Search.Filter.Build<Product>(options.Value);

        if (!result.Succeeded)
        {
            ModelState.AddFilterErrors(
                $"{nameof(Search)}.{nameof(Search.Filter)}",
                result
            );

            return;
        }

        Search.Results = [.. catalog.Products.Where(result.Filter!)];
        Search.Description = result.Filter!.ToString();
        Search.Searched = true;
    }
}
