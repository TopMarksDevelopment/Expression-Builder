# The web filter builder

`TopMarksDevelopment.ExpressionBuilder.Web` gives your ASP.NET Core app (MVC or Razor Pages) a filter editor you can
post to a page. This guide covers how it fits together, how to set it up, and how to change it.

- [How it works](#how-it-works)
- [Set up](#set-up)
- [What users can do](#what-users-can-do)
- [What can be filtered](#what-can-be-filtered)
- [Showing a saved filter](#showing-a-saved-filter)
- [Settings and text](#settings-and-text)
- [Custom operations and manipulators](#custom-operations-and-manipulators)
- [Changing the markup and the look](#changing-the-markup-and-the-look)
- [Working with the script](#working-with-the-script)
- [The posted format](#the-posted-format)
- [Security](#security)
- [Working on the filter builder itself](#working-on-the-filter-builder-itself)

## How it works

```text
 server                                   browser
 ──────                                   ───────
 FilterSchema.For<Product>()  ──────────▶  <script data-eb-schema>   what can be filtered
 Razor view (Default.cshtml)  ──────────▶  <template>s + hidden input  the markup, and the text
                                            filter-builder.js         builds the UI from both
                                            ◀── user edits, hidden input = JSON ──
 FilterBuilderModel.Build<Product>()  ◀──  form post
   checks the JSON against the schema
   ▼
 Filter<Product>  ──▶  list.Where(filter)  /  db.Products.Where(filter)
```

- The **server** works out, by reflection, what of your class can be filtered (the *schema*) and sends it to the page.
  The same schema is used again to check what comes back, so the two sides always agree.
- The **view** is plain Razor. It holds the markup of a group, a condition and a connector as `<template>`s, and all of
  the text, so you can restyle or translate it without touching the script.
- The **script** (TypeScript, shipped as one ES module) builds the interface from the schema and the templates and keeps
  a hidden input up to date with the filter as JSON. Anything that posts the form (a button, `requestSubmit()`,
  `FormData`) posts the filter.
- The posted JSON is **not** the library's own serialised filter: that names .NET types, which a browser must never
  get to choose. It is plain text (property names, operation names, values) that is validated and then turned into a
  `Filter<T>`. See [the posted format](#the-posted-format).

## Set up

Install the package:

> dotnet add package TopMarksDevelopment.ExpressionBuilder.Web

It references the main package, so every built-in operation is available.

### 1. Add the tag helper

In `_ViewImports.cshtml` (for MVC and for Razor Pages):

```cshtml
@addTagHelper *, TopMarksDevelopment.ExpressionBuilder.Web
```

### 2. Add a `FilterBuilderModel` to your model

```csharp
public class ProductSearch
{
    public FilterBuilderModel Filter { get; set; } = new();
    // ...and whatever else your page needs (results, paging, other search boxes)
}
```

### 3. Add the builder to a form

```cshtml
<form method="post">
    <filter-builder for="Filter" type="typeof(Product)" />
    <button type="submit">Search</button>
</form>
```

| Attribute        | Meaning                                                                                       |
| ---------------- | --------------------------------------------------------------------------------------------- |
| `for`            | The `FilterBuilderModel` property to edit. Nested names work (`Search.Filter`).               |
| `type`           | The class being filtered.                                                                     |
| `options`        | Settings for this one builder, instead of the registered ones.                                |
| `include-assets` | `false` if you bundle `filter-builder.js` and `.css` yourself. Default: added once per page.  |

Prefer view components? `<vc:filter-builder name="Filter.Json" model-type="typeof(Product)"></vc:filter-builder>` or
`Component.InvokeAsync("FilterBuilder", new { name = "Filter.Json", modelType = typeof(Product) })` render the same thing
(you give the posted name yourself, and optionally `json`, `options` and `id`). Unlike the tag helper they do not add the
script and stylesheet, so include `_content/TopMarksDevelopment.ExpressionBuilder.Web/js/filter-builder.js` (as
`type="module"`) and `.../css/filter-builder.css` yourself.

### 4. Check and apply what is posted

MVC:

```csharp
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

    search.Results = db.Products.Where(result.Filter!).ToList();
    return View(search);
}
```

Razor Pages: the same two calls in `OnPost`. If the model is a property of the page model, render its partial with
`<partial name="_Search" for="Search" />` so the posted name is `Search.Filter.Json`.

`Build` returns a `FilterBuildResult<T>`: `Succeeded`, `Filter`, and `Errors` (each with a `Path` such as
`root.items[2].values[0]` and a message written for the user). `GetFilterOrThrow()` is there for code that has already
validated.

Optional: register settings once, and inject the same ones where you build, so the page and the check agree.

```csharp
builder.Services.AddFilterBuilder(options => options.MaxConditions = 30);

// in the controller / page model
public ProductsController(IOptions<FilterBuilderOptions> options) { ... }
var result = search.Filter.Build<Product>(options.Value);
```

## What users can do

- **Pick a property** (including properties of related objects, shown as `Home address › Postcode`), or **a collection**.
  Picking a collection turns the row into a group that filters the *items* of that collection ("Any Reviews where…").
- **Pick a condition** that suits the property: text offers *contains*, *starts with*, *is blank*…; numbers and dates
  offer *greater than*, *between*…; booleans and enums offer *equals*; anything that can be missing offers *is set*.
- **Enter a value** with the right input: text, number, date, date and time, time, yes/no, or a list of an enum's members.
  *Between* has two boxes; *is one of* and the text operations take a list of values.
- **Choose how it connects** to the next condition: *And* or *Or*, between every two items.
- **Group** conditions in brackets ("Add group"), as deeply as the settings allow.
- **Advanced options**, behind the gear on a condition:
  - *When there are several values, match any/all of them* (where that makes sense for the operation);
  - *Adjust the text before comparing*: trim spaces, lower case, upper case, replace text. Your own adjustments are listed in a group of their own
    ([custom operations and manipulators](#custom-operations-and-manipulators)).
- A live sentence under the builder says what the filter means. An incomplete condition is highlighted, and the form is
  not posted until it is complete (add `data-eb-novalidate` to the builder's element to leave that to the server).

## What can be filtered

By default every public readable property of your class can, plus the properties of related objects (to
`MaxNavigationDepth`, default 2 names, as in `Birth.Country`) and collections:

| .NET type                                          | Kind     | Input            |
| -------------------------------------------------- | -------- | ---------------- |
| `string`                                           | text     | text             |
| `byte`…`ulong`, `float`, `double`, `decimal`       | number   | number           |
| `DateOnly`, `DateTime` marked `[DataType(Date)]`   | date     | date             |
| `DateTime`, `DateTimeOffset`                       | date and time | datetime-local |
| `TimeOnly`, `TimeSpan`                             | time     | time             |
| `bool`                                             | yes/no   | select           |
| `enum` (display names from `[Display]`)            | enum     | select           |
| `Guid`                                             | guid     | text             |
| another class                                      | object   | *is set / not set* only |
| `IEnumerable<T>`                                   | collection | its own group  |

`byte[]`, dictionaries, delegates and `Stream`s are left out. Nullable types behave as their underlying type, and gain
*is set / not set*.

**Keep sensitive properties out.** Being able to filter on a value reveals it ("Cost over 10", "Token starts with a"),
even if it is never displayed:

```csharp
public class Product
{
    [FilterBuilderIgnore]
    public decimal Cost { get; set; }
}

// or, for classes you don't own
services.AddFilterBuilder(o => o.Exclude<Product>(p => p.Cost, p => p.SupplierNotes));
```

A property that is ignored is not offered, and a post that uses it is refused.

## Showing a saved filter

```csharp
var filter = new Filter<Product>();
filter.Equal(p => p.Category, Category.Bikes).And().GreaterThan(p => p.Price, 500m);

search.Filter = FilterBuilderModel.FromFilter(filter);   // the builder opens with these conditions
```

`FromFilter` throws `NotSupportedException` rather than show a filter that means something different: for example one that
uses a property that is ignored, or a manipulator the builder does not know.

## Settings and text

`FilterBuilderOptions`:

| Setting                 | Default | Meaning                                                           |
| ----------------------- | ------- | ----------------------------------------------------------------- |
| `MaxGroupDepth`         | 5       | How deeply groups nest (the top level counts as 1).               |
| `MaxConditions`         | 100     | Conditions in a whole filter.                                     |
| `MaxValuesPerCondition` | 50      | Values in one condition.                                          |
| `MaxValueLength`        | 500     | Characters in one value.                                          |
| `MaxPayloadLength`      | 65536   | Characters of posted JSON that are read.                          |
| `MaxNavigationDepth`    | 2       | Names in a property path (`Name` = 1, `Birth.Country` = 2).       |
| `Labels`                | English | All the text of the builder (`FilterBuilderLabels`).              |
| `OperationLabels`       | -       | Replace an operation's text: `["Equal"] = "Is"`.                  |
| `ManipulatorLabels`     | -       | Replace an adjustment's text: `["Trim"] = "Remove spaces"`.        |
| `MaxManipulatorsPerCondition` | 8 | Adjustments on one condition.                                     |

Because the labels are in the options, the same text is used by the Razor view and by the script - translate them once.

## Custom operations and manipulators

You can add operations and manipulators ("adjust the text before comparing") of your own to fill gaps in the built-in ones.
They are ordinary Expression Builder extensions - an `IOperation` and an `IEntityManipulator` - so they work in code and
in saved filters as they always have. What this section adds is the part that is specific to the browser:

- **You opt in.** Nothing is offered just because an assembly is installed. An operation or manipulator reaches the
  builder only when your app _describes_ it (below). That description is also what a post is checked against, so a
  browser can only ever choose one of them **by name**; it can never name a type or a method.
- **They are listed apart.** Built-in ones are never mixed with yours: custom operations appear in the list under their
  own heading (`Custom` by default, or the group you name), and custom manipulators in their own group in the options.
  The headings only appear when you have added something.
- **They can't impersonate a built-in one.** Describing something with a built-in name (`Equal`, `Trim`…) throws. To
  change a built-in's wording use `OperationLabels` / `ManipulatorLabels`; to hide it use `ExcludeOperations` /
  `ExcludeManipulators`.

### An operation

```csharp
public class IsWeekend : IOperation { /* Name => "IsWeekend", Build(...) */ }

var descriptor = new OperationDescriptor(
    Name: "IsWeekend",
    Label: "Is at the weekend",
    Arity: ValueArity.None,                                   // None, One, Two or Many values
    Kinds: new HashSet<FieldKind> { FieldKind.Date, FieldKind.DateTime },
    Group: "Dates");                                          // optional heading; "Custom" if left out

// For every builder in the app...
OperationCatalog.Register<IsWeekend>(descriptor);

// ...or for one builder only
services.AddFilterBuilder(options => options.AddOperation<IsWeekend>(descriptor));
```

`Register<T>` also registers the operation with `OperationRegistry`, which is what lets a _saved_ filter find it by name;
use the non-generic `Register(descriptor)` if you register it yourself (for example with `RegisterAssembly`). An operation
that is described but cannot be found by the registry is not offered. An operation can only use the value shapes and
property kinds above; for anything else, build the filter in code.

### A manipulator

A manipulator class implements `IEntityManipulator` (`Name`, `Arguments`, `ManipulateMember`, `ManipulateValue`,
`Validate`). The text the user types arrives in `Arguments`, one string for each parameter you declare:

```csharp
// Parameters are what the user is asked for. Required defaults to true.
ManipulatorCatalog.Register<FirstLetters>(
    label: "First letters",
    parameters: [new ManipulatorParameter("How many")],
    group: "Text tools");

// ...or for one builder only
services.AddFilterBuilder(options => options.AddManipulator(
    ManipulatorDescriptor.For<FirstLetters>("First letters", [new ManipulatorParameter("How many")])));
```

Check the text in `Validate()` and throw an `ArgumentException` with a message fit for the user ("Give a whole number of
letters."): it is shown beside the condition. For full control, build the descriptor yourself:
`new ManipulatorDescriptor(name, label, create: text => …, parameters, kinds, group, read)` where `create` makes the
manipulator from the text and `read` turns a saved filter's manipulator back into text (so `FromFilter` can show it).
`kinds` says which kinds of property it suits (text only if left out).

A manipulator runs inside the expression, so if you filter a database it must be something your provider can translate
(`ToLower()` is; a method of your own is not, unless you map it).

### Leaving things out of one builder

```csharp
services.AddFilterBuilder(options => options
    .ExcludeOperations("SmartSearch", "Contains")
    .ExcludeManipulators("Replace"));
```

A post that uses something a builder leaves out is refused, not ignored.

## Changing the markup and the look

**Look.** Everything is scoped to `.eb` and themed with CSS custom properties; override them:

```css
.eb { --eb-accent: #0a7; --eb-radius: 2px; }
```

It follows `prefers-color-scheme`; set `data-theme="light"` or `"dark"` on `<html>` to force one. To use your own
framework's variables, point the properties at them: `.eb { --eb-border: var(--bs-border-color); --eb-accent: var(--bs-primary); }`.

**Markup.** Copy `Views/Shared/Components/FilterBuilder/Default.cshtml` into your app at the same path (Razor Pages:
`Pages/Shared/Components/FilterBuilder/Default.cshtml`). Your copy is used instead. Change classes, wrapping elements
and text as you like, but keep:

- the `data-eb-*` attributes - they are how the script finds each part;
- the `<template data-eb-template="…">` elements (below);
- the hidden input (`data-eb-input`) and the schema (`script[data-eb-schema]`).

**The script adds no classes of its own.** Everything it creates is cloned from one of the templates, so the classes (and
the surrounding elements) are yours to choose. The one exception is marking an incomplete condition: it gets the classes
named in `data-eb-invalid-class` on the builder's element (`eb-invalid` if you leave the attribute out), so for Bootstrap use
`data-eb-invalid-class="is-invalid"`.

| Template            | Made for                                       | Parts the script looks for (all `data-eb-…`)                                                                                                 |
| ------------------- | ---------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| `group`             | The top level, a bracketed group, a collection | `title`, `remove-group`, `items`, `empty`, `add-condition`, `add-group`                                                                      |
| `item`              | A place in a group's list (a `li`)             | the element itself                                                                                                                           |
| `condition`         | One condition                                  | `field`, `operation`, `values`, `add-value`, `toggle-options`, `remove`, `options`, `match`, `match-wrap`, `manipulators-wrap`, `manipulators`, `error` |
| `value`             | One value of a condition                       | `control="input"` **and** `control="select"` (the unused one is removed), `remove-value`; optional `value-joiner`, `value-label`            |
| `connector`         | What joins an item to the next (it goes inside the item's `li`) | `connector` (a select with `And` and `Or`)                                                                                                   |
| `manipulator-group` | A group of adjustments                         | `manipulator-group-title`, `manipulator-group-items`                                                                                         |
| `manipulator`       | One adjustment                                 | `manipulator` (the checkbox), `manipulator-label`, `manipulator-args`                                                                        |
| `manipulator-arg`   | One piece of text an adjustment asks for       | the element itself (an `input`)                                                                                                              |

If a part is missing the script says which one, when the page loads, so a customised view fails loudly rather than quietly.

**Example: Bootstrap.** Only the classes change:

```cshtml
<template data-eb-template="value">
    <span class="input-group" data-eb-value>
        <input class="form-control" data-eb-control="input" />
        <select class="form-select" data-eb-control="select"></select>
        <span class="input-group-text" data-eb-value-joiner hidden></span>
        <button type="button" class="btn btn-outline-danger" data-eb-remove-value aria-label="@l.RemoveValue" hidden>&minus;</button>
    </span>
</template>
```

**Example: a floating label** (the box comes before its label, inside a form group). `data-eb-value-label` receives the
text the box is otherwise labelled with for screen readers:

```cshtml
<template data-eb-template="value">
    <span class="FormGroup" data-eb-value>
        <input class="FormControl" placeholder=" " data-eb-control="input" />
        <select class="FormControl" data-eb-control="select"></select>
        <label data-eb-value-label></label>
        <span data-eb-value-joiner hidden></span>
        <button type="button" data-eb-remove-value aria-label="@l.RemoveValue" hidden>&minus;</button>
    </span>
</template>
```

## Working with the script

The script is `_content/TopMarksDevelopment.ExpressionBuilder.Web/js/filter-builder.js`, an ES module that starts every
`[data-eb-filter-builder]` on the page when it loads.

```js
import { FilterBuilder, initialise } from "/_content/TopMarksDevelopment.ExpressionBuilder.Web/js/filter-builder.js";

initialise(container);                       // builders you added to the page later
const builder = FilterBuilder.attach(el);    // or one element

builder.json;                                // the filter, as it will be posted
builder.validate();                          // true if complete (and shows what is missing if not)
builder.load(savedJson);                     // replace the filter

el.addEventListener("eb:change", (e) => console.log(e.detail.json));
```

A form reset puts back what the server rendered.

## The posted format

The hidden input holds a small JSON document. A `connector` says how an item joins the **next** one, as in the library.

```json
{
  "version": 1,
  "root": {
    "type": "group", "connector": "And",
    "items": [
      { "type": "condition", "field": "Name", "operation": "Contains", "values": ["bike", "cycle"],
        "match": "Any", "manipulators": [{ "name": "ToLower" }, { "name": "Replace", "args": ["-", ""] }], "connector": "Or" },
      { "type": "group", "collection": "Reviews", "connector": "And",
        "items": [{ "type": "condition", "field": "Stars", "operation": "GreaterThanOrEqual", "values": ["4"] }] }
    ]
  }
}
```

Values are always text, and are parsed as the property's type using the invariant culture (`2024-01-31`,
`2024-01-31T09:30`, `09:30`, `1234.5`, `true`, an enum member's name). A scalar collection (`List<string>`) is filtered
with a group whose conditions use an empty `field`.

## Security

The posted filter comes from a browser, so it is checked as untrusted input before anything is built:

- A `field` is only ever **looked up** in the schema, never used to reach into the class. Properties that are ignored,
  unknown, or of the wrong shape are refused. There is no way to name a method.
- An `operation` must be offered for that property (a `Contains` on a number is refused).
- Values are parsed as the property's type; enum values must be a member **name**; numbers must be finite; dates use fixed
  formats.
- Adjustments are chosen by name from the list your app described (`Trim`, `ToLower`, `ToUpper`, `Replace` and any custom ones), and only for the kinds of property they suit. What a name makes is decided by your app's description, never by the post.
- Limits stop an oversized filter from becoming an expensive query (conditions, values, length, group depth, payload size).
- Nothing in the posted text is ever rendered as markup, and the form is protected by ASP.NET's anti-forgery token like
  any other.
- It works under a strict Content Security Policy (tested in Chromium, Firefox and WebKit, in an app published and run in Production): the schema is a non-executing `application/json` block, the script is an
  external module, and there are no inline handlers or styles.
  ASP.NET's own `asp-validation-summary` tag helper does write an inline `style`, so under a strict policy list the
  errors yourself (`ViewData.ModelState.Values.SelectMany(v => v.Errors)`); `tests/ExpressionBuilder.Web.E2E/Consumers` does.

What it cannot do for you: decide **which properties are safe to search**. Mark the ones that are not.

Also remember that a saved filter in the *library's* JSON format (`JsonSerializer.Serialize(filter)`) names .NET types
and should only be deserialised from storage you trust. Use `FilterBuilderModel.FromFilter` to show one in the builder.

## Working on the filter builder itself

The script is TypeScript in `src/ExpressionBuilder.Web/client`. Its output, `wwwroot/js/filter-builder.js`, is committed so
building the .NET projects never needs node.

```text
cd src/ExpressionBuilder.Web/client
npm ci
npm run build     # type-check, then bundle into ../wwwroot/js/filter-builder.js
npm test          # unit tests (node's test runner + jsdom), against the real Razor view
```

The script's tests render the real `Default.cshtml` (with the Razor turned into plain HTML), so the markup and the script
cannot drift apart. They also read `client/test/fixtures/person-schema.json`, which a .NET test checks is exactly what the
server produces; after changing the schema run the .NET tests once with `UPDATE_FIXTURES=1` and commit the new fixture. In the
other direction, `contract.test.ts` writes the documents the script posts to `client/test/fixtures/posted/`, and a .NET test
checks the server accepts each one and finds the right people.

CI fails if `wwwroot/js/filter-builder.js` is not what `npm run build` produces.
