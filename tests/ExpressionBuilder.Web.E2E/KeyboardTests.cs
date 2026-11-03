namespace ExpressionBuilder.Web.E2E;

using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

/// <summary>
/// The builder with no mouse: every control can be reached and used from the keyboard, and focus is never left
/// stranded on something that is no longer there.
/// </summary>
[Collection(SampleAppCollection.Name)]
public class KeyboardTests(SampleApp app)
{
    public static TheoryData<string> Browsers => SampleAppCollection.Browsers;

    /// <summary>What a screen reader would call the focused control.</summary>
    static async Task<string> FocusedNameAsync(IPage page) =>
        await page.EvaluateAsync<string>(
            """
            () => {
                const e = document.activeElement;
                return e === document.body ? "(nothing)" : (e.getAttribute("aria-label") || e.textContent || e.tagName).trim();
            }
            """
        );

    /// <summary>Presses Tab until the focused control has this name.</summary>
    static async Task TabToAsync(IPage page, string name, int limit = 60)
    {
        for (var i = 0; i < limit; i++)
        {
            await page.Keyboard.PressAsync("Tab");

            if (await FocusedNameAsync(page) == name)
                return;
        }

        Assert.Fail(
            $"Tab never reached \"{name}\"; the last stop was \"{await FocusedNameAsync(page)}\""
        );
    }

    /// <summary>True when focus is somewhere inside the filter builder.</summary>
    static async Task<bool> FocusIsInsideAsync(IPage page) =>
        await page.EvaluateAsync<bool>(
            "() => !!document.activeElement?.closest('[data-eb-filter-builder]')"
        );

    [Theory(
        DisplayName = "A filter can be built, posted and read with the keyboard alone"
    )]
    [MemberData(nameof(Browsers))]
    public async Task WholeFlowWithoutAMouse(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        await TabToAsync(page, "Add condition");
        await page.Keyboard.PressAsync("Enter");

        // Adding a condition puts you in it, ready to choose a property
        Assert.Equal("Property 1", await FocusedNameAsync(page));

        // A closed list is changed by typing the start of what you want (or the arrow keys)
        await page.Keyboard.TypeAsync("Name");
        await Expect(page.Locator("[data-eb-field]")).ToHaveValueAsync("Name");

        await TabToAsync(page, "Condition 1");
        await page.Locator("[data-eb-operation]")
            .SelectOptionAsync(new SelectOptionValue { Label = "Contains" });

        await TabToAsync(page, "Value");
        await page.Keyboard.TypeAsync("bike");

        // Enter in a text box submits the form, as it does anywhere else
        await page.Keyboard.PressAsync("Enter");

        var expected = app
            .Catalog.Where(p =>
                p.Name.Contains("bike", StringComparison.OrdinalIgnoreCase)
            )
            .Select(p => p.Id)
            .Order()
            .ToList();

        Assert.Equal(expected, await builder.ResultIdsAsync());
    }

    [Theory(
        DisplayName = "Removing a condition leaves focus in the builder, not on nothing"
    )]
    [MemberData(nameof(Browsers))]
    public async Task FocusAfterRemovingACondition(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        await builder.AddConditionAsync();
        await builder.AddConditionAsync();
        await builder.AddConditionAsync();

        // Remove the middle one with the keyboard
        await page.Locator("[data-eb-remove]").Nth(1).FocusAsync();
        await page.Keyboard.PressAsync("Enter");

        await Expect(page.Locator("[data-eb-condition]")).ToHaveCountAsync(2);
        Assert.True(
            await FocusIsInsideAsync(page),
            $"focus was lost: {await FocusedNameAsync(page)}"
        );

        // ...and the last one, then the only one left
        await page.Locator("[data-eb-remove]").Last.FocusAsync();
        await page.Keyboard.PressAsync("Enter");
        Assert.True(
            await FocusIsInsideAsync(page),
            $"focus was lost: {await FocusedNameAsync(page)}"
        );

        await page.Locator("[data-eb-remove]").First.FocusAsync();
        await page.Keyboard.PressAsync("Enter");

        await Expect(page.Locator("[data-eb-condition]")).ToHaveCountAsync(0);
        Assert.Equal("Add condition", await FocusedNameAsync(page));
    }

    [Theory(
        DisplayName = "Removing a group, or one value of several, leaves focus in the builder"
    )]
    [MemberData(nameof(Browsers))]
    public async Task FocusAfterRemovingAGroupOrAValue(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var row = await builder.AddConditionAsync();
        await Builder.FillAsync(row, "Name", "Is one of", "a");
        await row.Locator("[data-eb-add-value]").ClickAsync();
        await Expect(row.Locator("[data-eb-value]")).ToHaveCountAsync(2);

        await row.Locator("[data-eb-remove-value]:visible").Last.FocusAsync();
        await page.Keyboard.PressAsync("Enter");

        await Expect(row.Locator("[data-eb-value]")).ToHaveCountAsync(1);
        Assert.True(
            await FocusIsInsideAsync(page),
            $"focus was lost: {await FocusedNameAsync(page)}"
        );

        await page.GetByRole(AriaRole.Button, new() { Name = "Add group" })
            .ClickAsync();
        await Expect(page.Locator("[data-eb-group] [data-eb-group]"))
            .ToHaveCountAsync(1);

        await page.Locator("[data-eb-remove-group]:visible").FocusAsync();
        await page.Keyboard.PressAsync("Enter");

        await Expect(page.Locator("[data-eb-group] [data-eb-group]"))
            .ToHaveCountAsync(0);
        Assert.True(
            await FocusIsInsideAsync(page),
            $"focus was lost: {await FocusedNameAsync(page)}"
        );
    }

    [Theory(
        DisplayName = "The options open and close with the keyboard, and the toggle says which"
    )]
    [MemberData(nameof(Browsers))]
    public async Task OptionsToggle(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var row = await builder.AddConditionAsync();
        await Builder.FillAsync(row, "Name", "Contains", "a");

        var toggle = row.Locator("[data-eb-toggle-options]");

        await toggle.FocusAsync();
        await page.Keyboard.PressAsync("Space");

        await Expect(toggle).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(row.Locator("[data-eb-options]")).ToBeVisibleAsync();

        // The toggle names the panel it controls
        var controls = await toggle.GetAttributeAsync("aria-controls");
        Assert.False(string.IsNullOrEmpty(controls));
        Assert.Equal(
            controls,
            await row.Locator("[data-eb-options]").GetAttributeAsync("id")
        );

        await page.Keyboard.PressAsync("Space");
        await Expect(toggle).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Theory(DisplayName = "A mistake is announced, and focus goes to it")]
    [MemberData(nameof(Browsers))]
    public async Task MistakeIsAnnouncedAndFocused(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var row = await builder.AddConditionAsync();
        await Builder.FillAsync(row, "Name", "Contains");
        await builder.SearchAsync();

        var error = row.Locator("[data-eb-error]");

        await Expect(error).ToBeVisibleAsync();
        await Expect(error).ToHaveAttributeAsync("role", "alert");
        Assert.True(
            await FocusIsInsideAsync(page),
            $"focus: {await FocusedNameAsync(page)}"
        );
    }

    [Theory(
        DisplayName = "Screen readers: groups have names, and the summary is a live region"
    )]
    [MemberData(nameof(Browsers))]
    public async Task WhatAScreenReaderSees(string engine)
    {
        var page = await app.OpenAsync(engine, "/Products");
        var builder = new Builder(page);

        var row = await builder.AddConditionAsync();
        await Builder.FillAsync(row, "Name", "Contains", "bike");

        // The group is named after its heading
        await Expect(
                page.GetByRole(AriaRole.Group, new() { Name = "Conditions" })
            )
            .ToBeVisibleAsync();

        // The sentence under the builder is announced when it changes
        var summary = page.GetByRole(AriaRole.Status);
        await Expect(summary).ToContainTextAsync("bike");

        // Controls say which condition they belong to, and the remove buttons which one they remove
        await Expect(
                row.GetByRole(
                    AriaRole.Button,
                    new() { Name = "Remove condition 1" }
                )
            )
            .ToBeVisibleAsync();
        await Expect(page.GetByLabel("Property 1", new() { Exact = true }))
            .ToBeVisibleAsync();
        await Expect(page.GetByLabel("Condition 1", new() { Exact = true }))
            .ToBeVisibleAsync();
    }
}
