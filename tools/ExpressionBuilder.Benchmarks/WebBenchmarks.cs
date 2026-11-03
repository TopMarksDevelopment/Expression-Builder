namespace ExpressionBuilder.Benchmarks;

using BenchmarkDotNet.Attributes;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// The schema of a class: found by reflection, sent to the page as JSON, and used again to check what is posted. Both
/// happen on every request that shows or handles a filter builder.
/// </summary>
[MemoryDiagnoser]
public class SchemaBenchmarks
{
    FilterSchema _schema = null!;

    [Params(typeof(Small), typeof(Large))]
    public Type Model { get; set; } = typeof(Small);

    [GlobalSetup]
    public void Setup()
    {
        _schema = FilterSchema.For(Model);

        Console.WriteLine(
            $"[{Model.Name}: {_schema.Types.Values.Sum(t => t.Fields.Count)} fields in {_schema.Types.Count} types, JSON {_schema.ToJson().Length:N0} chars]"
        );
    }

    /// <summary>What every page that shows a builder, and every post, does.</summary>
    [Benchmark(Baseline = true)]
    public FilterSchema Build() => FilterSchema.For(Model);

    /// <summary>The same schema as the text the page carries.</summary>
    [Benchmark]
    public string WriteJson() => _schema.ToJson();

    /// <summary>What a page does: the schema, and then its text.</summary>
    [Benchmark]
    public string BuildAndWriteJson() => FilterSchema.For(Model).ToJson();
}

/// <summary>
/// A filter posted by the browser (plain text) turned into a <c>Filter&lt;T&gt;</c>.
/// </summary>
[MemoryDiagnoser]
public class PostedFilterBenchmarks
{
    string _json = "";
    FilterSchema _schema = null!;
    FilterBuilderOptions _options = null!;

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

        var result = new FilterBuilderModel { Json = _json }.Build<Large>(
            _options
        );

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors)
            );

        Console.WriteLine(
            $"[{Conditions} conditions: {_json.Length:N0} chars posted]"
        );
    }

    /// <summary>What a post does: the schema, then the check.</summary>
    [Benchmark(Baseline = true)]
    public object Build() =>
        new FilterBuilderModel { Json = _json }.Build<Large>(_options);

    /// <summary>Just the check and the conversion, with the schema already to hand.</summary>
    [Benchmark]
    public object CompileOnly() =>
        new FilterCompiler(_schema, _options).Compile(_json);

    /// <summary>A whole post: check it, then make the expression a query would take.</summary>
    [Benchmark]
    public object BuildAndToExpression() =>
        new FilterBuilderModel { Json = _json }
            .Build<Large>(_options)
            .Filter!.ToExpression();
}
