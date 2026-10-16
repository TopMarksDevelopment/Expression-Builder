namespace TopMarksDevelopment.ExpressionBuilder.Registry;

using System.Collections.Concurrent;
using System.Reflection;

/// <summary>
/// Looks up implementations of <typeparamref name="TContract"/> by name, finding them on demand.
/// </summary>
/// <typeparam name="TContract">The interface being implemented</typeparam>
/// <remarks>
/// Implementations are stored as types (not instances), because a new instance is handed out for
/// every request - exactly as <c>new Equal()</c> would be in code.
/// </remarks>
sealed class NamedTypeRegistry<TContract>(
    Func<TContract, string> nameOf,
    Func<Type, bool>? exclude = null
)
    where TContract : class
{
    readonly ConcurrentDictionary<string, Type> _types = new(
        StringComparer.Ordinal
    );
    readonly object _discoveryLock = new();
    int _scannedAssemblyCount = -1;

    internal void Register(Type type)
    {
        if (!TryCreate(type, out var instance))
            throw new ArgumentException(
                $"{type} must be a concrete {typeof(TContract).Name} with a public parameterless constructor.",
                nameof(type)
            );

        _types[nameOf(instance)] = type;
    }

    internal void RegisterAssembly(Assembly assembly)
    {
        foreach (var type in AssemblyDiscovery.GetLoadableTypes(assembly))
            if (IsCandidate(type) && TryCreate(type, out var instance))
                _types.TryAdd(nameOf(instance), type);
    }

    internal bool TryGet(string name, out TContract instance)
    {
        if (!_types.ContainsKey(name))
            Discover();

        if (
            _types.TryGetValue(name, out var type)
            && TryCreate(type, out var created)
        )
        {
            instance = created;
            return true;
        }

        instance = null!;
        return false;
    }

    internal IReadOnlyList<TContract> GetAll()
    {
        Discover();

        return _types
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => TryCreate(p.Value, out var created) ? created : null)
            .Where(i => i != null)
            .ToList()!;
    }

    /// <summary>
    /// Scans for implementations, unless nothing has been loaded since the last scan.
    /// </summary>
    void Discover()
    {
        lock (_discoveryLock)
        {
            if (_scannedAssemblyCount == AssemblyDiscovery.LoadedAssemblyCount)
                return;

            foreach (var assembly in AssemblyDiscovery.GetCandidates())
                RegisterAssembly(assembly);

            _scannedAssemblyCount = AssemblyDiscovery.LoadedAssemblyCount;
        }
    }

    bool IsCandidate(Type type) =>
        type
            is {
                IsAbstract: false,
                IsInterface: false,
                IsGenericTypeDefinition: false
            }
        && typeof(TContract).IsAssignableFrom(type)
        && exclude?.Invoke(type) != true;

    static bool TryCreate(Type type, out TContract instance)
    {
        try
        {
            if (Activator.CreateInstance(type) is TContract created)
            {
                instance = created;
                return true;
            }
        }
        catch (Exception ex)
            when (ex
                    is MissingMethodException
                        or TargetInvocationException
                        or MemberAccessException
            )
        {
            // Not constructible without arguments, so it cannot be created by name.
        }

        instance = null!;
        return false;
    }
}
