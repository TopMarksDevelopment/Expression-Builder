namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

public sealed class TypeTracker
{
    public static readonly ICollection<Type> DefaultFilterStatementTypes =
    [
        typeof(FilterStatement<string>),
        typeof(FilterStatement<string?>),
        typeof(FilterStatement<int>),
        typeof(FilterStatement<int?>),
        typeof(FilterStatement<short>),
        typeof(FilterStatement<short?>),
        typeof(FilterStatement<long>),
        typeof(FilterStatement<long?>),
        typeof(FilterStatement<uint>),
        typeof(FilterStatement<uint?>),
        typeof(FilterStatement<ushort>),
        typeof(FilterStatement<ushort?>),
        typeof(FilterStatement<ulong>),
        typeof(FilterStatement<ulong?>),
        typeof(FilterStatement<byte>),
        typeof(FilterStatement<byte?>),
        typeof(FilterStatement<bool>),
        typeof(FilterStatement<bool?>),
        typeof(FilterStatement<DateTime>),
        typeof(FilterStatement<DateTime?>),
        typeof(FilterStatement<DateTimeOffset>),
        typeof(FilterStatement<DateTimeOffset?>),
        typeof(FilterStatement<DateOnly>),
        typeof(FilterStatement<DateOnly?>),
        typeof(FilterStatement<TimeOnly>),
        typeof(FilterStatement<TimeOnly?>),
        typeof(FilterStatement<float>),
        typeof(FilterStatement<float?>),
        typeof(FilterStatement<double>),
        typeof(FilterStatement<double?>)
    ];

    TypeTracker() { }

    static ICollection<Type>? _operations;
    public static ICollection<Type> Operations
    {
        get => _operations ??= [];
        set { _operations = value; }
    }

    static ICollection<Type>? _filterStatementTypes;
    public static ICollection<Type> FilterStatementTypes
    {
        get => _filterStatementTypes ??= DefaultFilterStatementTypes;
        set { _filterStatementTypes = value; }
    }

    static ICollection<Type>? _manipulatorTypes;
    public static ICollection<Type> ManipulatorTypes
    {
        get => _manipulatorTypes ??= [];
        set { _manipulatorTypes = value; }
    }
}
