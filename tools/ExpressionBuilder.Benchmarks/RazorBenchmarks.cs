namespace ExpressionBuilder.Benchmarks;

using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TopMarksDevelopment.ExpressionBuilder.Web;

public class BenchPage
{
    public Type? ModelType { get; set; }

    public FilterBuilderModel Filter { get; set; } = new();
}

public class BenchController : Controller
{
    [HttpGet("/page/{size}")]
    public IActionResult Index(string size) =>
        View(
            new BenchPage
            {
                ModelType = size switch
                {
                    "small" => typeof(Small),
                    "large" => typeof(Large),
                    _ => null,
                },
            }
        );
}

/// <summary>
/// The whole request for a page with the filter builder on it, in an in-memory server (no network): routing, the
/// controller, the tag helper, the schema, and the Razor view. <c>none</c> is the same page without a builder, which is
/// what the others have to be compared with.
/// </summary>
[MemoryDiagnoser]
public class RazorBenchmarks
{
    WebApplication _app = null!;
    HttpClient _client = null!;

    [Params("none", "small", "large")]
    public string Size { get; set; } = "none";

    [GlobalSetup]
    public async Task Setup()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                ContentRootPath = AppContext.BaseDirectory,
                ApplicationName = typeof(RazorBenchmarks)
                    .Assembly.GetName()
                    .Name,
                EnvironmentName = Environments.Production,
            }
        );

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder
            .Services.AddControllersWithViews()
            .AddApplicationPart(typeof(RazorBenchmarks).Assembly);
        builder.Services.AddFilterBuilder();

        _app = builder.Build();
        _app.MapControllers();

        await _app.StartAsync();

        _client = _app.GetTestClient();

        var html = await _client.GetStringAsync("/page/" + Size);

        Console.WriteLine($"[{Size}: {html.Length:N0} characters of HTML]");
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    [Benchmark]
    public async Task<string> Page() =>
        await _client.GetStringAsync("/page/" + Size);
}
