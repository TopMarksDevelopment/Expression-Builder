namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

public sealed class TypeTracker
{
    TypeTracker() { }

    static ICollection<Type>? _operations;
    public static ICollection<Type> Operations
    {
        get => _operations ??= [];
        set { _operations = value; }
    }

    static ICollection<Type>? _manipulatorTypes;
    public static ICollection<Type> ManipulatorTypes
    {
        get => _manipulatorTypes ??= [];
        set { _manipulatorTypes = value; }
    }
}
