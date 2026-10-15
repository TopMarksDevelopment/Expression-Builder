namespace ExpressionBuilder.Benchmarks;

using BenchmarkDotNet.Running;
using TopMarksDevelopment.ExpressionBuilder;

static class Program
{
    static void Main(string[] args)
    {
        if (args is ["show"])
        {
            var e = FilterShapes.ThreeByName().ToExpression();
            Console.WriteLine(e);
            Console.WriteLine(e.InlineValues());
            Console.WriteLine(FilterShapes.ThreeByHand());
            return;
        }
        if (args is ["ef"])
        {
            EfExperiment.Run();

            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
