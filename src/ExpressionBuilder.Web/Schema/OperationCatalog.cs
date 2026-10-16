namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Collections.Concurrent;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// The operations the filter builder offers, and how it presents them.
/// </summary>
/// <remarks>
/// The built-in operations are described here. To offer an operation of your own, make sure it can be
/// found by the <see cref="OperationRegistry"/> and describe it with
/// <see cref="Register(OperationDescriptor)"/> (or do both with <see cref="Register{TOperation}"/>). An
/// operation is only offered if it is both described and registered. Custom operations are kept apart
/// from the built-in ones: they are listed under their own heading, and can't take a built-in name.
/// </remarks>
public static class OperationCatalog
{
    static readonly ConcurrentDictionary<
        string,
        OperationDescriptor
    > _descriptors = new(StringComparer.Ordinal);

    // The order operations appear in the list is the order they are registered here
    static readonly List<string> _order = [];
    static readonly object _orderLock = new();

    static OperationCatalog()
    {
        FieldKind[] text = [FieldKind.String];
        FieldKind[] ordered =
        [
            FieldKind.Number,
            FieldKind.Date,
            FieldKind.DateTime,
            FieldKind.Time,
        ];
        FieldKind[] comparable =
        [
            FieldKind.String,
            .. ordered,
            FieldKind.Boolean,
            FieldKind.Enum,
            FieldKind.Guid,
        ];
        FieldKind[] listable =
        [
            FieldKind.String,
            .. ordered,
            FieldKind.Enum,
            FieldKind.Guid,
        ];
        FieldKind[] nullable =
        [
            FieldKind.String,
            .. ordered,
            FieldKind.Boolean,
            FieldKind.Enum,
            FieldKind.Guid,
            FieldKind.Object,
        ];

        Add("Equal", "Equals", ValueArity.One, comparable);
        Add("NotEqual", "Does not equal", ValueArity.One, comparable);
        // Matching all of several values only makes sense for some operations: text cannot start with
        // two different things, so StartsWith and EndsWith always match any of them
        Add("Contains", "Contains", ValueArity.Many, text, supportsMatch: true);
        Add(
            "DoesNotContain",
            "Does not contain",
            ValueArity.Many,
            text,
            supportsMatch: true
        );
        Add("StartsWith", "Starts with", ValueArity.Many, text);
        Add(
            "DoesNotStartWith",
            "Does not start with",
            ValueArity.Many,
            text,
            supportsMatch: true
        );
        Add("EndsWith", "Ends with", ValueArity.Many, text);
        Add(
            "DoesNotEndWith",
            "Does not end with",
            ValueArity.Many,
            text,
            supportsMatch: true
        );
        Add("SmartSearch", "Matches search terms", ValueArity.Many, text);
        Add("GreaterThan", "Is greater than", ValueArity.One, ordered);
        Add(
            "GreaterThanOrEqual",
            "Is greater than or equal to",
            ValueArity.One,
            ordered
        );
        Add("LessThan", "Is less than", ValueArity.One, ordered);
        Add(
            "LessThanOrEqual",
            "Is less than or equal to",
            ValueArity.One,
            ordered
        );
        Add("Between", "Is between", ValueArity.Two, ordered);
        Add(
            "BetweenExclusive",
            "Is between (not including the ends)",
            ValueArity.Two,
            ordered
        );
        Add("NotBetween", "Is not between", ValueArity.Two, ordered);
        Add(
            "NotBetweenExclusive",
            "Is not between (not including the ends)",
            ValueArity.Two,
            ordered
        );
        Add("In", "Is one of", ValueArity.Many, listable);
        Add("NotIn", "Is not one of", ValueArity.Many, listable);
        Add("IsEmpty", "Is empty text", ValueArity.None, text);
        Add("IsNotEmpty", "Is not empty text", ValueArity.None, text);
        Add("IsNullOrWhiteSpace", "Is blank", ValueArity.None, text);
        Add("IsNotNullOrWhiteSpace", "Is not blank", ValueArity.None, text);
        Add(
            "IsNull",
            "Is not set",
            ValueArity.None,
            nullable,
            nullableOnly: true
        );
        Add(
            "IsNotNull",
            "Is set",
            ValueArity.None,
            nullable,
            nullableOnly: true
        );
    }

    static void Add(
        string name,
        string label,
        ValueArity arity,
        FieldKind[] kinds,
        bool nullableOnly = false,
        bool supportsMatch = false
    ) =>
        Store(
            new OperationDescriptor(
                name,
                label,
                arity,
                kinds.ToHashSet(),
                nullableOnly,
                supportsMatch
            )
            {
                IsBuiltIn = true,
            }
        );

    static void Store(OperationDescriptor descriptor)
    {
        if (_descriptors.TryAdd(descriptor.Name, descriptor))
            lock (_orderLock)
                _order.Add(descriptor.Name);
        else
            _descriptors[descriptor.Name] = descriptor;
    }

    /// <summary>
    /// Describes an operation of your own, so it is offered by every filter builder (apart from those that
    /// leave it out). It is replaced if a custom operation of the same name was described before.
    /// </summary>
    /// <remarks>
    /// The operation itself must also be findable by the <see cref="OperationRegistry"/>; see the generic
    /// overload, which does both. A custom operation is listed apart from the built-in ones.
    /// </remarks>
    /// <param name="descriptor">The description</param>
    /// <exception cref="InvalidOperationException">
    /// The name belongs to a built-in operation. Use <see cref="FilterBuilderOptions.OperationLabels"/> to
    /// change its text, or <see cref="FilterBuilderOptions.ExcludeOperations"/> to leave it out.
    /// </exception>
    public static void Register(OperationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ThrowIfBuiltIn(descriptor.Name);

        // A custom operation is always shown as one, whatever the descriptor says
        Store(descriptor with { IsBuiltIn = false });
    }

    /// <summary>
    /// Registers an operation so it can be found by name, and describes it for the filter builder.
    /// </summary>
    /// <typeparam name="TOperation">The operation</typeparam>
    /// <param name="descriptor">The description, whose name must be the operation's name</param>
    public static void Register<TOperation>(OperationDescriptor descriptor)
        where TOperation : IOperation, new()
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        EnsureNamesMatch<TOperation>(descriptor);

        OperationRegistry.Register<TOperation>();
        Register(descriptor);
    }

    internal static void EnsureNamesMatch<TOperation>(
        OperationDescriptor descriptor
    )
        where TOperation : IOperation, new()
    {
        var name = new TOperation().Name;

        if (!string.Equals(name, descriptor.Name, StringComparison.Ordinal))
            throw new ArgumentException(
                $"The operation is named \"{name}\", but it is described as \"{descriptor.Name}\".",
                nameof(descriptor)
            );
    }

    internal static void ThrowIfBuiltIn(string name)
    {
        if (
            _descriptors.TryGetValue(name, out var existing)
            && existing.IsBuiltIn
        )
            throw new InvalidOperationException(
                $"\"{name}\" is a built-in operation, so it can't be replaced."
            );
    }

    /// <summary>
    /// Forgets the description of a custom operation, so it is no longer offered. A built-in operation
    /// can't be forgotten: leave it out of a builder with <see cref="FilterBuilderOptions.ExcludeOperations"/>.
    /// </summary>
    /// <param name="name">The operation's <see cref="IOperation.Name"/></param>
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
    /// Gets the description of an operation.
    /// </summary>
    /// <param name="name">The operation's <see cref="IOperation.Name"/></param>
    /// <param name="descriptor">The description, if there is one</param>
    /// <returns>True if the operation is described</returns>
    public static bool TryGet(
        string name,
        out OperationDescriptor descriptor
    ) => _descriptors.TryGetValue(name, out descriptor!);

    /// <summary>
    /// Gets the operations that are both described and available, in the order they are offered.
    /// </summary>
    public static IReadOnlyList<OperationDescriptor> GetAvailable()
    {
        string[] order;

        lock (_orderLock)
            order = [.. _order];

        return order
            .Where(name =>
                OperationRegistry.TryGet(name, out _)
                && _descriptors.ContainsKey(name)
            )
            .Select(name => _descriptors[name])
            .ToList();
    }
}
