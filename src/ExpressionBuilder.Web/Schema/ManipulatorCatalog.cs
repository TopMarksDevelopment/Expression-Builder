namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Collections.Concurrent;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// The manipulators ("adjust the text before comparing") the filter builder offers, and how it makes them.
/// </summary>
/// <remarks>
/// A browser can only choose from the manipulators described here (or added to one builder with
/// <see cref="FilterBuilderOptions.AddManipulator"/>); it can never name a type or method of its own.
/// Custom manipulators are kept apart from the built-in ones: they are listed under their own heading, and
/// can't take a built-in name.
/// </remarks>
public static class ManipulatorCatalog
{
    static readonly ConcurrentDictionary<
        string,
        ManipulatorDescriptor
    > _descriptors = new(StringComparer.Ordinal);

    static readonly List<string> _order = [];
    static readonly object _orderLock = new();

    static ManipulatorCatalog()
    {
        Store(MethodManipulator("Trim", "Trim spaces"));
        Store(MethodManipulator("ToLower", "Make lower case"));
        Store(MethodManipulator("ToUpper", "Make upper case"));

        Store(
            new ManipulatorDescriptor(
                "Replace",
                "Replace text",
                text => new ReplaceManipulator(text[0], text[1]),
                [
                    new ManipulatorParameter("Find"),
                    new ManipulatorParameter("Replace with", Required: false),
                ],
                read: manipulator =>
                    manipulator
                        is ReplaceManipulator
                        {
                            Arguments: [string find, string with]
                        }
                        ? [find, with]
                        : null
            )
            {
                IsBuiltIn = true,
            }
        );
    }

    static ManipulatorDescriptor MethodManipulator(
        string method,
        string label
    ) =>
        new(
            method,
            label,
            _ => new ExpressionMethodManipulator(
                typeof(string).GetMethod(method, Type.EmptyTypes)!,
                []
            ),
            read: manipulator =>
                manipulator
                    is ExpressionMethodManipulator { Arguments: [] } named
                && named.Name == method
                    ? []
                    : null
        )
        {
            IsBuiltIn = true,
        };

    static void Store(ManipulatorDescriptor descriptor)
    {
        if (_descriptors.TryAdd(descriptor.Name, descriptor))
            lock (_orderLock)
                _order.Add(descriptor.Name);
        else
            _descriptors[descriptor.Name] = descriptor;
    }

    /// <summary>
    /// Describes a manipulator of your own, so it is offered by every filter builder (apart from those that
    /// leave it out). It is replaced if a custom manipulator of the same name was described before.
    /// </summary>
    /// <param name="descriptor">The description</param>
    /// <exception cref="InvalidOperationException">
    /// The name belongs to a built-in manipulator. Use <see cref="FilterBuilderOptions.ManipulatorLabels"/> to
    /// change its text, or <see cref="FilterBuilderOptions.ExcludeManipulators"/> to leave it out.
    /// </exception>
    public static void Register(ManipulatorDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ThrowIfBuiltIn(descriptor.Name);

        Store(descriptor);
    }

    /// <summary>
    /// Registers a manipulator class so saved filters can find it by name, and describes it for the filter
    /// builder. See <see cref="ManipulatorDescriptor.For{TManipulator}"/>.
    /// </summary>
    /// <typeparam name="TManipulator">The manipulator</typeparam>
    /// <param name="label">What the user sees</param>
    /// <param name="parameters">What it needs from the user. None if omitted.</param>
    /// <param name="kinds">The kinds of property it works on. Text only if omitted.</param>
    /// <param name="group">The heading it is listed under</param>
    public static void Register<TManipulator>(
        string label,
        IReadOnlyList<ManipulatorParameter>? parameters = null,
        IReadOnlySet<FieldKind>? kinds = null,
        string? group = null
    )
        where TManipulator : IEntityManipulator, new()
    {
        var descriptor = ManipulatorDescriptor.For<TManipulator>(
            label,
            parameters,
            kinds,
            group
        );

        ThrowIfBuiltIn(descriptor.Name);
        ManipulatorRegistry.Register<TManipulator>();
        Store(descriptor);
    }

    internal static void ThrowIfBuiltIn(string name)
    {
        if (
            _descriptors.TryGetValue(name, out var existing)
            && existing.IsBuiltIn
        )
            throw new InvalidOperationException(
                $"\"{name}\" is a built-in manipulator, so it can't be replaced."
            );
    }

    /// <summary>
    /// Forgets the description of a custom manipulator, so it is no longer offered. A built-in manipulator
    /// can't be forgotten: leave it out of a builder with
    /// <see cref="FilterBuilderOptions.ExcludeManipulators"/>.
    /// </summary>
    /// <param name="name">The name the manipulator was described with</param>
    /// <returns>True if it was described</returns>
    public static bool Unregister(string name)
    {
        if (
            _descriptors.TryGetValue(name, out var existing)
            && existing.IsBuiltIn
        )
            return false;

        lock (_orderLock)
            _order.Remove(name);

        return _descriptors.TryRemove(name, out _);
    }

    /// <summary>
    /// Gets the manipulators that are described, in the order they are offered.
    /// </summary>
    public static IReadOnlyList<ManipulatorDescriptor> GetAvailable()
    {
        string[] order;

        lock (_orderLock)
            order = [.. _order];

        return order
            .Select(name =>
                _descriptors.TryGetValue(name, out var found) ? found : null
            )
            .OfType<ManipulatorDescriptor>()
            .ToList();
    }
}
