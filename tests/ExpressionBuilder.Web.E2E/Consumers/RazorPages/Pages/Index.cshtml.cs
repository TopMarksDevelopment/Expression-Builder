namespace Consumer.Pages;

using Consumer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TopMarksDevelopment.ExpressionBuilder.Web;

public class IndexModel : PageModel
{
    [BindProperty]
    public ProductSearch Search { get; set; } = new();

    public void OnGet() { }

    public void OnPost()
    {
        var result = Search.Filter.Build<Product>();

        if (!result.Succeeded)
        {
            ModelState.AddFilterErrors(
                nameof(Search) + "." + nameof(Search.Filter),
                result
            );

            return;
        }

        Search.Results = [.. Product.All.Where(result.Filter!)];
    }
}
