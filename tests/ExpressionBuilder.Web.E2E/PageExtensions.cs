namespace ExpressionBuilder.Web.E2E;

using Microsoft.Playwright;

public static class PageExtensions
{
    /// <summary>
    /// Runs in every page before its own scripts: writes down what the Content-Security-Policy refused and any
    /// error a script raised, so a test can ask for them afterwards.
    /// </summary>
    public const string WatchScript = """
        window.__problems = [];
        document.addEventListener("securitypolicyviolation", (e) =>
            window.__problems.push("CSP: " + e.violatedDirective + " blocked " + (e.blockedURI || "inline")));
        window.addEventListener("error", (e) => window.__problems.push("error: " + e.message));
        window.addEventListener("unhandledrejection", (e) => window.__problems.push("rejection: " + e.reason));
        """;

    /// <summary>What went wrong in the page so far: policy violations and script errors.</summary>
    public static async Task<string[]> ProblemsAsync(this IPage page) =>
        await page.EvaluateAsync<string[]>("() => window.__problems ?? []");
}
