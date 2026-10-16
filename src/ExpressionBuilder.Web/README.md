# Expression Builder - Web filter builder

A ready-made filter editor for ASP.NET Core (MVC **and** Razor Pages), built on
[Expression Builder](https://github.com/TopMarksDevelopment/Expression-Builder).

Your users pick a **property** (or a **collection**), a **condition**, a **value** (if one is needed) and how it
**connects** to the next condition. Groups and the advanced options (match any/all, adjusting text) are a click
away. You post the form to a page, turn what arrives into a `Filter<T>` and apply it to a list or a database query.

```csharp
var result = search.Filter.Build<Product>();           // checks what was posted

var products = db.Products.Where(result.Filter!);      // an ordinary Expression Builder filter
```

## Set up

> dotnet add package TopMarksDevelopment.ExpressionBuilder.Web

**1.** Add the tag helper to `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, TopMarksDevelopment.ExpressionBuilder.Web
```

**2.** Put a `FilterBuilderModel` on your model:

```csharp
public class ProductSearch
{
    public FilterBuilderModel Filter { get; set; } = new();
}
```

**3.** Add the builder to a form (it brings its own script and stylesheet, once per page):

```cshtml
<form method="post">
    <filter-builder for="Filter" type="typeof(Product)" />
    <button type="submit">Search</button>
</form>
```

**4.** In the handler (MVC or Razor Page), build the filter and use it:

```csharp
[HttpPost]
public IActionResult Index(ProductSearch search)
{
    var result = search.Filter.Build<Product>();

    if (!result.Succeeded)
    {
        ModelState.AddFilterErrors(nameof(search.Filter), result);
        return View(search);
    }

    search.Results = db.Products.Where(result.Filter!).ToList();
    return View(search);
}
```

That is all that is needed. The `samples/ExpressionBuilder.Web.Sample` app has a working MVC page and Razor Page.

## Things you will want to know

- **A posted filter is untrusted input.** Only properties the builder offers can be used, each operation must suit
  its property, values are parsed as the property's type, and the size of a filter is limited. Keep
  properties that must not be searchable out with `[FilterBuilderIgnore]` (searching on a hidden value reveals it).
- **Show a saved filter** with `FilterBuilderModel.FromFilter(filter)`.
- **Change the text, limits or labels** with `services.AddFilterBuilder(options => ...)`.
- **Change the look** by overriding the CSS variables, or copy `Views/Shared/Components/FilterBuilder/Default.cshtml`
  into your app and edit it. The script adds no classes of its own - everything it makes is cloned from a `<template>` in
  that view - so it can match Bootstrap or your own form styles, as long as the `data-eb-*` attributes stay.
- **Add your own operation or manipulator** with `OperationCatalog.Register<T>(...)` / `ManipulatorCatalog.Register<T>(...)`
  (or `options.AddOperation` / `AddManipulator` for one builder). They are listed apart from the built-in ones, a browser can
  only choose them by name, and `options.ExcludeOperations(...)` / `ExcludeManipulators(...)` leave built-in ones out.

The full guide is in
[docs/web-filter-builder.md](https://github.com/TopMarksDevelopment/Expression-Builder/blob/main/docs/web-filter-builder.md).
