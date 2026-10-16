namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

/// <summary>
/// <c>&lt;filter-builder for="Filter" type="typeof(Product)" /&gt;</c>: the filter builder for a
/// <see cref="FilterBuilderModel"/> on your model.
/// </summary>
/// <remarks>
/// Add it to your views with <c>@addTagHelper *, TopMarksDevelopment.ExpressionBuilder.Web</c> in
/// <c>_ViewImports.cshtml</c>. Place it inside a <c>&lt;form&gt;</c>; the filter is posted with the
/// rest of the form.
/// </remarks>
[HtmlTargetElement(
    "filter-builder",
    TagStructure = TagStructure.NormalOrSelfClosing
)]
public class FilterBuilderTagHelper(IViewComponentHelper viewComponents)
    : TagHelper
{
    const string AssetsRendered =
        "TopMarksDevelopment.ExpressionBuilder.Web.Assets";

    /// <summary>
    /// The <see cref="FilterBuilderModel"/> property to edit, such as <c>Filter</c> or
    /// <c>Search.Filter</c>.
    /// </summary>
    [HtmlAttributeName("for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// The class being filtered, such as <c>typeof(Product)</c>.
    /// </summary>
    [HtmlAttributeName("type")]
    public Type? ModelType { get; set; }

    /// <summary>
    /// Settings for this filter builder. The ones registered with <c>AddFilterBuilder</c> if omitted.
    /// </summary>
    [HtmlAttributeName("options")]
    public FilterBuilderOptions? Options { get; set; }

    /// <summary>
    /// Whether to add the stylesheet and script (once per page). Turn it off if you bundle them
    /// yourself.
    /// </summary>
    [HtmlAttributeName("include-assets")]
    public bool IncludeAssets { get; set; } = true;

    /// <summary>
    /// The context of the view being rendered.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    /// <inheritdoc />
    public override async Task ProcessAsync(
        TagHelperContext context,
        TagHelperOutput output
    )
    {
        if (For == null || ModelType == null)
            throw new InvalidOperationException(
                "<filter-builder> needs 'for' (the FilterBuilderModel property) and 'type' (the class being filtered)."
            );

        ((IViewContextAware)viewComponents).Contextualize(ViewContext);

        var name = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(
            For.Name + "." + nameof(FilterBuilderModel.Json)
        );

        var content = await viewComponents.InvokeAsync(
            typeof(FilterBuilderViewComponent),
            new
            {
                name,
                modelType = ModelType,
                json = (For.Model as FilterBuilderModel)?.Json,
                options = Options,
                id = output.Attributes.TryGetAttribute("id", out var id)
                    ? id.Value?.ToString()
                    : null,
            }
        );

        output.TagName = null;
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.Clear();

        if (
            IncludeAssets
            && ViewContext.HttpContext.Items.TryAdd(AssetsRendered, true)
        )
            output.Content.AppendHtml(Assets());

        output.Content.AppendHtml(content);
    }

    IHtmlContent Assets()
    {
        // Cache busting: a new version of the package means new files
        var version = typeof(FilterBuilderTagHelper)
            .Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion.Split('+')[0];
        var basePath = "~/_content/TopMarksDevelopment.ExpressionBuilder.Web/";
        var url = new Microsoft.AspNetCore.Mvc.Routing.UrlHelper(ViewContext);

        return new HtmlString(
            $"<link rel=\"stylesheet\" href=\"{url.Content(basePath + "css/filter-builder.css")}?v={version}\" />"
                + $"<script type=\"module\" src=\"{url.Content(basePath + "js/filter-builder.js")}?v={version}\"></script>"
        );
    }
}
