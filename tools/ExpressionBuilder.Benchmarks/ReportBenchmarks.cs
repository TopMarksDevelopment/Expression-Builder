namespace ExpressionBuilder.Benchmarks;

using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// A report is for a class chosen at run time, so the code that turns a posted filter into an expression cannot name the
/// class. Three ways of doing that, from the same checked posted filter: the way it is done now (the class is a type
/// argument), a reflection call on every request, and a call that was closed over the class once, when the class was
/// registered.
/// </summary>
[MemoryDiagnoser]
public class NonGenericBuildBenchmarks
{
    static readonly MethodInfo ToExpressionOf = typeof(IFilter).GetMethod(
        nameof(IFilter.ToExpression)
    )!;

    FilterBuilderOptions _options = null!;
    FilterSchema _schema = null!;
    string _json = "";
    ICollection<IFilterItem> _items = null!;
    Func<ICollection<IFilterItem>, LambdaExpression> _closed = null!;
    Type _type = typeof(Large);

    [Params(3, 20, 100)]
    public int Conditions { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _options = new();
        _schema = FilterSchema.For<Large>(_options);
        _json = FilterBuilderModel
            .FromFilter(FilterShapes.Build(Conditions), _options)
            .Json!;
        _items = new FilterCompiler(_schema, _options).Compile(_json).Items!;

        // What a registration of the class would keep: made when the class is known, used for every request
        _closed = items => new Filter<Large> { Items = items }.ToExpression();
    }

    /// <summary>Just making the filter from the checked items and then the expression, with the class as a type argument.</summary>
    [Benchmark(Baseline = true)]
    public object Typed() =>
        new Filter<Large> { Items = _items }.ToExpression();

    /// <summary>The same, through a delegate that was made when the class was registered.</summary>
    [Benchmark]
    public object ClosedWhenRegistered() => _closed(_items);

    /// <summary>The same, finding the generic types and the method on every request.</summary>
    [Benchmark]
    public object ReflectionEachRequest()
    {
        var filter = (IFilter)
            Activator.CreateInstance(typeof(Filter<>).MakeGenericType(_type))!;

        foreach (var item in _items)
            filter.Items.Add(item);

        return ToExpressionOf.MakeGenericMethod(_type).Invoke(filter, null)!;
    }

    /// <summary>A whole post as it is now: the schema, the check, the filter and its expression.</summary>
    [Benchmark]
    public object WholePostTyped() =>
        new FilterBuilderModel { Json = _json }
            .Build<Large>(_options)
            .Filter!.ToExpression();

    /// <summary>A whole post for a class known only as a <see cref="Type"/>, with reflection on every request.</summary>
    [Benchmark]
    public object WholePostReflection()
    {
        var schema = FilterSchema.For(_type, _options);
        var items = new FilterCompiler(schema, _options).Compile(_json).Items!;
        var filter = (IFilter)
            Activator.CreateInstance(typeof(Filter<>).MakeGenericType(_type))!;

        foreach (var item in items)
            filter.Items.Add(item);

        return ToExpressionOf.MakeGenericMethod(_type).Invoke(filter, null)!;
    }
}

/// <summary>
/// Putting a report together from what the user chose: the columns, the order and the page. The expression is built; the
/// query is not run (it is over an empty list, so nothing here depends on a database).
/// </summary>
[MemoryDiagnoser]
public class ReportComposeBenchmarks
{
    static readonly string[] Columns =
    [
        "Reference",
        "Placed",
        "Total",
        "Status",
        "Customer.Name",
        "Id",
        "Shipped",
        "Tax",
        "Discount",
        "Weight",
        "Priority",
        "Gift",
        "Channel",
        "Currency",
        "Carrier",
        "Tracking",
        "Department",
        "Customer.Country",
        "ShipTo.City",
        "ShipTo.Postcode",
    ];

    static readonly (string, bool)[] SortChoices =
    [
        ("Placed", true),
        ("Total", false),
        ("Reference", false),
    ];

    readonly IQueryable<ReportOrder> _source =
        new List<ReportOrder>().AsQueryable();
    readonly Expression<Func<ReportOrder, bool>> _filter = ReportSpike.Filter(
        1
    );
    string[] _columns = [];
    (string, bool)[] _sort = [];

    [Params(5, 20)]
    public int ColumnCount { get; set; }

    [Params(1, 3)]
    public int SortCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _columns = [.. Columns.Take(ColumnCount)];
        _sort = [.. SortChoices.Take(SortCount)];
    }

    /// <summary><c>x => new object[] { x.A, x.B.C, ... }</c></summary>
    [Benchmark]
    public object Projection() =>
        ReportSpike.ObjectArray<ReportOrder>(_columns, false);

    /// <summary>The same, with a null test before each related object.</summary>
    [Benchmark]
    public object ProjectionNullGuarded() =>
        ReportSpike.ObjectArray<ReportOrder>(_columns, true);

    /// <summary>The user's filter, the order, the tie-break, the page and the projection, as one query.</summary>
    [Benchmark(Baseline = true)]
    public object Compose() =>
        ReportSpike.Select(
            ReportSpike.OrderAndPage(
                _source.Where(_filter),
                _sort,
                "Id",
                100,
                50,
                true
            ),
            ReportSpike.ObjectArray<ReportOrder>(_columns, false)
        );
}

/// <summary>
/// Finding the columns and the order the user chose in the schema, which is what stops a column the user may not see from
/// being used: every path goes through the same lookup as a filter's.
/// </summary>
[MemoryDiagnoser]
public class ResolveColumnsBenchmarks
{
    FilterSchema _schema = null!;
    string[] _paths = [];

    /// <summary>Columns, plus the order, chosen from the end of the list: the slowest place to find one.</summary>
    [Params(5, 20, 23)]
    public int Paths { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _schema = FilterSchema.For<Large>();
        _paths = [.. ReportSpike.ScalarPaths(typeof(Large)).TakeLast(Paths)];
    }

    [Benchmark]
    public int Resolve()
    {
        var found = 0;

        foreach (var path in _paths)
            if (_schema.FindField(_schema.Root, path) != null)
                found++;

        return found;
    }
}

/// <summary>
/// What a hook that decides, for each property, whether the current user may see it adds to making a schema. The hook is
/// not in the library yet; this is the work such a hook would do on its own (a lookup of the rule for a property, and of
/// the permission among the user's), for every property of every class in the schema of <see cref="Large"/>.
/// </summary>
[MemoryDiagnoser]
public class AuthorisationHookBenchmarks
{
    PropertyInfo[] _properties = [];
    Dictionary<PropertyInfo, string> _rules = [];
    HashSet<string> _permissions = [];
    Func<PropertyInfo, bool> _hook = null!;

    [GlobalSetup]
    public void Setup()
    {
        _properties =
        [
            .. new[]
            {
                typeof(Large),
                typeof(Address),
                typeof(Company),
                typeof(Order),
                typeof(OrderLine),
                typeof(Contact),
            }.SelectMany(t => t.GetProperties()),
        ];

        // A dozen properties need a permission; the user has some of them
        _rules = _properties
            .Where((_, i) => i % 11 == 0)
            .ToDictionary(p => p, p => "view:" + p.Name);
        _permissions = [.. _rules.Values.Where((_, i) => i % 2 == 0)];

        _hook = p =>
            !_rules.TryGetValue(p, out var permission)
            || _permissions.Contains(permission);

        Console.WriteLine(
            $"[{_properties.Length} properties, {_rules.Count} with a rule]"
        );
    }

    [Benchmark(Baseline = true)]
    public FilterSchema SchemaWithoutHook() => FilterSchema.For<Large>();

    /// <summary>Only the hook, called once for each property.</summary>
    [Benchmark]
    public int HookAlone()
    {
        var allowed = 0;

        foreach (var property in _properties)
            if (_hook(property))
                allowed++;

        return allowed;
    }
}
