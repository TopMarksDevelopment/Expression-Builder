namespace ExpressionBuilder.Web.E2E;

using ExpressionBuilder.Web.Sample.Models;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

/// <summary>
/// A person builds a filter by clicking, posts it, and gets the right results - in every browser.
/// </summary>
[Collection(SampleAppCollection.Name)]
public class FilterBuilderTests(SampleApp app)
{
    public static TheoryData<string> Browsers => SampleAppCollection.Browsers;

    [Theory(
        DisplayName = "Build a filter by clicking, post it, and see the right products"
    )]
    [MemberData(nameof(Browsers))]
    public async Task BuildsAndPosts(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var first = await builder.AddConditionAsync();
        await Builder.FillAsync(first, "Name", "Contains", "bike");

        var second = await builder.AddConditionAsync();
        await Builder.FillAsync(second, "Price", "Is greater than", "300");

        await builder.SearchAsync();

        var expected = app
            .Catalog.Where(p =>
                p.Name.Contains("bike", StringComparison.OrdinalIgnoreCase)
                && p.Price > 300
            )
            .Select(p => p.Id)
            .Order()
            .ToList();

        Assert.NotEmpty(expected);
        Assert.Equal(expected, await builder.ResultIdsAsync());
        await Expect(page.Locator("#results-heading"))
            .ToHaveTextAsync($"{expected.Count} products");

        // The builder shows what was posted
        await Expect(page.Locator("[data-eb-condition]")).ToHaveCountAsync(2);
    }

    [Theory(
        DisplayName = "Dates and times: a date box, a date-and-time box, and a range"
    )]
    [MemberData(nameof(Browsers))]
    public async Task DatesAndTimes(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var released = await builder.AddConditionAsync();
        await Builder.FillAsync(
            released,
            "Released",
            "Is greater than",
            "2024-06-01"
        );
        await Expect(released.Locator("[data-eb-control]"))
            .ToHaveAttributeAsync("type", "date");

        var ordered = await builder.AddConditionAsync();
        await Builder.FillAsync(
            ordered,
            "Last ordered",
            "Is less than",
            "2025-09-15T09:30"
        );
        await Expect(ordered.Locator("[data-eb-control]"))
            .ToHaveAttributeAsync("type", "datetime-local");

        var price = await builder.AddConditionAsync();
        await Builder.FillAsync(price, "Price", "Is between", "50", "400");
        await Expect(price.Locator("[data-eb-control]")).ToHaveCountAsync(2);

        await builder.SearchAsync();

        var expected = app
            .Catalog.Where(p =>
                p.Released > new DateTime(2024, 6, 1)
                && p.LastOrdered < new DateTime(2025, 9, 15, 9, 30, 0)
                && p.Price >= 50
                && p.Price <= 400
            )
            .Select(p => p.Id)
            .Order()
            .ToList();

        Assert.Equal(expected, await builder.ResultIdsAsync());
    }

    [Theory(
        DisplayName = "A collection is filtered by its items, and a list of choices posts the choice"
    )]
    [MemberData(nameof(Browsers))]
    public async Task CollectionsAndChoices(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var category = await builder.AddConditionAsync();
        await Builder.FillAsync(category, "Category", "Is", "Bikes");

        var reviews = await builder.AddConditionAsync();
        await reviews
            .Locator("[data-eb-field]")
            .SelectOptionAsync(new SelectOptionValue { Label = "Reviews" });

        // The row became a group for the items of the collection
        var group = page.Locator("[data-eb-group] [data-eb-group]");
        await Expect(group.Locator("[data-eb-title]"))
            .ToContainTextAsync("Reviews");

        await Builder.FillAsync(
            group.Locator("[data-eb-condition]"),
            "Stars",
            "Is greater than or equal to",
            "4"
        );
        await builder.SearchAsync();

        var expected = app
            .Catalog.Where(p =>
                p.Category == Category.Bikes && p.Reviews.Any(r => r.Stars >= 4)
            )
            .Select(p => p.Id)
            .Order()
            .ToList();

        Assert.Equal(expected, await builder.ResultIdsAsync());
    }

    [Theory(
        DisplayName = "A property of a related object is chosen through a path, and the path posts"
    )]
    [MemberData(nameof(Browsers))]
    public async Task PropertyBehindARelationship(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var row = await builder.AddConditionAsync();

        // The supplier moves into the path above the list, which then offers what a supplier has
        await row.Locator("[data-eb-field]")
            .SelectOptionAsync(new SelectOptionValue { Label = "Supplier" });

        var path = row.Locator("[data-eb-path]");
        await Expect(path).ToBeVisibleAsync();
        await Expect(path.Locator("[data-eb-path-step]"))
            .ToHaveTextAsync(["Product", "Supplier"]);
        await Expect(row.Locator("[data-eb-field] option[value='Name']"))
            .ToHaveCountAsync(1);
        await Expect(row.Locator("[data-eb-field] option[value='Price']"))
            .ToHaveCountAsync(0);

        await Builder.FillAsync(row, "Country", "Is", "France");
        await builder.SearchAsync();

        var expected = app
            .Catalog.Where(p => p.Supplier?.Country == "France")
            .Select(p => p.Id)
            .Order()
            .ToList();

        Assert.NotEmpty(expected);
        Assert.Equal(expected, await builder.ResultIdsAsync());

        // The saved filter shows the same path when the page comes back
        await Expect(
                page.Locator(
                    "[data-eb-condition] [data-eb-path] [data-eb-path-step]"
                )
            )
            .ToHaveTextAsync(["Product", "Supplier"]);
    }

    [Theory(
        DisplayName = "A collection can be asked for every item, not just any"
    )]
    [MemberData(nameof(Browsers))]
    public async Task EveryItemOfACollection(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var reviews = await builder.AddConditionAsync();
        await reviews
            .Locator("[data-eb-field]")
            .SelectOptionAsync(new SelectOptionValue { Label = "Reviews" });

        var group = page.Locator("[data-eb-group] [data-eb-group]");
        await group
            .Locator("[data-eb-group-match]")
            .SelectOptionAsync(new SelectOptionValue { Label = "Every item" });

        await Builder.FillAsync(
            group.Locator("[data-eb-condition]"),
            "Stars",
            "Is greater than or equal to",
            "3"
        );
        await builder.SearchAsync();

        // A product with no reviews has none that fall short, so it is among them
        var expected = app
            .Catalog.Where(p => p.Reviews.All(r => r.Stars >= 3))
            .Select(p => p.Id)
            .Order()
            .ToList();

        Assert.Contains(app.Catalog, p => p.Reviews.Count == 0);
        Assert.Equal(expected, await builder.ResultIdsAsync());
        await Expect(group.Locator("[data-eb-group-match]"))
            .ToHaveValueAsync("All");
    }

    [Theory(
        DisplayName = "The app's own operation and adjustment are offered apart, and work"
    )]
    [MemberData(nameof(Browsers))]
    public async Task CustomOperationAndManipulator(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var released = await builder.AddConditionAsync();
        await released
            .Locator("[data-eb-field]")
            .SelectOptionAsync(new SelectOptionValue { Label = "Released" });

        // The operations are in two groups, the built-in ones first
        var groups = released.Locator("[data-eb-operation] optgroup");
        await Expect(groups).ToHaveCountAsync(2);
        await Expect(groups.First).ToHaveAttributeAsync("label", "Standard");
        await Expect(groups.Last).ToHaveAttributeAsync("label", "Custom");

        await Builder.FillAsync(released, "Released", "Is at the weekend");

        var name = await builder.AddConditionAsync();
        await Builder.FillAsync(name, "Name", "Contains", "multitool");
        await Builder.OpenOptionsAsync(name);

        var custom = name.Locator("[data-eb-manipulator-group]").Last;
        await Expect(custom.Locator("[data-eb-manipulator-group-title]"))
            .ToHaveTextAsync("Custom");
        await custom.GetByLabel("Ignore punctuation").CheckAsync();

        // The first condition joins the second with Or
        await page.Locator("[data-eb-connector]").SelectOptionAsync("Or");
        await builder.SearchAsync();

        var expected = app
            .Catalog.Where(p =>
                p.Released.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                || p.Name.StartsWith("Multi-tool", StringComparison.Ordinal)
            )
            .Select(p => p.Id)
            .Order()
            .ToList();

        Assert.Equal(expected, await builder.ResultIdsAsync());
    }

    [Theory(
        DisplayName = "A very long property name stays inside its cell instead of covering the condition list"
    )]
    [MemberData(nameof(Browsers))]
    public async Task LongPropertyNameDoesNotOverflow(string engine)
    {
        foreach (var width in new[] { 1100, 390 })
        {
            var page = await app.OpenAsync(
                engine,
                "/Products",
                viewport: new ViewportSize { Width = width, Height = 800 }
            );
            var row = await new Builder(page).AddConditionAsync();

            // A select is as wide as its longest option, so one long name is all it takes
            await row.Locator("[data-eb-field]")
                .EvaluateAsync(
                    "s => s.add(new Option('A really quite long property name that goes on and on and on (Total of all things)', 'long'))"
                );

            var field = await row.Locator("[data-eb-field]").BoundingBoxAsync();
            var picker = await row.Locator("[data-eb-picker]")
                .BoundingBoxAsync();
            var operation = await row.Locator("[data-eb-operation]")
                .BoundingBoxAsync();

            Assert.NotNull(field);
            Assert.NotNull(picker);
            Assert.NotNull(operation);
            Assert.True(
                field.Width <= picker.Width + 0.5,
                $"{engine} at {width}px: the property list is {field.Width}px wide in a cell {picker.Width}px wide"
            );

            // Side by side, the property list must end before the condition list starts
            if (Math.Abs(field.Y - operation.Y) < field.Height)
                Assert.True(
                    field.X + field.Width <= operation.X + 0.5,
                    $"{engine} at {width}px: the property list covers the condition list"
                );

            Assert.False(
                await page.EvaluateAsync<bool>(
                    "() => document.documentElement.scrollWidth > window.innerWidth"
                ),
                $"{engine} at {width}px: the page scrolls sideways"
            );
        }
    }

    [Theory(
        DisplayName = "At phone and tablet widths every control stays inside the builder"
    )]
    [MemberData(nameof(Browsers))]
    public async Task FitsNarrowScreens(string engine)
    {
        foreach (var width in new[] { 390, 768 })
        {
            var page = await app.OpenAsync(
                engine,
                "/Products?preset=true",
                viewport: new ViewportSize { Width = width, Height = 800 }
            );
            await Expect(page.Locator("[data-eb-condition]").First)
                .ToBeVisibleAsync();

            // A list sized to its longest option, as in a real app with long names
            await page.Locator("[data-eb-operation]")
                .First.EvaluateAsync(
                    "s => s.add(new Option('A really quite long condition name that goes on and on', 'long'))"
                );

            var outside = await page.EvaluateAsync<string[]>(
                """
                () => {
                    const box = document.querySelector('.eb').getBoundingClientRect();
                    return [...document.querySelectorAll('.eb select, .eb input, .eb button')]
                        .filter(e => {
                            const r = e.getBoundingClientRect();
                            return r.width > 0 && (r.right > box.right + 1 || r.left < box.left - 1);
                        })
                        .map(e => e.className);
                }
                """
            );

            Assert.True(
                outside.Length == 0,
                $"{engine} at {width}px: {string.Join(", ", outside)} stick out of the builder"
            );
            Assert.False(
                await page.EvaluateAsync<bool>(
                    "() => document.documentElement.scrollWidth > window.innerWidth"
                ),
                $"{engine} at {width}px: the page scrolls sideways"
            );
        }
    }

    [Theory(
        DisplayName = "An incomplete filter is not posted, and says what is missing"
    )]
    [MemberData(nameof(Browsers))]
    public async Task IncompleteFilterIsNotPosted(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var row = await builder.AddConditionAsync();
        await Builder.FillAsync(row, "Name", "Contains");
        await builder.SearchAsync();

        await Expect(row.Locator("[data-eb-error]")).ToBeVisibleAsync();
        await Expect(page.Locator("#results-heading")).ToHaveCountAsync(0);
    }

    [Theory(
        DisplayName = "A saved filter is shown, and posting it again finds products"
    )]
    [MemberData(nameof(Browsers))]
    public async Task PresetRoundTrips(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products?preset=true");
        var builder = new Builder(page);

        await Expect(page.Locator("[data-eb-condition]").First)
            .ToBeVisibleAsync();
        await builder.SearchAsync();

        Assert.NotEmpty(await builder.ResultIdsAsync());
    }
}
