namespace ExpressionBuilder.Tests.Registry;

using System.Linq.Expressions;
using System.Reflection;
using TopMarksDevelopment.ExpressionBuilder.Api;

[Collection("Operation registry")]
public class OperationRegistryTests
{
    [Theory(
        DisplayName = "Finds the operations of referenced packages by name"
    )]
    [InlineData("Equal")]
    [InlineData("IsNull")]
    [InlineData("LessThanOrEqual")]
    public void FindsReferencedOperations(string name)
    {
        var operation = OperationRegistry.Get(name);

        Assert.Equal(name, operation.Name);
    }

    [Fact(DisplayName = "Every request gets a new instance")]
    public void GivesNewInstances()
    {
        // Operations are structs, so boxed instances are never shared. Custom class based
        // operations must not be shared either, as they may hold state.
        OperationRegistry.Register<CounterOperation>();

        var first = (CounterOperation)OperationRegistry.Get("Counter");
        first.Calls++;

        var second = (CounterOperation)OperationRegistry.Get("Counter");

        Assert.NotSame(first, second);
        Assert.Equal(0, second.Calls);
    }

    [Fact(DisplayName = "TryGet does not throw for an unknown operation")]
    public void TryGetUnknownIsFalse()
    {
        var found = OperationRegistry.TryGet("NoSuchOperation", out _);

        Assert.False(found);
    }

    [Fact(DisplayName = "Get explains how to fix an unknown operation")]
    public void GetUnknownThrowsHelpfully()
    {
        var exception = Assert.Throws<TypeLoadException>(
            () => OperationRegistry.Get("NoSuchOperation")
        );

        Assert.Contains("NoSuchOperation", exception.Message);
        Assert.Contains(nameof(OperationRegistry.Register), exception.Message);
    }

    [Fact(
        DisplayName = "Operations from loaded assemblies are found without registering"
    )]
    public void FindsOperationsInLoadedAssemblies() =>
        // This test assembly uses the API, so its operations are found like any other
        Assert.True(OperationRegistry.TryGet("Discovered", out _));

    [Fact(DisplayName = "Registering replaces an operation with the same name")]
    public void RegisterReplaces()
    {
        OperationRegistry.Register<ReplacedOperationV1>();
        OperationRegistry.Register<ReplacedOperationV2>();

        Assert.IsType<ReplacedOperationV2>(OperationRegistry.Get("Replaced"));
    }

    [Fact(DisplayName = "An assembly can be registered as a whole")]
    public void RegisterAssembly() =>
        Assert.Null(
            Record.Exception(
                () => OperationRegistry.RegisterAssembly(typeof(Equal).Assembly)
            )
        );

    [Fact(DisplayName = "A null assembly is rejected")]
    public void RegisterNullAssemblyThrows() =>
        Assert.Throws<ArgumentNullException>(
            () => OperationRegistry.RegisterAssembly(null!)
        );

    [Fact(DisplayName = "GetAll lists every operation once, in name order")]
    public void GetAllIsOrderedAndUnique()
    {
        var names = OperationRegistry.GetAll().Select(o => o.Name).ToList();

        Assert.Contains("Equal", names);
        Assert.Equal(names.Distinct().Order(StringComparer.Ordinal), names);
    }

    [Fact(
        DisplayName = "Operations that cannot be created without arguments are ignored"
    )]
    public void IgnoresOperationsNeedingArguments()
    {
        OperationRegistry.RegisterAssembly(
            typeof(OperationRegistryTests).Assembly
        );

        Assert.False(OperationRegistry.TryGet("NeedsArguments", out _));
    }

    [Fact(DisplayName = "Is safe to use from many threads")]
    public void ThreadSafe()
    {
        var results = new Exception?[16];

        Parallel.For(
            0,
            results.Length,
            i =>
                results[i] = Record.Exception(() =>
                {
                    OperationRegistry.Get("Equal");
                    OperationRegistry.TryGet("Nope" + i, out _);
                    OperationRegistry.GetAll();
                })
        );

        Assert.All(results, Assert.Null);
    }

    public class CounterOperation : OperationBase
    {
        public int Calls { get; set; }

        public override string Name => "Counter";
    }

    public class ReplacedOperationV1 : OperationBase
    {
        public override string Name => "Replaced";
    }

    public class ReplacedOperationV2 : OperationBase
    {
        public override string Name => "Replaced";
    }

    // Found by scanning the assembly, rather than being registered by a test
    public class DiscoveredOperation : OperationBase
    {
        public override string Name => "Discovered";
    }

    public class NeedsArgumentsOperation(int argument) : OperationBase
    {
        public int Argument { get; } = argument;

        public override string Name => "NeedsArguments";
    }

    public abstract class OperationBase : IOperation
    {
        public abstract string Name { get; }

        public OperationDefaults Defaults => new();

        public Expression Build<TPropertyType>(
            Expression member,
            IFilterCollection<TPropertyType?> values,
            IFilterStatementOptions? options
        ) => Expression.Constant(true);

        public void Validate(IFilterStatement statement) { }
    }
}
