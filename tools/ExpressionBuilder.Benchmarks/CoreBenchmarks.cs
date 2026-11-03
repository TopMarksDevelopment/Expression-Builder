namespace ExpressionBuilder.Benchmarks;

using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Extensions;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// Making a filter, turning it into an expression, and the extra pass that moves its values into variables.
/// </summary>
/// <remarks>
/// <c>ToExpression()</c> is the expression tree being built, followed by <c>ValueParameters.Parameterise</c> over the
/// finished tree. <c>TreeOnly</c> calls the private builder directly so the two can be told apart.
/// </remarks>
[MemoryDiagnoser]
public class FilterBuildBenchmarks
{
    static readonly MethodInfo BuildExpression =
        typeof(Filter<Large>).GetMethod(
            "BuildExpression",
            BindingFlags.Instance | BindingFlags.NonPublic
        )!;

    Filter<Large> _filter = null!;
    Expression<Func<Large, bool>> _constants = null!;
    Expression<Func<Large, bool>> _parameters = null!;
    Func<Expression<Func<Large, bool>>> _treeOnly = null!;

    /// <summary>Small is a search form, medium a saved search, large the most the web builder allows.</summary>
    [Params(3, 20, 100)]
    public int Conditions { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _filter = FilterShapes.Build(Conditions);
        _parameters = _filter.ToExpression();
        _constants = _parameters.InlineValues();
        _treeOnly =
            (Func<Expression<Func<Large, bool>>>)
                Delegate.CreateDelegate(
                    typeof(Func<Expression<Func<Large, bool>>>),
                    _filter,
                    BuildExpression
                );
    }

    /// <summary>Adding the conditions to a new filter.</summary>
    [Benchmark]
    public Filter<Large> MakeFilter() => FilterShapes.Build(Conditions);

    /// <summary>What a caller pays: the tree and the pass over it.</summary>
    [Benchmark(Baseline = true)]
    public Expression<Func<Large, bool>> ToExpression() =>
        _filter.ToExpression();

    /// <summary>The tree alone (what <c>ToExpression</c> cost before values became parameters).</summary>
    [Benchmark]
    public Expression<Func<Large, bool>> TreeOnly() => _treeOnly();

    /// <summary>The pass alone, over a tree whose values are constants.</summary>
    [Benchmark]
    public Expression<Func<Large, bool>> ParameterisePass() =>
        ValueParameters.Parameterise(_constants);

    /// <summary>The opposite pass (<c>InlineValues()</c>).</summary>
    [Benchmark]
    public Expression<Func<Large, bool>> InlinePass() =>
        ValueParameters.Inline(_parameters);

    /// <summary>Making the filter and the expression, as a request that builds a filter and uses it once does.</summary>
    [Benchmark]
    public Expression<Func<Large, bool>> MakeAndToExpression() =>
        FilterShapes.Build(Conditions).ToExpression();

    /// <summary>Turning the expression into a delegate, for filtering objects in memory. Not Expression Builder's code, but it is what a call to <c>Where(list)</c> pays.</summary>
    [Benchmark]
    public Func<Large, bool> Compile() => _parameters.Compile();

    /// <summary>Filtering objects in memory with a filter: <c>list.Where(filter)</c> makes the filter into a delegate.</summary>
    [Benchmark]
    public Func<Large, bool> FilterToDelegate()
    {
        Func<Large, bool> func = _filter;

        return func;
    }

    [Benchmark]
    public Func<Large, bool> CompileConstants() => _constants.Compile();
}

/// <summary>
/// Running a filter over objects in memory, compared with the lambda you would have written.
/// </summary>
[MemoryDiagnoser]
public class FilterRunBenchmarks
{
    List<Large> _items = null!;
    Func<Large, bool> _byHandDelegate = null!;
    Func<Large, bool> _byHandCompiled = null!;
    Func<Large, bool> _simpleDelegate = null!;
    Func<Large, bool> _filterParameters = null!;
    Func<Large, bool> _filterConstants = null!;

    [Params(10_000)]
    public int Items { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _items = FilterShapes.Items(Items);
        _byHandDelegate = x =>
            x.Name != null
            && x.Name.Trim().ToLower().Contains("name0")
            && x.Age >= 19
            && x.Active == true;
        _simpleDelegate = x =>
            x.Name != null
            && x.Name.Contains("name0")
            && x.Age >= 19
            && x.Active == true;
        _byHandCompiled = FilterShapes.ThreeByHand().Compile();

        var expression = FilterShapes.ThreeByName().ToExpression();
        _filterParameters = expression.Compile();
        _filterConstants = expression.InlineValues().Compile();

        var expected = _items.Count(_byHandDelegate);

        foreach (
            var f in new[]
            {
                _byHandCompiled,
                _filterParameters,
                _filterConstants,
            }
        )
            if (_items.Count(f) != expected)
                throw new InvalidOperationException(
                    "The filters disagree, so the comparison means nothing."
                );
    }

    [Benchmark(Baseline = true)]
    public int HandWrittenDelegate() => _items.Count(_byHandDelegate);

    /// <summary>The lambda most people would write. It does less (no trimming or case-folding), so it is not the same filter.</summary>
    [Benchmark]
    public int HandWrittenSimpleDelegate() => _items.Count(_simpleDelegate);

    [Benchmark]
    public int HandWrittenExpressionCompiled() => _items.Count(_byHandCompiled);

    /// <summary>What <c>ToExpression().Compile()</c> gives now.</summary>
    [Benchmark]
    public int FilterWithParameters() => _items.Count(_filterParameters);

    /// <summary>What it gave before values became parameters.</summary>
    [Benchmark]
    public int FilterWithConstants() => _items.Count(_filterConstants);
}

/// <summary>
/// Reading and writing a saved filter, as JSON and as protobuf. The JSON names .NET types, so it is for storage you trust.
/// </summary>
[MemoryDiagnoser]
public class SerializationBenchmarks
{
    string _json = "";
    byte[] _bytes = [];
    Filter<Large> _filter = null!;

    [Params(3, 20, 100)]
    public int Conditions { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _filter = FilterShapes.Build(Conditions, enums: false);
        _json = JsonSerializer.Serialize(_filter);
        _filter.SerializeTo(out _bytes);

        Console.WriteLine(
            $"[{Conditions} conditions: JSON {_json.Length:N0} chars, protobuf {_bytes.Length:N0} bytes]"
        );
    }

    [Benchmark]
    public string JsonWrite() => JsonSerializer.Serialize(_filter);

    [Benchmark]
    public Filter<Large>? JsonRead() =>
        JsonSerializer.Deserialize<Filter<Large>>(_json);

    [Benchmark]
    public byte[] ProtobufWrite()
    {
        _filter.SerializeTo(out var bytes);

        return bytes;
    }

    [Benchmark]
    public Filter<Large> ProtobufRead() =>
        Filter<Large>.DeserializeFrom(_bytes);
}

/// <summary>
/// Finding an operation by name, once everything is known. (The first lookup of a process is in
/// <see cref="ColdRegistryBenchmarks"/>.)
/// </summary>
[MemoryDiagnoser]
public class RegistryBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Warm: the first lookup scans the assemblies
        OperationRegistry.Get("Equal");
        OperationRegistry.TryGet("NoSuchOperation", out _);
        ManipulatorRegistry.TryGet("NoSuchManipulator", out _);
    }

    /// <summary>The baseline: what a lookup replaces.</summary>
    [Benchmark(Baseline = true)]
    public object NewDirectly() =>
        TopMarksDevelopment.ExpressionBuilder.Operation.Equal;

    /// <summary>A name that is known.</summary>
    [Benchmark]
    public object Hit() => OperationRegistry.Get("Equal");

    /// <summary>A name that is not an operation, such as one from a damaged or hostile document.</summary>
    [Benchmark]
    public bool Miss() => OperationRegistry.TryGet("NoSuchOperation", out _);

    [Benchmark]
    public bool ManipulatorMiss() =>
        ManipulatorRegistry.TryGet("NoSuchManipulator", out _);

    [Benchmark]
    public int GetAll() => OperationRegistry.GetAll().Count;
}

/// <summary>
/// Where the time of building a schema goes, to see what is worth caching.
/// </summary>
[MemoryDiagnoser]
public class SchemaPartsBenchmarks
{
    string[] _names = [];
    FilterBuilderOptions _options = new();

    [GlobalSetup]
    public void Setup() =>
        _names = typeof(Large).GetProperties().Select(p => p.Name).ToArray();

    /// <summary>The label of every property of the large class.</summary>
    [Benchmark]
    public int Humanize()
    {
        var length = 0;

        foreach (var name in _names)
            length += SchemaBuilder.Humanize(name).Length;

        return length;
    }

    /// <summary>The operations that are described and can be found.</summary>
    [Benchmark]
    public int CatalogOperations() => OperationCatalog.GetAvailable().Count;

    [Benchmark]
    public int OptionsOperations() => _options.GetOperations().Count;

    [Benchmark]
    public int OptionsManipulators() => _options.GetManipulators().Count;
}
