namespace TopMarksDevelopment.ExpressionBuilder.Tests;

using System.Reflection;
using TopMarksDevelopment.ExpressionBuilder.Api;

public class OperationTests
{
    [Fact(DisplayName = "All operations have been found")]
    public void HasAllOperations()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

        var referencedPaths = Directory.GetFiles(
            AppDomain.CurrentDomain.BaseDirectory,
            "*.dll"
        );
        var toLoad = referencedPaths
            .Where(r =>
                !loadedPaths.Contains(
                    r,
                    StringComparer.InvariantCultureIgnoreCase
                )
            )
            .ToList();

        toLoad.ForEach(path =>
            loadedAssemblies.Add(
                AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))
            )
        );

        var type = typeof(IOperation);

        var types = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && p != type);

        Assert.Equal(25, types.Count());
    }
}
