namespace Consumer.Controllers;

using Consumer.Models;
using Microsoft.AspNetCore.Mvc;
using TopMarksDevelopment.ExpressionBuilder.Web;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new ProductSearch());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ProductSearch search)
    {
        var result = search.Filter.Build<Product>();

        if (!result.Succeeded)
        {
            ModelState.AddFilterErrors(nameof(search.Filter), result);

            return View(search);
        }

        search.Results = [.. Product.All.Where(result.Filter!)];

        return View(search);
    }
}
