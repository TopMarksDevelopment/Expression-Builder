namespace ExpressionBuilder.Web.Tests;

using System.Net;
using System.Text.RegularExpressions;
using ExpressionBuilder.Web.Sample.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using static ExpressionBuilder.Web.Tests.TestData;

/// <summary>
/// The whole thing, as a visitor meets it: a page with the filter builder on it, the files it needs,
/// and a post that is checked and applied. The sample app has one page of each kind (MVC and Razor Pages).
/// </summary>
[Collection("Operation catalog")]
public partial class SampleAppTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    /// <summary>The two pages, and the name each posts the filter under.</summary>
    public static TheoryData<string, string> Pages =>
        new()
        {
            { "/Products", "Filter.Json" },
            { "/Search", "Search.Filter.Json" },
        };

    IReadOnlyList<Product> Catalog =>
        factory.Services.GetRequiredService<ProductCatalog>().Products;

    HttpClient Client() => factory.CreateClient();

    /// <summary>The ids in the first column of the results table.</summary>
    static List<int> ShownIds(string html) =>
        [
            .. Regex
                .Matches(html, @"<tr>\s*<td>(\d+)</td>")
                .Select(m => int.Parse(m.Groups[1].Value))
                .Order(),
        ];

    [GeneratedRegex(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\""
    )]
    private static partial Regex Token();

    async Task<(string Html, HttpClient Client)> Get(string url)
    {
        var client = Client();
        var html = await client.GetStringAsync(url);

        return (html, client);
    }

    /// <summary>Posts the form of a page, the way a browser would, with its anti-forgery token.</summary>
    async Task<(HttpStatusCode Status, string Html)> Post(
        string page,
        string field,
        string? json,
        bool withToken = true
    )
    {
        var (html, client) = await Get(page);
        var form = new Dictionary<string, string> { [field] = json ?? "" };

        if (withToken)
            form["__RequestVerificationToken"] = WebUtility.HtmlDecode(
                Token().Match(html).Groups[1].Value
            );

        var response = await client.PostAsync(
            page,
            new FormUrlEncodedContent(form)
        );

        return (
            response.StatusCode,
            await response.Content.ReadAsStringAsync()
        );
    }

    #region The page

    [Theory(
        DisplayName = "The page has the filter builder, posting under the model's name"
    )]
    [MemberData(nameof(Pages))]
    public async Task PageHasTheFilterBuilder(string page, string field)
    {
        var (html, _) = await Get(page);

        Assert.Contains("data-eb-filter-builder", html);
        Assert.Contains($"name=\"{field}\"", html);
        Assert.Contains("type=\"hidden\"", html);
        Assert.Contains("<template data-eb-template=\"condition\">", html);
        Assert.Contains("<template data-eb-template=\"group\">", html);
        Assert.Contains("<template data-eb-template=\"connector\">", html);
        Assert.Contains("data-eb-schema", html);
    }

    [Theory(
        DisplayName = "The page brings in the script and the stylesheet, once"
    )]
    [MemberData(nameof(Pages))]
    public async Task PageIncludesItsAssets(string page, string _)
    {
        var (html, _) = await Get(page);

        Assert.Single(Regex.Matches(html, "filter-builder\\.js"));
        Assert.Single(Regex.Matches(html, "filter-builder\\.css"));
        Assert.Contains("type=\"module\"", html);
    }

    [Theory(
        DisplayName = "The script and the stylesheet are served by the package"
    )]
    [InlineData(
        "/_content/TopMarksDevelopment.ExpressionBuilder.Web/js/filter-builder.js",
        "javascript"
    )]
    [InlineData(
        "/_content/TopMarksDevelopment.ExpressionBuilder.Web/css/filter-builder.css",
        "css"
    )]
    public async Task AssetsAreServed(string url, string contentType)
    {
        var response = await Client().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            contentType,
            response.Content.Headers.ContentType?.MediaType
        );
        Assert.True((await response.Content.ReadAsStringAsync()).Length > 1000);
    }

    [Fact(
        DisplayName = "The page tells the script what can be filtered, without what is protected"
    )]
    public async Task SchemaDescribesProductButNotItsCost()
    {
        var (html, _) = await Get("/Products");
        var schema = Regex
            .Match(
                html,
                "<script type=\"application/json\" data-eb-schema>(.*?)</script>",
                RegexOptions.Singleline
            )
            .Groups[1]
            .Value;

        Assert.Contains("\"path\":\"Name\"", schema);
        Assert.Contains("\"path\":\"Supplier.Country\"", schema);
        Assert.Contains("\"path\":\"Reviews\"", schema);
        Assert.DoesNotContain("Cost", schema);
    }

    [Fact(
        DisplayName = "The settings given to AddFilterBuilder reach the page"
    )]
    public async Task OptionsReachThePage()
    {
        var (html, _) = await Get("/Products");

        // The sample renames Equal
        Assert.Contains("\"name\":\"Equal\",\"label\":\"Is\"", html);
    }

    [Theory(DisplayName = "A saved filter can be shown in the builder")]
    [InlineData("/Products?preset=true", "Filter.Json")]
    [InlineData("/Search?preset=true", "Search.Filter.Json")]
    public async Task PresetIsShown(string url, string field)
    {
        var (html, _) = await Get(url);
        var value = WebUtility.HtmlDecode(
            Regex
                .Match(
                    html,
                    $"name=\"{Regex.Escape(field)}\" value=\"([^\"]*)\""
                )
                .Groups[1]
                .Value
        );

        Assert.Contains("\"field\":\"Category\"", value);
        Assert.Contains("\"collection\":\"Reviews\"", value);
    }

    #endregion The page

    #region Posting

    [Theory(DisplayName = "A posted filter is applied to the products")]
    [MemberData(nameof(Pages))]
    public async Task PostedFilterIsApplied(string page, string field)
    {
        var json = TestData.Doc(
            Cond("Category", "Equal", ["Bikes"]),
            Cond("Price", "LessThan", ["400"], connector: "Or"),
            Cond("Name", "StartsWith", ["Chain"])
        );

        var (status, html) = await Post(page, field, json);

        var expected = Catalog
            .Where(p =>
                (p.Category == Category.Bikes && p.Price < 400)
                || p.Name.StartsWith(
                    "Chain",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .ToList();

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.NotEmpty(expected);
        Assert.Contains($"{expected.Count} product", html);

        Assert.Equal(expected.Select(p => p.Id).Order(), ShownIds(html));
    }

    [Fact(
        DisplayName = "A filter on a collection, a related object and a date"
    )]
    public async Task ComplexFilterIsApplied()
    {
        var json = TestData.Doc(
            Group(
                [Cond("Stars", "GreaterThanOrEqual", ["4"])],
                collection: "Reviews"
            ),
            Cond("Supplier.Country", "In", ["UK", "France"]),
            Cond("Released", "GreaterThan", ["2023-01-01"])
        );

        var (_, html) = await Post("/Products", "Filter.Json", json);

        var expected = Catalog
            .Where(p =>
                p.Reviews.Any(r => r.Stars >= 4)
                && p.Supplier != null
                && (
                    p.Supplier.Country == "UK" || p.Supplier.Country == "France"
                )
                && p.Released > new DateTime(2023, 1, 1)
            )
            .Select(p => p.Id)
            .ToList();

        Assert.NotEmpty(expected);
        Assert.Equal(
            expected,
            Regex
                .Matches(html, "<tr>\\s*<td>(\\d+)</td>")
                .Select(m => int.Parse(m.Groups[1].Value))
                .Order()
                .ToList()
        );
    }

    [Fact(DisplayName = "The filter in words is shown")]
    public async Task DescriptionIsShown()
    {
        var (_, html) = await Post(
            "/Products",
            "Filter.Json",
            TestData.Doc(Cond("Name", "Contains", ["bike"]))
        );

        Assert.Contains("class=\"description\"", html);
        Assert.Contains("Name", html);
    }

    [Fact(DisplayName = "Nothing posted shows every product")]
    public async Task EmptyFilterShowsEverything()
    {
        var (status, html) = await Post(
            "/Products",
            "Filter.Json",
            TestData.Doc()
        );

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"{Catalog.Count} products", html);
    }

    [Theory(
        DisplayName = "A filter that is refused is explained, and nothing is shown"
    )]
    [MemberData(nameof(Pages))]
    public async Task RefusedFilterIsExplained(string page, string field)
    {
        var (status, html) = await Post(
            page,
            field,
            TestData.Doc(Cond("Cost", "GreaterThan", ["1"]))
        );

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(
            "&quot;Cost&quot; is not a property that can be filtered on.",
            html
        );
        Assert.DoesNotContain("results-heading", html);
    }

    [Theory(DisplayName = "Rubbish is refused rather than failing")]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData(
        "{\"version\":1,\"root\":{\"type\":\"group\",\"items\":[{\"type\":\"condition\",\"field\":\"Name\",\"operation\":\"Equal\",\"values\":[1]}]}}"
    )]
    public async Task RubbishIsRefused(string json)
    {
        var (status, html) = await Post("/Products", "Filter.Json", json);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("validation-summary-errors", html);
    }

    [Fact(DisplayName = "A post needs the anti-forgery token the form carries")]
    public async Task PostNeedsAToken()
    {
        var (status, _) = await Post(
            "/Products",
            "Filter.Json",
            TestData.Doc(),
            withToken: false
        );

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    [Fact(DisplayName = "The filter that is posted is kept on the page")]
    public async Task PostedFilterStaysInTheBuilder()
    {
        var json = TestData.Doc(Cond("Name", "Contains", ["bike"]));
        var (_, html) = await Post("/Products", "Filter.Json", json);

        var value = WebUtility.HtmlDecode(
            Regex
                .Match(html, "name=\"Filter.Json\" value=\"([^\"]*)\"")
                .Groups[1]
                .Value
        );

        Assert.Contains("\"field\":\"Name\"", value);
    }

    [Fact(DisplayName = "Posted values are never trusted as markup")]
    public async Task PostedValuesAreEncoded()
    {
        var json = TestData.Doc(
            Cond("Name", "Contains", ["<script>alert(1)</script>\" onload=\"x"])
        );
        var (_, html) = await Post("/Products", "Filter.Json", json);

        Assert.DoesNotContain("<script>alert(1)</script>", html);
        Assert.Contains("0 products", html);
    }

    [Fact(
        DisplayName = "The app's own operation and manipulator are offered, under a heading of their own"
    )]
    public async Task CustomThingsAreOffered()
    {
        var (html, _) = await Get("/Products");

        Assert.Contains(
            "\"name\":\"IsWeekend\",\"label\":\"Is at the weekend\"",
            html
        );
        Assert.Contains("\"group\":\"Custom\"", html);
        Assert.Contains("\"name\":\"IgnorePunctuation\"", html);
    }

    [Fact(DisplayName = "The app's own operation and manipulator are applied")]
    public async Task CustomThingsAreApplied()
    {
        var json = TestData.Doc(
            Cond("Released", "IsWeekend", connector: "Or"),
            Cond(
                "Name",
                "Contains",
                ["multitool"],
                manipulators: [new { name = "IgnorePunctuation" }]
            )
        );

        var (status, html) = await Post("/Products", "Filter.Json", json);

        var expected = Catalog
            .Where(p =>
                p.Released.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                || p.Name.StartsWith("Multi-tool", StringComparison.Ordinal)
            )
            .Select(p => p.Id)
            .Order()
            .ToList();

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(
            Catalog.Single(p => p.Name == "Multi-tool").Id,
            expected
        );
        Assert.Equal(expected, ShownIds(html));
    }

    #endregion Posting
}
