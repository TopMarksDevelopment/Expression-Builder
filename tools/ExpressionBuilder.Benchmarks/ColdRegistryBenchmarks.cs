namespace ExpressionBuilder.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using TopMarksDevelopment.ExpressionBuilder;

/// <summary>
/// The first lookup in a process, which finds and loads the assemblies that hold operations. A registry cannot be
/// reset, so each measurement is one call in a new process (<c>launchCount</c> of them), and includes the JIT.
/// </summary>
[Config(typeof(ColdConfig))]
public class ColdRegistryBenchmarks
{
    class ColdConfig : ManualConfig
    {
        public ColdConfig() =>
            AddJob(
                Job.Default.WithStrategy(RunStrategy.ColdStart)
                    .WithLaunchCount(25)
                    .WithWarmupCount(0)
                    .WithIterationCount(1)
                    .WithInvocationCount(1)
                    .WithUnrollFactor(1)
            );
    }

    [Benchmark]
    public object FirstLookup() => OperationRegistry.Get("Equal");
}
