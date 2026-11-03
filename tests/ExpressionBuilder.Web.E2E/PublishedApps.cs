namespace ExpressionBuilder.Web.E2E;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Apps like yours: the packages are packed, installed into a new MVC app and a new Razor Pages app from a local feed
/// (not from this repository), published, and run in Production with a strict Content-Security-Policy. Anything that
/// only works from the sample app shows up here.
/// </summary>
public sealed class PublishedApps : IAsyncLifetime
{
    readonly string _work = Path.Combine(
        Path.GetTempPath(),
        "eb-consumers-" + Guid.NewGuid().ToString("N")[..8]
    );
    readonly List<Process> _running = [];

    /// <summary>The address of each app, by name (<c>Mvc</c>, <c>RazorPages</c>).</summary>
    public Dictionary<string, string> Urls { get; } = [];

    static string RepositoryRoot
    {
        get
        {
            var folder = new DirectoryInfo(AppContext.BaseDirectory);

            while (
                folder != null
                && !File.Exists(
                    Path.Combine(folder.FullName, "ExpressionBuilder.sln")
                )
            )
                folder = folder.Parent;

            return folder?.FullName
                ?? throw new InvalidOperationException(
                    "Could not find the repository."
                );
        }
    }

    public async Task InitializeAsync()
    {
        var feed = Path.Combine(_work, "feed");
        var packages = Path.Combine(_work, "packages");

        Directory.CreateDirectory(feed);

        // Every package, as they would be published
        await Dotnet(
            RepositoryRoot,
            $"pack ExpressionBuilder.sln -c Release -o \"{feed}\" --nologo"
        );

        var version = Path.GetFileNameWithoutExtension(
                Directory
                    .GetFiles(
                        feed,
                        "TopMarksDevelopment.ExpressionBuilder.Web.*.nupkg"
                    )
                    .Single()
            )
            .Replace("TopMarksDevelopment.ExpressionBuilder.Web.", "");

        // Copies, so building them leaves nothing in the repository
        CopyConsumers(
            Path.Combine(
                RepositoryRoot,
                "tests",
                "ExpressionBuilder.Web.E2E",
                "Consumers"
            ),
            Path.Combine(_work, "src")
        );

        // The feed first, nuget.org for everything else
        await File.WriteAllTextAsync(
            Path.Combine(_work, "src", "nuget.config"),
            $"""
            <configuration>
              <packageSources>
                <clear />
                <add key="local" value="{feed}" />
                <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
              </packageSources>
            </configuration>
            """
        );

        foreach (var app in new[] { "Mvc", "RazorPages" })
        {
            var source = Path.Combine(_work, "src", app);
            var output = Path.Combine(_work, "out", app);

            // A private package folder, so nothing cached from an earlier build is reused
            await Dotnet(
                source,
                $"publish -c Release -o \"{output}\" --nologo -p:EbVersion={version} -p:RestorePackagesPath=\"{packages}\""
            );

            Urls[app] = Start(output, $"Consumer{app}.dll");
        }

        await WaitUntilServingAsync();
    }

    public Task DisposeAsync()
    {
        foreach (var process in _running)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // Already gone
            }

            process.Dispose();
        }

        try
        {
            Directory.Delete(_work, recursive: true);
        }
        catch (IOException)
        {
            // Left in the temp folder; not worth failing a test run for
        }

        return Task.CompletedTask;
    }

    /// <summary>The apps are source next to the tests; copy them out of the way to build them.</summary>
    static void CopyConsumers(string from, string to)
    {
        foreach (
            var file in Directory.GetFiles(
                from,
                "*",
                SearchOption.AllDirectories
            )
        )
        {
            var relative = Path.GetRelativePath(from, file);

            // Not what an earlier build left behind
            if (
                relative
                    .Split(Path.DirectorySeparatorChar)
                    .Any(part => part is "bin" or "obj")
            )
                continue;

            var target = Path.Combine(to, relative);

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    static async Task Dotnet(string folder, string arguments)
    {
        using var process = Process.Start(
            new ProcessStartInfo("dotnet", arguments)
            {
                WorkingDirectory = folder,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment =
                {
                    ["DOTNET_NOLOGO"] = "1",
                    ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1",
                },
            }
        )!;

        var output = process.StandardOutput.ReadToEndAsync();
        var errors = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"dotnet {arguments} failed ({process.ExitCode}) in {folder}:\n{await output}\n{await errors}"
            );
    }

    string Start(string folder, string dll)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);

        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        listener.Stop();

        var process = Process.Start(
            new ProcessStartInfo(
                "dotnet",
                $"\"{dll}\" --urls http://127.0.0.1:{port}"
            )
            {
                WorkingDirectory = folder,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                // Not Development: static web assets must come from what was published
                Environment = { ["ASPNETCORE_ENVIRONMENT"] = "Production" },
            }
        )!;

        // Nobody reads this, so it must not fill up and stop the app
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        _running.Add(process);

        return $"http://127.0.0.1:{port}";
    }

    async Task WaitUntilServingAsync()
    {
        using var client = new HttpClient();

        foreach (var url in Urls.Values)
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    using var response = await client.GetAsync(url);

                    break;
                }
                catch (HttpRequestException) when (attempt < 60)
                {
                    await Task.Delay(500);
                }
            }
        }
    }
}
