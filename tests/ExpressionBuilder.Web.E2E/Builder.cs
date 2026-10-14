namespace ExpressionBuilder.Web.E2E;

using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

/// <summary>
/// Drives the filter builder the way a person does: by choosing from lists, typing and clicking, finding things by
/// what they are called on the page.
/// </summary>
public sealed class Builder(IPage page)
{
    public IPage Page => page;

    ILocator Conditions => page.Locator("[data-eb-condition]");

    /// <summary>Adds a condition to the top level, and returns its row.</summary>
    public async Task<ILocator> AddConditionAsync()
    {
        var before = await Conditions.CountAsync();

        await page.Locator("[data-eb-root] > [data-eb-group] > footer")
            .GetByRole(AriaRole.Button, new() { Name = "Add condition" })
            .ClickAsync();

        await Expect(Conditions).ToHaveCountAsync(before + 1);

        return Conditions.Last;
    }

    /// <summary>Chooses a property, then (if given) a condition, then types or chooses the values.</summary>
    public static async Task FillAsync(
        ILocator row,
        string property,
        string? condition = null,
        params string[] values
    )
    {
        await row.Locator("[data-eb-field]")
            .SelectOptionAsync(new SelectOptionValue { Label = property });

        if (condition != null)
            await row.Locator("[data-eb-operation]")
                .SelectOptionAsync(new SelectOptionValue { Label = condition });

        await SetValuesAsync(row, values);
    }

    public static async Task SetValuesAsync(
        ILocator row,
        params string[] values
    )
    {
        for (var i = 0; i < values.Length; i++)
        {
            var control = row.Locator("[data-eb-values] [data-eb-control]")
                .Nth(i);

            if (
                await control.EvaluateAsync<string>("e => e.tagName")
                == "SELECT"
            )
                await control.SelectOptionAsync(
                    new SelectOptionValue { Label = values[i] }
                );
            else
                await control.FillAsync(values[i]);
        }
    }

    /// <summary>Opens the options (match any or all, adjustments) of a condition.</summary>
    public static async Task OpenOptionsAsync(ILocator row)
    {
        var toggle = row.Locator("[data-eb-toggle-options]");

        if (await toggle.GetAttributeAsync("aria-expanded") != "true")
            await toggle.ClickAsync();
    }

    public async Task SearchAsync() =>
        await page.GetByRole(AriaRole.Button, new() { Name = "Search" })
            .ClickAsync();

    /// <summary>The ids shown in the results table, once the results have appeared.</summary>
    public async Task<List<int>> ResultIdsAsync()
    {
        await Expect(page.Locator("#results-heading")).ToBeVisibleAsync();

        return
        [
            .. (
                await page.Locator("tbody tr td:first-child")
                    .AllInnerTextsAsync()
            )
                .Select(int.Parse)
                .Order(),
        ];
    }

    /// <summary>The filter, as it is about to be posted.</summary>
    public async Task<string> PostedJsonAsync() =>
        await page.Locator("[data-eb-input]").InputValueAsync();
}
