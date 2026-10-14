namespace ExpressionBuilder.Web.E2E;

using System.Text;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

/// <summary>
/// axe, the accessibility checker, over the filter builder in each state a person can leave it in, in both colour
/// schemes. The page around the builder (the sample's own layout) is left out: this is about the package.
/// </summary>
[Collection(SampleAppCollection.Name)]
public class AccessibilityTests(SampleApp app)
{
    public static TheoryData<string, ColorScheme> BrowsersAndSchemes
    {
        get
        {
            var data = new TheoryData<string, ColorScheme>();

            foreach (var engine in SampleApp.Engines)
            foreach (
                var scheme in new[] { ColorScheme.Light, ColorScheme.Dark }
            )
                data.Add(engine, scheme);

            return data;
        }
    }

    static async Task AssertNoViolationsAsync(IPage page, string state)
    {
        var results = await page.RunAxe(
            new AxeRunContext
            {
                Include = [new AxeSelector("[data-eb-filter-builder]")],
            }
        );

        if (results.Violations.Length == 0)
            return;

        var report = new StringBuilder(
            $"axe found problems ({state}):"
        ).AppendLine();

        foreach (var violation in results.Violations)
        {
            report.AppendLine(
                $"- [{violation.Impact}] {violation.Id}: {violation.Help}"
            );

            foreach (var node in violation.Nodes.Take(3))
            {
                var why = node
                    .Any.Concat(node.All)
                    .Concat(node.None)
                    .Select(check => check.Message);

                report
                    .AppendLine($"    {node.Target}")
                    .AppendLine($"    {node.Html}")
                    .AppendLine($"    {string.Join(" / ", why)}");
            }
        }

        Assert.Fail(report.ToString());
    }

    [Theory(DisplayName = "Accessibility: an empty filter")]
    [MemberData(nameof(BrowsersAndSchemes))]
    public async Task EmptyFilter(string engine, ColorScheme scheme)
    {
        var page = await app.OpenAsync(engine, "/Products", scheme);

        await AssertNoViolationsAsync(page, "empty");
    }

    [Theory(
        DisplayName = "Accessibility: a filter using every kind of box, with its options open and a mistake showing"
    )]
    [MemberData(nameof(BrowsersAndSchemes))]
    public async Task BusyFilter(string engine, ColorScheme scheme)
    {
        var page = await app.OpenAsync(engine, "/Products", scheme);
        var builder = new Builder(page);

        var text = await builder.AddConditionAsync();
        await Builder.FillAsync(text, "Name", "Contains", "bike");
        await Builder.OpenOptionsAsync(text);

        var replace = text.Locator("[data-eb-manipulator-item=\"Replace\"]");
        await replace.Locator("[data-eb-manipulator]").CheckAsync();
        await replace.Locator("[data-eb-manipulator-arg]").First.FillAsync("-");

        var number = await builder.AddConditionAsync();
        await Builder.FillAsync(number, "Price", "Is between", "10", "500");

        var date = await builder.AddConditionAsync();
        await Builder.FillAsync(date, "Released", "Is at the weekend");

        var when = await builder.AddConditionAsync();
        await Builder.FillAsync(
            when,
            "Last ordered",
            "Is greater than",
            "2025-01-01T10:00"
        );

        var choice = await builder.AddConditionAsync();
        await Builder.FillAsync(choice, "Category", "Is one of", "Bikes");

        var flag = await builder.AddConditionAsync();
        await Builder.FillAsync(flag, "Discontinued", "Is", "No");

        var reviews = await builder.AddConditionAsync();
        await reviews
            .Locator("[data-eb-field]")
            .SelectOptionAsync(new SelectOptionValue { Label = "Reviews" });

        // An incomplete condition, with its error showing
        var incomplete = await builder.AddConditionAsync();
        await Builder.FillAsync(incomplete, "Name", "Starts with");
        await builder.SearchAsync();
        await Expect(incomplete.Locator("[data-eb-error]")).ToBeVisibleAsync();

        await AssertNoViolationsAsync(page, "busy");
    }

    [Theory(
        DisplayName = "Accessibility: the results page, with the saved filter shown"
    )]
    [MemberData(nameof(BrowsersAndSchemes))]
    public async Task AfterSearching(string engine, ColorScheme scheme)
    {
        var page = await app.OpenAsync(engine, "/Products?preset=true", scheme);
        var builder = new Builder(page);

        await builder.SearchAsync();
        await builder.ResultIdsAsync();

        await AssertNoViolationsAsync(page, "after searching");
    }
}
