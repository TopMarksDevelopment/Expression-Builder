namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Everything the filter builder needs to know about a class: the properties and collections that
/// can be filtered, what kind each is, and the operations that suit it.
/// </summary>
/// <remarks>
/// It is sent to the browser to build the choices, and used on the server to check a posted filter -
/// so both sides always agree on what is allowed.
/// </remarks>
public sealed class FilterSchema
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
        },
    };

    internal FilterSchema(
        string root,
        IReadOnlyDictionary<string, TypeSchema> types,
        IReadOnlyList<OperationSchema> operations,
        IReadOnlyList<ManipulatorSchema> manipulators,
        FilterBuilderLabels labels,
        IReadOnlyDictionary<string, OperationDescriptor> operationDescriptors,
        IReadOnlyDictionary<
            string,
            ManipulatorDescriptor
        > manipulatorDescriptors
    )
    {
        Root = root;
        Types = types;
        Operations = operations;
        Manipulators = manipulators;
        Labels = labels;
        OperationDescriptors = operationDescriptors;
        ManipulatorDescriptors = manipulatorDescriptors;
    }

    /// <summary>
    /// The id of the class being filtered.
    /// </summary>
    public string Root { get; }

    /// <summary>
    /// Every class that is filtered, by id: the root and the items of any collections.
    /// </summary>
    public IReadOnlyDictionary<string, TypeSchema> Types { get; }

    /// <summary>
    /// The operations on offer.
    /// </summary>
    public IReadOnlyList<OperationSchema> Operations { get; }

    /// <summary>
    /// The adjustments ("manipulators") on offer.
    /// </summary>
    public IReadOnlyList<ManipulatorSchema> Manipulators { get; }

    /// <summary>
    /// The text of the filter builder.
    /// </summary>
    public FilterBuilderLabels Labels { get; }

    /// <summary>
    /// Describes the filtering of a class.
    /// </summary>
    /// <typeparam name="T">The class</typeparam>
    /// <param name="options">The settings; the defaults if omitted</param>
    public static FilterSchema For<T>(FilterBuilderOptions? options = null)
        where T : class => For(typeof(T), options);

    /// <summary>
    /// Describes the filtering of a class.
    /// </summary>
    /// <param name="type">The class</param>
    /// <param name="options">The settings; the defaults if omitted</param>
    public static FilterSchema For(
        Type type,
        FilterBuilderOptions? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(type);

        return new SchemaBuilder(options ?? new()).Build(type);
    }

    /// <summary>
    /// Gets the schema in the form the script reads.
    /// </summary>
    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    // What a posted name makes is decided by these, not by anything in the post
    [JsonIgnore]
    internal IReadOnlyDictionary<
        string,
        OperationDescriptor
    > OperationDescriptors { get; }

    [JsonIgnore]
    internal IReadOnlyDictionary<
        string,
        ManipulatorDescriptor
    > ManipulatorDescriptors { get; }

    internal FieldSchema? FindField(string typeId, string path) =>
        Types.TryGetValue(typeId, out var type)
            ? type.Fields.FirstOrDefault(f => f.Path == path)
            : null;
}

/// <summary>
/// A class that is filtered: the root, or the items of a collection.
/// </summary>
/// <param name="Id">Identifies the class within the schema</param>
/// <param name="Label">What to call it</param>
/// <param name="Fields">What can be filtered on</param>
public sealed record TypeSchema(
    string Id,
    string Label,
    IReadOnlyList<FieldSchema> Fields
);

/// <summary>
/// Something that can be filtered on: a property (or a property of a related object), or a
/// collection.
/// </summary>
/// <param name="Path">
/// How the property is referred to, as in the string conventions of the filter: <c>Name</c>,
/// <c>Birth.Country</c>. Empty for the item of a collection of plain values.
/// </param>
/// <param name="Label">What to call it</param>
/// <param name="Kind">What it holds</param>
/// <param name="Nullable">Whether it can be <c>null</c></param>
/// <param name="Input">The kind of input to collect a value with</param>
/// <param name="Operations">The names of the operations that suit it</param>
/// <param name="Step">For numbers, how much a value changes by (<c>1</c> or <c>any</c>)</param>
/// <param name="Options">For enums, the choices</param>
/// <param name="ItemType">For collections, the id of the class of the items</param>
public sealed record FieldSchema(
    string Path,
    string Label,
    FieldKind Kind,
    bool Nullable,
    string Input,
    IReadOnlyList<string> Operations,
    string? Step = null,
    IReadOnlyList<EnumOption>? Options = null,
    string? ItemType = null
)
{
    /// <summary>
    /// The type that values are read as (the underlying type, if it is nullable).
    /// </summary>
    [JsonIgnore]
    internal Type ValueType { get; init; } = typeof(string);
}

/// <summary>
/// A choice of an enum.
/// </summary>
/// <param name="Value">The name of the member, which is what is posted</param>
/// <param name="Label">What the user sees</param>
public sealed record EnumOption(string Value, string Label);

/// <summary>
/// An operation on offer.
/// </summary>
/// <param name="Name">The name of the operation</param>
/// <param name="Label">What the user sees</param>
/// <param name="Arity">How many values it takes</param>
/// <param name="SupportsMatch">Whether the user can choose to match any or all of several values</param>
/// <param name="DefaultMatch">Which of them is matched unless the user chooses: <c>Any</c> or <c>All</c></param>
/// <param name="Group">The heading it is listed under, or <c>null</c> for the built-in operations</param>
public sealed record OperationSchema(
    string Name,
    string Label,
    ValueArity Arity,
    bool SupportsMatch,
    string DefaultMatch,
    string? Group = null
);

/// <summary>
/// A manipulator ("adjust the text before comparing") on offer.
/// </summary>
/// <param name="Name">The name of the manipulator, which is what is posted</param>
/// <param name="Label">What the user sees</param>
/// <param name="Parameters">What the user is asked for, in the order it is posted</param>
/// <param name="Kinds">The kinds of property it suits</param>
/// <param name="Group">The heading it is listed under, or <c>null</c> for the built-in manipulators</param>
public sealed record ManipulatorSchema(
    string Name,
    string Label,
    IReadOnlyList<ManipulatorParameter> Parameters,
    IReadOnlyList<FieldKind> Kinds,
    string? Group = null
);
