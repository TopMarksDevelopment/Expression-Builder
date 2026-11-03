namespace TopMarksDevelopment.ExpressionBuilder.Registry;

using System.Reflection;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// Finds the assemblies that might hold operations or manipulators.
/// </summary>
/// <remarks>
/// The CLR only loads an assembly when code first touches one of its types. A filter that is
/// deserialised before anything has used (for example) <c>Operation.Equal</c> would therefore not be
/// able to see the <c>Equal</c> operation by scanning the loaded assemblies alone. So, as well as
/// those, we follow the references of anything that uses this library and load the Expression Builder
/// assemblies they point at, and any that sit next to the application (a package can be installed
/// without any code using a type from it).
/// </remarks>
static class AssemblyDiscovery
{
    const string OwnAssemblyPrefix = "TopMarksDevelopment.ExpressionBuilder";

    static readonly string ApiAssemblyName = typeof(IOperation)
        .Assembly.GetName()
        .Name!;

    /// <summary>
    /// The number of assemblies currently loaded. A scan can be skipped if this has not changed
    /// since the last one.
    /// </summary>
    internal static int LoadedAssemblyCount =>
        AppDomain.CurrentDomain.GetAssemblies().Length;

    /// <summary>
    /// Gets every assembly that could contain an implementation, loading referenced Expression
    /// Builder assemblies that are not loaded yet.
    /// </summary>
    internal static IReadOnlyCollection<Assembly> GetCandidates()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Queue<Assembly>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            seen.Add(assembly.FullName!);

            if (UsesApi(assembly))
                pending.Enqueue(assembly);
        }

        while (pending.Count > 0)
            foreach (
                var reference in pending.Dequeue().GetReferencedAssemblies()
            )
            {
                if (
                    reference.Name?.StartsWith(
                        OwnAssemblyPrefix,
                        StringComparison.Ordinal
                    ) != true
                    || !seen.Add(reference.FullName)
                )
                    continue;

                try
                {
                    pending.Enqueue(Assembly.Load(reference));
                }
                catch (Exception ex)
                    when (ex is IOException or BadImageFormatException)
                {
                    // A reference that cannot be loaded cannot supply operations either.
                }
            }

        LoadFromApplicationFolder(seen);

        return AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && UsesApi(a))
            .ToList();
    }

    /// <summary>
    /// Loads the Expression Builder assemblies that are next to the application but that nothing
    /// refers to, such as the operations of packages that are installed but not used in code.
    /// </summary>
    static void LoadFromApplicationFolder(HashSet<string> seen)
    {
        IEnumerable<string> files;

        try
        {
            files = Directory.EnumerateFiles(
                AppContext.BaseDirectory,
                OwnAssemblyPrefix + "*.dll"
            );
        }
        catch (Exception ex)
            when (ex is IOException or UnauthorizedAccessException)
        {
            return;
        }

        foreach (var file in files)
            try
            {
                var name = AssemblyName.GetAssemblyName(file);

                if (!seen.Add(name.FullName))
                    continue;

                try
                {
                    Assembly.Load(name);
                }
                catch (FileNotFoundException)
                {
                    // Not part of the application's dependencies, so load it from where it is
                    Assembly.LoadFrom(file);
                }
            }
            catch (Exception ex)
                when (ex is IOException or BadImageFormatException)
            {
                // Not an assembly we can use
            }
    }

    static bool UsesApi(Assembly assembly) =>
        assembly.GetName().Name == ApiAssemblyName
        || assembly
            .GetReferencedAssemblies()
            .Any(r => r.Name == ApiAssemblyName);

    /// <summary>
    /// Gets the types of an assembly, ignoring the ones that could not be loaded (a partially
    /// available dependency must not hide the rest).
    /// </summary>
    internal static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }
}
