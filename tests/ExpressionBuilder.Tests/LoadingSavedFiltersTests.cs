namespace TopMarksDevelopment.ExpressionBuilder.Tests;

using System.Diagnostics;
using System.Text.Json;

/// <summary>
/// A saved filter names its operations as text, so they have to be found again when it is loaded -
/// including in an app that has not used any of them yet.
/// </summary>
public class LoadingSavedFiltersTests
{
    [Fact(
        DisplayName = "A saved filter works in a fresh process, before any operation has been used"
    )]
    public async Task SavedFilterLoadsBeforeOperationsAreUsed()
    {
        // Between, Contains, IsNotNull and In: one from each family of operation
        var filter = new Filter<Product>();
        filter
            .Between(x => x.Id, 2, 5)
            .And()
            .IsNotNull(x => x.Name)
            .And()
            .Contains(x => x.Name, "r")
            .And()
            .In(x => x.Id, [2, 3, 4, 6]);

        var (exitCode, output, error) = await RunProbe(
            JsonSerializer.Serialize(filter)
        );

        Assert.True(exitCode == 0, error);

        // The probe has the people: 1 Alan, 2 Barbara, 3 Bob, 4 Brian, 5 (no name), 6 Bella
        Assert.Equal("2,4", output.Trim());
    }

    static async Task<(int ExitCode, string Output, string Error)> RunProbe(
        string input
    )
    {
        var probe = FindProbe();

        using var process = Process.Start(
            new ProcessStartInfo("dotnet", $"\"{probe}\"")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        )!;

        await process.StandardInput.WriteAsync(input);
        process.StandardInput.Close();

        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (process.ExitCode, await output, await error);
    }

    /// <summary>
    /// The probe is built next to this project (see the project reference that guarantees it is
    /// built first), in the same configuration and framework.
    /// </summary>
    static string FindProbe()
    {
        var output = new DirectoryInfo(
            AppContext.BaseDirectory.TrimEnd('/', '\\')
        );
        var framework = output.Name;
        var configuration = output.Parent!.Name;
        var tests = output.Parent.Parent!.Parent!.Parent!;

        var probe = Path.Combine(
            tests.FullName,
            "ExpressionBuilder.Tests.Probe",
            "bin",
            configuration,
            framework,
            "ExpressionBuilder.Tests.Probe.dll"
        );

        Assert.True(
            File.Exists(probe),
            $"The probe app was not built: {probe}"
        );

        return probe;
    }
}
