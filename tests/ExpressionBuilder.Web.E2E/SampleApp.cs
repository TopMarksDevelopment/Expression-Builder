namespace ExpressionBuilder.Web.E2E;

using ExpressionBuilder.Web.Sample.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

/// <summary>
/// The sample app running on a real port, and the browsers to drive it with. Shared by every test, as
/// starting either is slow.
/// </summary>
public sealed class SampleApp : IAsyncLifetime
{
    /// <summary>
    /// The browsers to test with. All three unless <c>EB_E2E_BROWSERS</c> lists some, such as
    /// <c>chromium</c>.
    /// </summary>
    public static readonly string[] Engines = (
        Environment.GetEnvironmentVariable("EB_E2E_BROWSERS")
        ?? "chromium,firefox,webkit"
    ).Split(
        ',',
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
    );

    readonly WebApplicationFactory<global::Program> _factory = new();
    readonly Dictionary<string, IBrowser> _browsers = [];
    IPlaywright? _playwright;

    public string BaseUrl { get; private set; } = "";

    public IReadOnlyList<Product> Catalog =>
        _factory.Services.GetRequiredService<ProductCatalog>().Products;

    public async Task InitializeAsync()
    {
        _factory.UseKestrel(0);
        _factory.StartServer();
        BaseUrl = _factory.CreateClient().BaseAddress!.ToString().TrimEnd('/');

        // A no-op once a browser is installed. CI installs them (and what they need from the OS) first.
        foreach (var engine in Engines)
            if (Microsoft.Playwright.Program.Main(["install", engine]) != 0)
                throw new InvalidOperationException(
                    $"Could not install {engine} for Playwright."
                );

        _playwright = await Playwright.CreateAsync();
    }

    public async Task DisposeAsync()
    {
        foreach (var browser in _browsers.Values)
            await browser.CloseAsync();

        _playwright?.Dispose();
        await _factory.DisposeAsync();
    }

    /// <summary>
    /// Opens a page of the sample in a new browser context, so tests do not share cookies or state.
    /// </summary>
    public Task<IPage> OpenAsync(
        string engine,
        string path,
        ColorScheme scheme = ColorScheme.Light,
        ViewportSize? viewport = null
    ) => OpenUrlAsync(engine, BaseUrl + path, scheme, viewport);

    /// <summary>
    /// Opens any address in a new browser context. The page notes every Content-Security-Policy violation and every
    /// script error (see <see cref="PageExtensions.ProblemsAsync"/>).
    /// </summary>
    public async Task<IPage> OpenUrlAsync(
        string engine,
        string url,
        ColorScheme scheme = ColorScheme.Light,
        ViewportSize? viewport = null
    )
    {
        var type = engine switch
        {
            "chromium" => _playwright!.Chromium,
            "firefox" => _playwright!.Firefox,
            "webkit" => _playwright!.Webkit,
            _ => throw new ArgumentException($"Unknown browser {engine}"),
        };

        if (!_browsers.TryGetValue(engine, out var browser))
            _browsers[engine] = browser = await type.LaunchAsync();

        var context = await browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                ColorScheme = scheme,
                ViewportSize =
                    viewport ?? new ViewportSize { Width = 1100, Height = 900 },
            }
        );
        var page = await context.NewPageAsync();

        await page.AddInitScriptAsync(PageExtensions.WatchScript);
        await page.GotoAsync(url);

        return page;
    }
}
