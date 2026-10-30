namespace ExpressionBuilder.Benchmarks;

using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using BenchmarkDotNet.Environments;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using TopMarksDevelopment.ExpressionBuilder;

/// <summary>
/// What the report builder design (docs/architecture/report-builder.md) rests on, measured through Entity Framework Core
/// and SQLite: does a projection to columns chosen at run time translate, what does each way of shaping the result cost
/// a request, and what does a new set of columns cost the first time.
/// </summary>
/// <remarks>
/// Not a BenchmarkDotNet benchmark, for the reason <see cref="EfExperiment"/> is not: what is measured includes a cache
/// that fills as queries are made. Run it with <c>dotnet run -c Release -- report</c>.
/// </remarks>
static class ReportExperiment
{
    const int Rows = 10_000;
    const int PageSize = 50;

    static readonly string[] Six =
    [
        "Reference",
        "Placed",
        "Total",
        "Status",
        "Customer.Name",
        "ShipTo.City",
    ];

    static readonly string[] Twenty =
    [
        "Id",
        "Reference",
        "Placed",
        "Shipped",
        "Total",
        "Tax",
        "Discount",
        "Weight",
        "Status",
        "Priority",
        "Gift",
        "Channel",
        "Currency",
        "Carrier",
        "Tracking",
        "Department",
        "Customer.Name",
        "Customer.Country",
        "ShipTo.City",
        "ShipTo.Postcode",
    ];

    static readonly (string Path, bool Descending)[] Sort = [("Placed", true)];

    sealed class Env : IDisposable
    {
        readonly SqliteConnection _connection;

        Env(
            SqliteConnection connection,
            MemoryCache cache,
            DbContextOptions<ReportDb> options
        )
        {
            _connection = connection;
            Cache = cache;
            Options = options;
        }

        public MemoryCache Cache { get; }

        public DbContextOptions<ReportDb> Options { get; }

        public ReportDb Open() => new(Options);

        public static Env Create(bool indexed = true)
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            // Made as EF makes its own: a limit of 10,240 units, and 5% of them dropped when it is reached
            var cache = new MemoryCache(
                new MemoryCacheOptions
                {
                    SizeLimit = 10_240,
                    CompactionPercentage = 0.05,
                }
            );
            var options = new DbContextOptionsBuilder<ReportDb>()
                .UseSqlite(connection)
                .UseMemoryCache(cache)
                .ConfigureWarnings(w =>
                    w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)
                )
                .Options;

            var env = new Env(connection, cache, options);

            using var db = env.Open();
            db.Database.EnsureCreated();

            // The order a report is usually in. Without it every request sorts all the rows
            if (indexed)
                db.Database.ExecuteSqlRaw(
                    "CREATE INDEX \"IX_Orders_Placed_Id\" ON \"Orders\" (\"Placed\" DESC, \"Id\")"
                );

            var notes = new string('n', 200);

            db.Customers.AddRange(
                Enumerable
                    .Range(1, 500)
                    .Select(i => new ReportCustomer
                    {
                        Id = i,
                        Name = $"Customer {i}",
                        Email = $"c{i}@example.com",
                        Region = (Region)(i % 4),
                        Country = i % 3 == 0 ? "UK" : "DE",
                        CreditLimit = i * 100,
                        Notes = notes,
                    })
            );
            db.Orders.AddRange(
                Enumerable
                    .Range(1, Rows)
                    .Select(i => new ReportOrder
                    {
                        Id = i,
                        Reference = $"ORD-{i:D6}",
                        Placed = new DateTime(2024, 1, 1).AddDays(i % 700),
                        Shipped =
                            i % 5 == 0
                                ? null
                                : new DateTime(2024, 1, 3).AddDays(i % 700),
                        Total = 10 + i * 37 % 990,
                        Tax = i % 40,
                        Discount = i % 15,
                        Weight = i % 90 / 3.0,
                        Status = (OrderStatus)(i % 5),
                        Priority = i % 4,
                        Gift = i % 7 == 0,
                        Channel = i % 2 == 0 ? "web" : "phone",
                        Currency = "GBP",
                        Carrier = i % 3 == 0 ? "RM" : "DPD",
                        Tracking = i % 6 == 0 ? null : $"TRK{i}",
                        Department = $"D{i % 12}",
                        CostCentre = $"C{i % 30}",
                        ApprovedBy = i % 4 == 0 ? null : "sam",
                        ApprovedOn =
                            i % 4 == 0 ? null : new DateTime(2024, 2, 1),
                        CreatedBy = "import",
                        UpdatedOn = null,
                        Notes = notes,
                        CustomerId = 1 + i % 500,
                        ShipTo = new ReportAddress
                        {
                            Line1 = $"{i} High Street",
                            City = $"Town{i % 80}",
                            Postcode = $"AB{i % 99} 1XY",
                            Country = "UK",
                        },
                    })
            );
            db.SaveChanges();

            return env;
        }

        public void Dispose()
        {
            Cache.Dispose();
            _connection.Dispose();
        }
    }

    /// <summary>One way of shaping the rows of a report: a query, run to the end, giving a row of cells for each row.</summary>
    sealed record Variant(
        string Name,
        string[] Columns,
        Func<
            ReportDb,
            Expression<Func<ReportOrder, bool>>,
            int,
            List<object?[]>
        > Run
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
            $"Entity Framework Core {typeof(DbContext).Assembly.GetName().Version}, SQLite in memory, {Rows:N0} orders (35 columns, a related customer, an owned address), "
                + $"a new DbContext for each query, pages of {PageSize}."
        );
        report.AppendLine();

        Translation(report);
        Warm(report, indexed: true, queries: 300, repeats: 9);
        Warm(report, indexed: false, queries: 40, repeats: 5);
        Cold(report);
        Paging(report);

        Console.WriteLine();
        Console.WriteLine(report);

        Directory.CreateDirectory("BenchmarkDotNet.Artifacts");
        File.WriteAllText(
            Path.Combine("BenchmarkDotNet.Artifacts", "report-results.md"),
            report.ToString()
        );
    }

    #region Variants

    static List<object?[]> Read(IQueryable query)
    {
        var rows = new List<object?[]>();

        foreach (var row in (IEnumerable)query)
            rows.Add((object?[])row);

        return rows;
    }

    static IQueryable<ReportOrder> Page(
        ReportDb db,
        Expression<Func<ReportOrder, bool>> filter,
        int page,
        bool include = false
    )
    {
        var source = db.Orders.AsNoTracking();

        if (include)
            source = source.Include(o => o.Customer);

        return ReportSpike.OrderAndPage(
            source.Where(filter),
            Sort,
            "Id",
            page * PageSize,
            PageSize,
            true
        );
    }

    /// <summary>Read the whole row of the class, then pick the columns: what you get without a projection.</summary>
    static Variant Entity(string[] columns)
    {
        var getters = ReportSpike.Getters<ReportOrder>(columns);

        return new(
            "Whole entity, then read the columns",
            columns,
            (db, filter, page) =>
                Page(db, filter, page, include: true)
                    .ToList()
                    .ConvertAll(o => getters.Select(g => g(o)).ToArray())
        );
    }

    /// <summary>A compile-time anonymous type: not possible for columns chosen at run time; the best case.</summary>
    static Variant Anonymous() =>
        new(
            "Anonymous type (compile time)",
            Six,
            (db, filter, page) =>
                Page(db, filter, page)
                    .Select(o => new
                    {
                        o.Reference,
                        o.Placed,
                        o.Total,
                        o.Status,
                        CustomerName = o.Customer.Name,
                        ShipToCity = o.ShipTo.City,
                    })
                    .ToList()
                    .ConvertAll(r =>
                        new object?[]
                        {
                            r.Reference,
                            r.Placed,
                            r.Total,
                            r.Status,
                            r.CustomerName,
                            r.ShipToCity,
                        }
                    )
        );

    /// <summary>A result class the app registers: one shape for the root, whatever is chosen.</summary>
    static Variant Typed() =>
        new(
            "Registered result class (compile time)",
            Six,
            (db, filter, page) =>
                Page(db, filter, page)
                    .Select(o => new OrderRow
                    {
                        Reference = o.Reference,
                        Placed = o.Placed,
                        Total = o.Total,
                        Status = o.Status,
                        CustomerName = o.Customer.Name,
                        ShipToCity = o.ShipTo.City,
                    })
                    .ToList()
                    .ConvertAll(r =>
                        new object?[]
                        {
                            r.Reference,
                            r.Placed,
                            r.Total,
                            r.Status,
                            r.CustomerName,
                            r.ShipToCity,
                        }
                    )
        );

    /// <summary>An expression built for the columns of this request: <c>new object[] { ... }</c>.</summary>
    static Variant ObjectArray(string[] columns, bool guarded) =>
        new(
            guarded
                ? "object[] built per request, null-guarded"
                : "object[] built per request",
            columns,
            (db, filter, page) =>
                Read(
                    ReportSpike.Select(
                        Page(db, filter, page),
                        ReportSpike.ObjectArray<ReportOrder>(columns, guarded)
                    )
                )
        );

    /// <summary>The same, then each row made into a dictionary of column name to value.</summary>
    static Variant AsDictionary(string[] columns)
    {
        return new(
            "object[] built per request, then a dictionary per row",
            columns,
            (db, filter, page) =>
            {
                var rows = ObjectArray(columns, false).Run(db, filter, page);
                var made = new List<object?[]>(rows.Count);

                foreach (var row in rows)
                {
                    var dictionary = new Dictionary<string, object?>(
                        columns.Length
                    );

                    for (var i = 0; i < columns.Length; i++)
                        dictionary[columns[i]] = row[i];

                    // What a renderer would then do: read each cell back
                    made.Add([.. dictionary.Values]);
                }

                return made;
            }
        );
    }

    /// <summary>A class made for this set of columns, once, and kept.</summary>
    static Variant Emitted(string[] columns)
    {
        var (selector, readers) = ReportSpike.Emitted<ReportOrder>(columns);

        return new(
            "Generated class, made once per set of columns",
            columns,
            (db, filter, page) =>
            {
                var rows = new List<object?[]>();

                foreach (
                    var row in (IEnumerable)
                        ReportSpike.Select(Page(db, filter, page), selector)
                )
                    rows.Add(readers.Select(r => r(row)).ToArray());

                return rows;
            }
        );
    }

    #endregion

    #region Translation

    static void Translation(StringBuilder report)
    {
        report.AppendLine("## Does it translate?");
        report.AppendLine();
        report.AppendLine("| Query | Result |");
        report.AppendLine("| --- | --- |");

        using var env = Env.Create();

        string Try(Func<string> run)
        {
            try
            {
                return run();
            }
            catch (Exception ex)
            {
                var message = ex.Message.ReplaceLineEndings(" ");

                return $"**fails**: {ex.GetType().Name}: {(message.Length > 150 ? message[..150] + "…" : message)}";
            }
        }

        void Check(string name, Func<ReportDb, string> run) =>
            report.AppendLine(
                $"| {name} | {Try(() =>
                {
                    using var db = env.Open();

                    return run(db);
                })} |"
            );

        string Count(IQueryable query)
        {
            var n = 0;

            foreach (var _ in (IEnumerable)query)
                n++;

            return $"translates, {n} rows";
        }

        var filter = ReportSpike.Filter(1);

        Check(
            "Select `new object[] { ... }` of 6 columns, filter, order, skip, take",
            db =>
                Count(
                    ReportSpike.Select(
                        Page(db, filter, 2),
                        ReportSpike.ObjectArray<ReportOrder>(Six, false)
                    )
                )
        );
        Check(
            "The same, null-guarded (`x.Customer == null ? null : x.Customer.Name`)",
            db =>
                Count(
                    ReportSpike.Select(
                        Page(db, filter, 2),
                        ReportSpike.ObjectArray<ReportOrder>(Six, true)
                    )
                )
        );
        Check(
            "Select into a class generated for the columns (`new Row { C0 = ... }`)",
            db => $"translates, {Emitted(Six).Run(db, filter, 2).Count} rows"
        );
        Check(
            "Select into `new Dictionary<string, object?> { { \"a\", x.A }, ... }`",
            db =>
            {
                var x = Expression.Parameter(typeof(ReportOrder), "x");
                var add = typeof(Dictionary<string, object?>).GetMethod("Add")!;
                var selector = Expression.Lambda(
                    Expression.ListInit(
                        Expression.New(typeof(Dictionary<string, object?>)),
                        Six.Take(3)
                            .Select(c =>
                                Expression.ElementInit(
                                    add,
                                    Expression.Constant(c),
                                    Expression.Convert(
                                        Expression.Property(x, c),
                                        typeof(object)
                                    )
                                )
                            )
                    ),
                    x
                );

                return Count(ReportSpike.Select(Page(db, filter, 0), selector));
            }
        );
        Check(
            "Group by `new object[] { x.Status }`, count each group",
            db => Count(GroupCount(db, ObjectArrayKey()))
        );
        Check(
            "Group by a ValueTuple of two columns, count each group",
            db => Count(GroupCount(db, TupleKey()))
        );
        Check(
            "Group by a generated class (`new Key { C0 = ..., C1 = ... }`), count each group",
            db => Count(GroupCount(db, EmittedKey()))
        );
        Check(
            "Group by an anonymous type (compile time), count each group",
            db =>
                $"translates, {db.Orders.GroupBy(o => new { o.Status, o.Channel }).Select(g => g.Count()).ToList().Count} rows"
        );

        // In memory, with a related object that is missing
        var orders = new List<ReportOrder>
        {
            new() { Reference = "a", Customer = null! },
        };

        report.AppendLine(
            $"| **In memory**, `x.Customer.Name` with no customer: not guarded | {Try(() => $"{Read(ReportSpike.Select(orders.AsQueryable(), ReportSpike.ObjectArray<ReportOrder>(["Customer.Name"], false))).Count} rows")} |"
        );
        report.AppendLine(
            $"| **In memory**, the same, null-guarded | {Try(() => $"{Read(ReportSpike.Select(orders.AsQueryable(), ReportSpike.ObjectArray<ReportOrder>(["Customer.Name"], true))).Count} rows")} |"
        );
        report.AppendLine();

        using (var db = env.Open())
        {
            var plain = ReportSpike.Select(
                Page(db, filter, 2),
                ReportSpike.ObjectArray<ReportOrder>(Six, false)
            );
            var guarded = ReportSpike.Select(
                Page(db, filter, 2),
                ReportSpike.ObjectArray<ReportOrder>(Six, true)
            );

            report.AppendLine("SQL, six columns, not guarded:");
            report.AppendLine();
            report.AppendLine("```sql");
            report.AppendLine(Sql(plain));
            report.AppendLine("```");
            report.AppendLine();
            report.AppendLine("SQL, six columns, null-guarded:");
            report.AppendLine();
            report.AppendLine("```sql");
            report.AppendLine(Sql(guarded));
            report.AppendLine("```");
            report.AppendLine();
        }
    }

    static string Sql(IQueryable query) =>
        EntityFrameworkQueryableExtensions.ToQueryString(query);

    static LambdaExpression ObjectArrayKey()
    {
        var x = Expression.Parameter(typeof(ReportOrder), "x");

        return Expression.Lambda(
            Expression.NewArrayInit(
                typeof(object),
                Expression.Convert(
                    Expression.Property(x, "Status"),
                    typeof(object)
                )
            ),
            x
        );
    }

    static LambdaExpression TupleKey()
    {
        var x = Expression.Parameter(typeof(ReportOrder), "x");
        var type = typeof(ValueTuple<OrderStatus, string>);

        return Expression.Lambda(
            Expression.New(
                type.GetConstructor([typeof(OrderStatus), typeof(string)])!,
                Expression.Property(x, "Status"),
                Expression.Property(x, "Channel")
            ),
            x
        );
    }

    static LambdaExpression EmittedKey()
    {
        var (selector, _) = ReportSpike.Emitted<ReportOrder>([
            "Status",
            "Channel",
        ]);

        return selector;
    }

    /// <summary><c>db.Orders.GroupBy(key).Select(g => g.Count())</c> for a key chosen at run time.</summary>
    static IQueryable GroupCount(ReportDb db, LambdaExpression key)
    {
        var grouping = typeof(IGrouping<,>).MakeGenericType(
            key.ReturnType,
            typeof(ReportOrder)
        );
        IQueryable<ReportOrder> orders = db.Orders;
        var grouped = orders.Provider.CreateQuery(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.GroupBy),
                [typeof(ReportOrder), key.ReturnType],
                orders.Expression,
                Expression.Quote(key)
            )
        );
        var g = Expression.Parameter(grouping, "g");
        var count = typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
            .MakeGenericMethod(typeof(ReportOrder));

        // The result is boxed in an object[] so that Read() can be used on every query
        return grouped.Provider.CreateQuery(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                [grouping, typeof(object[])],
                grouped.Expression,
                Expression.Quote(
                    Expression.Lambda(
                        Expression.NewArrayInit(
                            typeof(object),
                            Expression.Convert(
                                Expression.Call(count, g),
                                typeof(object)
                            )
                        ),
                        g
                    )
                )
            )
        );
    }

    #endregion

    #region Warm

    static void Warm(
        StringBuilder report,
        bool indexed,
        int queries,
        int repeats
    )
    {
        report.AppendLine(
            indexed
                ? "## A request for a page, when the shape of the query is already known"
                : "## The same, when the database has no index for the order and sorts all 10,000 rows on every request"
        );
        report.AppendLine();
        report.AppendLine(
            $"{queries} requests for a page of {PageSize}, each with different values in its filter (`Placed >=` and `Priority >=`), "
                + "ordered by Placed descending then Id. Every request builds its expression again (the object[] and dictionary rows build the "
                + "projection each time; the generated class is made once). Each ends with a list of rows of cells. "
                + $"Median of {repeats} runs, the variants in a different order each time."
        );
        report.AppendLine();
        report.AppendLine(
            "| Columns | How the rows are shaped | Per request (median) | Lowest to highest of the runs | Allocated per request |"
        );
        report.AppendLine("| ---: | --- | ---: | ---: | ---: |");

        using var env = Env.Create(indexed);

        var variants = new List<Variant>
        {
            ObjectArray(["Id"], false),
            Entity(Six),
            Anonymous(),
            Typed(),
            ObjectArray(Six, false),
            ObjectArray(Six, true),
            AsDictionary(Six),
            Emitted(Six),
            Entity(Twenty),
            ObjectArray(Twenty, false),
            ObjectArray(Twenty, true),
            AsDictionary(Twenty),
            Emitted(Twenty),
        };

        // Everything must agree on what the rows are before their speed is compared
        var expected = Verify(variants[1], env);

        foreach (var variant in variants)
            Verify(variant, env, expected, variants[1]);

        var results = variants.ToDictionary(
            v => v,
            _ => new List<(double Us, double Bytes)>()
        );
        var random = new Random(7);

        for (var r = 0; r < repeats; r++)
            foreach (var variant in variants.OrderBy(_ => random.Next()))
            {
                for (var i = 0; i < 10; i++)
                {
                    using var db = env.Open();
                    variant.Run(db, ReportSpike.Filter(-i - 1), i);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var bytes = GC.GetAllocatedBytesForCurrentThread();
                var clock = Stopwatch.StartNew();

                for (var i = 1; i <= queries; i++)
                {
                    using var db = env.Open();
                    variant.Run(db, ReportSpike.Filter(i), i % 10);
                }

                clock.Stop();

                results[variant]
                    .Add(
                        (
                            clock.Elapsed.TotalMilliseconds * 1000 / queries,
                            (GC.GetAllocatedBytesForCurrentThread() - bytes)
                                / (double)queries
                        )
                    );
            }

        foreach (var variant in variants)
        {
            var ordered = results[variant].OrderBy(x => x.Us).ToList();
            var median = ordered[repeats / 2];

            report.AppendLine(
                $"| {variant.Columns.Length} | {variant.Name} | {median.Us:N0} µs | {ordered[0].Us:N0} to {ordered[^1].Us:N0} µs | {median.Bytes / 1024:N0} KB |"
            );
        }

        report.AppendLine();
    }

    /// <summary>Runs a variant once and checks it returns the rows the first one did (the same references, in the same order).</summary>
    static string Verify(
        Variant variant,
        Env env,
        string? expected = null,
        Variant? reference = null
    )
    {
        using var db = env.Open();
        var rows = variant.Run(db, ReportSpike.Filter(3), 2);

        if (rows.Count != PageSize)
            throw new InvalidOperationException(
                $"{variant.Name} returned {rows.Count} rows."
            );

        // A variant without the reference column can only be held to the number of rows
        var at = Array.IndexOf(variant.Columns, "Reference");

        if (at < 0)
            return "";

        var references = string.Join(",", rows.Select(r => r[at]));

        if (expected != null && references != expected)
            throw new InvalidOperationException(
                $"{variant.Name} returned different rows from {reference!.Name}."
            );

        return references;
    }

    #endregion

    #region Cold

    static void Cold(StringBuilder report)
    {
        const int Sets = 40;

        report.AppendLine("## The first request for a new set of columns");
        report.AppendLine();
        report.AppendLine(
            $"{Sets} different sets of six columns chosen at random from the 35 that can be, each run twice (two different filters). "
                + "\"Building\" is making the projection (and, for the generated class, the class and the way to read it); \"first\" is the first "
                + "request, in which Entity Framework translates the query; \"second\" is the next, with other values in the filter. "
                + "Each variant has an Entity Framework query cache of its own."
        );
        report.AppendLine();
        report.AppendLine(
            "| How the rows are shaped | Building | First request | Second request | New query-cache entries per set |"
        );
        report.AppendLine("| --- | ---: | ---: | ---: | ---: |");

        var paths = ReportSpike.ScalarPaths(typeof(ReportOrder));
        var random = new Random(11);
        var sets = Enumerable
            .Range(0, Sets)
            .Select(_ => paths.OrderBy(_ => random.Next()).Take(6).ToArray())
            .ToList();

        foreach (
            var (name, make, buildOnly) in new (
                string,
                Func<string[], Variant>,
                Action<string[]>
            )[]
            {
                (
                    "object[] built per request",
                    c => ObjectArray(c, false),
                    c => ReportSpike.ObjectArray<ReportOrder>(c, false)
                ),
                (
                    "Generated class, made once per set",
                    Emitted,
                    c => Emitted(c)
                ),
            }
        )
        {
            using var env = Env.Create();

            // Anything EF caches for a first query of any kind is there before we start counting
            for (var w = 0; w < 3; w++)
            {
                var warm = make(sets[w]);
                using var db = env.Open();
                warm.Run(db, ReportSpike.Filter(w), 0);
            }

            var entries = env.Cache.Count;
            double build = 0,
                first = 0,
                second = 0;

            foreach (var set in sets.Skip(3))
            {
                var clock = Stopwatch.StartNew();

                buildOnly(set);

                clock.Stop();
                build += clock.Elapsed.TotalMilliseconds;

                var variant = make(set);

                clock.Restart();
                using (var db = env.Open())
                    variant.Run(db, ReportSpike.Filter(1), 0);
                clock.Stop();
                first += clock.Elapsed.TotalMilliseconds;

                clock.Restart();
                using (var db = env.Open())
                    variant.Run(db, ReportSpike.Filter(2), 0);
                clock.Stop();
                second += clock.Elapsed.TotalMilliseconds;
            }

            var n = Sets - 3;

            report.AppendLine(
                $"| {name} | {build / n * 1000:N0} µs | {first / n:N2} ms | {second / n * 1000:N0} µs | {(env.Cache.Count - entries) / (double)n:N1} |"
            );
        }

        report.AppendLine();
    }

    #endregion

    #region Paging

    static void Paging(StringBuilder report)
    {
        const int Queries = 400;

        report.AppendLine("## Page numbers: constants or variables");
        report.AppendLine();
        report.AppendLine(
            $"{Queries} requests that walk through 40 different pages of the same report (six columns, same filter), with the "
                + "page's Skip and Take written into the expression as constants, or as reads of a variable (which Entity Framework turns into a parameter)."
        );
        report.AppendLine();
        report.AppendLine(
            "| Skip and Take | Per request | New query-cache entries |"
        );
        report.AppendLine("| --- | ---: | ---: |");

        foreach (var asParameter in new[] { false, true })
        {
            using var env = Env.Create();

            IQueryable Build(ReportDb db, int page) =>
                ReportSpike.Select(
                    ReportSpike.OrderAndPage(
                        db.Orders.AsNoTracking().Where(ReportSpike.Filter(1)),
                        Sort,
                        "Id",
                        page * PageSize,
                        PageSize,
                        asParameter
                    ),
                    ReportSpike.ObjectArray<ReportOrder>(Six, false)
                );

            for (var w = 0; w < 3; w++)
                Read(Build(env.Open(), w));

            var entries = env.Cache.Count;
            var clock = Stopwatch.StartNew();

            for (var i = 0; i < Queries; i++)
                Read(Build(env.Open(), i % 40));

            clock.Stop();

            report.AppendLine(
                $"| {(asParameter ? "Variables" : "Constants")} | {clock.Elapsed.TotalMilliseconds * 1000 / Queries:N0} µs | {env.Cache.Count - entries} |"
            );
        }

        report.AppendLine();
    }

    #endregion
}
