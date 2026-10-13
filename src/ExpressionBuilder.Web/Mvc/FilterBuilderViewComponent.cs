namespace TopMarksDevelopment.ExpressionBuilder.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

/// <summary>
/// Renders the filter builder. Use it from a view (<c>&lt;vc:filter-builder .../&gt;</c> or
/// <c>Component.InvokeAsync("FilterBuilder", ...)</c>), or through the <c>&lt;filter-builder&gt;</c>
/// tag helper, which is easier.
/// </summary>
/// <param name="defaults">The settings registered with <c>AddFilterBuilder</c></param>
public class FilterBuilderViewComponent(IOptions<FilterBuilderOptions> defaults)
    : ViewComponent
{
    /// <summary>
    /// Renders the filter builder for a class.
    /// </summary>
    /// <param name="name">The name of the hidden input that is posted, such as <c>Filter.Json</c></param>
    /// <param name="modelType">The class being filtered</param>
    /// <param name="json">The filter to start with</param>
    /// <param name="options">Settings for this one; the registered settings if omitted</param>
    /// <param name="id">The id of the element; made up if omitted</param>
    public IViewComponentResult Invoke(
        string name,
        Type modelType,
        string? json = null,
        FilterBuilderOptions? options = null,
        string? id = null
    )
    {
        options ??= defaults.Value;

        return View(
            new FilterBuilderViewModel
            {
                Id = id ?? "eb-" + Guid.NewGuid().ToString("N")[..8],
                Name = name,
                Json = json,
                SchemaJson = FilterSchema.For(modelType, options).ToJson(),
                Labels = options.Labels,
            }
        );
    }
}
