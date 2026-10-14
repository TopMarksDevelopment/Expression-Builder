namespace ExpressionBuilder.Web.E2E;

using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

/// <summary>
/// The packages as an app meets them: installed from a feed into a new MVC app and a new Razor Pages app, published,
/// run in Production (so the files must come from what was published, not from the build's own folders), under a
/// Content-Security-Policy that allows nothing inline.
/// </summary>
[Collection(SampleAppCollection.Name)]
public class PublishedAppTests(SampleApp app, PublishedApps published)
{
    const string Package =
        "/_content/TopMarksDevelopment.ExpressionBuilder.Web";

    public static TheoryData<string> Apps => new() { "Mvc", "RazorPages" };

    public static TheoryData<string, string> AppsAndBrowsers
    {
        get
        {
            var data = new TheoryData<string, string>();

            foreach (var name in new[] { "Mvc", "RazorPages" })
            foreach (var engine in SampleApp.Engines)
                data.Add(name, engine);

            return data;
        }
    }

    [Theory(
        DisplayName = "Published, in Production: the script and the stylesheet are served by the app"
    )]
    [MemberData(nameof(Apps))]
    public async Task AssetsAreServed(string name)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri(published.Urls[name]),
        };

        var script = await client.GetAsync($"{Package}/js/filter-builder.js");
        var style = await client.GetAsync($"{Package}/css/filter-builder.css");

        Assert.Equal(System.Net.HttpStatusCode.OK, script.StatusCode);
        Assert.Contains(
            "javascript",
            script.Content.Headers.ContentType?.MediaType
        );
        Assert.Contains(
            "data-eb-filter-builder",
            await script.Content.ReadAsStringAsync()
        );

        Assert.Equal(System.Net.HttpStatusCode.OK, style.StatusCode);
        Assert.Equal("text/css", style.Content.Headers.ContentType?.MediaType);
    }

    [Theory(
        DisplayName = "Published: the page has the builder, and links to files that exist"
    )]
    [MemberData(nameof(Apps))]
    public async Task PageLinksToItsAssets(string name)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri(published.Urls[name]),
        };

        var html = await client.GetStringAsync("/");

        Assert.Contains("data-eb-filter-builder", html);
        Assert.Contains($"{Package}/js/filter-builder.js", html);
        Assert.Contains($"{Package}/css/filter-builder.css", html);
        Assert.DoesNotContain("<style", html);
        Assert.DoesNotContain(" onclick=", html);
    }

    [Theory(
        DisplayName = "Published, strict policy: build a filter, post it, and see the right products - with nothing refused"
    )]
    [MemberData(nameof(AppsAndBrowsers))]
    public async Task BuildsAndPostsUnderAStrictPolicy(
        string name,
        string engine
    )
    {
        var page = await app.OpenUrlAsync(engine, published.Urls[name]);
        var builder = new Builder(page);

        var first = await builder.AddConditionAsync();
        await Builder.FillAsync(first, "Name", "Contains", "bike");

        var second = await builder.AddConditionAsync();
        await Builder.FillAsync(second, "Price", "Is greater than", "50");

        // The stylesheet was applied (a group has a coloured edge), so the policy let it through
        var edge = await page.Locator("[data-eb-group]")
            .First.EvaluateAsync<string>(
                "e => getComputedStyle(e).borderLeftWidth"
            );
        Assert.Equal("4px", edge);

        await builder.SearchAsync();

        // The consumer's products: ids 1-30, named "Road bike" (every third), "Helmet" or "Bike lock", priced 5 each
        var expected = Enumerable
            .Range(1, 30)
            .Where(i => i % 3 != 1 && i * 5 > 50)
            .ToList();

        Assert.Equal(expected, await builder.ResultIdsAsync());
        await Expect(page.Locator("[data-eb-condition]")).ToHaveCountAsync(2);

        Assert.Empty(await page.ProblemsAsync());
    }

    [Theory(
        DisplayName = "Published: the builder is on the page the app was posted back to, with its anti-forgery token"
    )]
    [MemberData(nameof(Apps))]
    public async Task PostNeedsTheToken(string name)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri(published.Urls[name]),
        };
        var field = name == "Mvc" ? "Filter.Json" : "Search.Filter.Json";

        var response = await client.PostAsync(
            "/",
            new FormUrlEncodedContent(
                new Dictionary<string, string> { [field] = "{}" }
            )
        );

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
