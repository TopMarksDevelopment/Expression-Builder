namespace ExpressionBuilder.Benchmarks;

using System.Diagnostics;
using System.Text;
using BenchmarkDotNet.Environments;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using TopMarksDevelopment.ExpressionBuilder;

public class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime Created { get; set; }
}

public sealed class ItemContext(DbContextOptions<ItemContext> options)
    : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();
}

/// <summary>
/// What values-as-parameters saves: the same number of queries that differ only in their values, run through Entity
/// Framework Core with the old behaviour (constants, <c>InlineValues()</c>) and the new (parameters).
/// </summary>
/// <remarks>
/// Not a BenchmarkDotNet benchmark, because the thing measured is a cache that fills up as the queries are made. Each
/// run has its own query cache, made like EF's own (a size limit of 10,240), so the two modes cannot see each other's
/// entries. Run it with <c>dotnet run -c Release -- ef</c>.
/// </remarks>
static class EfExperiment
{
    const int Rows = 1_000;

    enum Mode
    {
        Constants,
        Parameters,
    }

    sealed record Result(
        Mode Mode,
        string Pattern,
        int Queries,
        double TotalMs,
        int CacheEntries,
        long RetainedKb
    );

    public static void Run()
    {
        var report = new StringBuilder();

        report.AppendLine(
            string.Join(
                Environment.NewLine,
                HostEnvironmentInfo.GetCurrent().ToFormattedString()
            )
        );
        report.AppendLine(
            $"Entity Framework Core {typeof(DbContext).Assembly.GetName().Version}, SQLite in memory, {Rows:N0} rows, "
                + "a new DbContext for each query, two conditions (a text and a number) in each."
        );
        report.AppendLine();
        report.AppendLine(
            "| Values | Queries | Mode | Total | Per query | Query-cache entries | Memory kept |"
        );
        report.AppendLine("| --- | ---: | --- | ---: | ---: | ---: | ---: |");

        // Warm up everything that is not about the values (JIT, EF's model and service provider)
        foreach (var mode in Enum.GetValues<Mode>())
            Measure(mode, "different", 50);

        foreach (
            var (queries, repeats) in new[]
            {
                (1, 15),
                (10, 9),
                (100, 5),
                (1_000, 3),
                (12_000, 1),
            }
        )
        {
            var results = new Dictionary<Mode, List<Result>>
            {
                [Mode.Constants] = [],
                [Mode.Parameters] = [],
            };

            // Alternate, so neither mode always runs on a warmer machine
            for (var r = 0; r < repeats; r++)
                foreach (
                    var mode in r % 2 == 0
                        ? new[] { Mode.Constants, Mode.Parameters }
                        : [Mode.Parameters, Mode.Constants]
                )
                    results[mode].Add(Measure(mode, "different", queries));

            foreach (var mode in Enum.GetValues<Mode>())
                Add(report, results[mode], repeats);
        }

        // The same value every time: both modes find their query in the cache, so this is the cost of the parameters alone
        foreach (var mode in Enum.GetValues<Mode>())
        {
            var runs = Enumerable
                .Range(0, 3)
                .Select(_ => Measure(mode, "same", 1_000))
                .ToList();

            Add(report, runs, 3);
        }

        Console.WriteLine();
        Console.WriteLine(report);

        Directory.CreateDirectory("BenchmarkDotNet.Artifacts");
        File.WriteAllText(
            Path.Combine("BenchmarkDotNet.Artifacts", "ef-results.md"),
            report.ToString()
        );
    }

    static void Add(StringBuilder report, List<Result> runs, int repeats)
    {
        var median = runs.OrderBy(r => r.TotalMs).ElementAt(runs.Count / 2);

        report.AppendLine(
            $"| {median.Pattern} | {median.Queries:N0} | {median.Mode} | {Time(median.TotalMs)} | "
                + $"{Time(median.TotalMs / median.Queries)} | {median.CacheEntries:N0} | {median.RetainedKb:N0} KB |"
                + (repeats > 1 ? $" <!-- median of {repeats} -->" : "")
        );
    }

    static string Time(double ms) =>
        ms >= 1000 ? $"{ms / 1000:N2} s"
        : ms >= 1 ? $"{ms:N1} ms"
        : $"{ms * 1000:N0} µs";

    static Result Measure(Mode mode, string pattern, int queries)
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        // Made as EF makes its own: a limit of 10,240 units, and 5% of them dropped when it is reached
        using var cache = new MemoryCache(
            new MemoryCacheOptions
            {
                SizeLimit = 10_240,
                CompactionPercentage = 0.05,
            }
        );
        var options = new DbContextOptionsBuilder<ItemContext>()
            .UseSqlite(connection)
            .UseMemoryCache(cache)
            // A new cache means a new internal service provider, which is exactly the isolation wanted here
            .ConfigureWarnings(w =>
                w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)
            )
            .Options;

        using (var setup = new ItemContext(options))
        {
            setup.Database.EnsureCreated();
            setup.Items.AddRange(
                Enumerable
                    .Range(1, Rows)
                    .Select(i => new Item
                    {
                        Id = i,
                        Name = $"item{i}",
                        Quantity = i % 50,
                        Price = i,
                        Created = new DateTime(2024, 1, 1).AddDays(i),
                    })
            );
            setup.SaveChanges();
        }

        // Anything EF caches for a first query of any kind is there before we start counting
        for (var w = 0; w < 3; w++)
            Query(options, mode, -w - 1, w);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var entriesBefore = cache.Count;
        var memoryBefore = GC.GetTotalMemory(true);
        var clock = Stopwatch.StartNew();

        for (var i = 1; i <= queries; i++)
            Query(
                options,
                mode,
                pattern == "same" ? 1 : i,
                pattern == "same" ? 1 : i % 50
            );

        clock.Stop();

        var memoryAfter = GC.GetTotalMemory(true);

        return new Result(
            mode,
            pattern,
            queries,
            clock.Elapsed.TotalMilliseconds,
            cache.Count - entriesBefore,
            Math.Max(0, memoryAfter - memoryBefore) / 1024
        );
    }

    static void Query(
        DbContextOptions<ItemContext> options,
        Mode mode,
        int name,
        int quantity
    )
    {
        var filter = new Filter<Item>();

        filter.Add("Name", Operation.Equal, [$"item{name}"]).And();
        filter.Add("Quantity", Operation.GreaterThanOrEqual, [quantity]);

        var expression = filter.ToExpression();

        if (mode == Mode.Constants)
            expression = expression.InlineValues();

        using var context = new ItemContext(options);

        _ = context.Items.Where(expression).ToList();
    }
}
