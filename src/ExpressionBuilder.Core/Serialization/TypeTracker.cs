namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

/// <summary>
/// A cache of the operation and manipulator types found while deserialising.
/// </summary>
[Obsolete(
    "This is no longer used. Operations and manipulators are found by OperationRegistry and "
        + "ManipulatorRegistry; use their Register methods to add your own."
)]
public sealed class TypeTracker
{
    TypeTracker() { }

    static ICollection<Type>? _operations;

    /// <summary>Not used. See <see cref="OperationRegistry"/>.</summary>
    public static ICollection<Type> Operations
    {
        get => _operations ??= [];
        set { _operations = value; }
    }

    static ICollection<Type>? _manipulatorTypes;

    /// <summary>Not used. See <see cref="ManipulatorRegistry"/>.</summary>
    public static ICollection<Type> ManipulatorTypes
    {
        get => _manipulatorTypes ??= [];
        set { _manipulatorTypes = value; }
    }
}
