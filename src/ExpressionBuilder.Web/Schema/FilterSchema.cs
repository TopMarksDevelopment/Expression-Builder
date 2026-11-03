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
        int maxNavigationDepth,
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
        MaxNavigationDepth = maxNavigationDepth;
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
    /// How many names a path can have when it follows relationships (<c>Birth.Country.Name</c> is three).
    /// The script stops offering relationships at this length and the server refuses a longer path.
    /// </summary>
    public int MaxNavigationDepth { get; }

    /// <summary>
    /// Every class that is filtered, by id: the root, the classes it relates to, and the items of any
    /// collections.
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

    /// <summary>
    /// Finds a property or collection by its path from a class, following relationships one name at a
    /// time. Only what the schema offers is ever found, and never through a collection (a group does that).
    /// </summary>
    /// <returns>
    /// The field with its whole <paramref name="path"/> and a label that shows the way (
    /// <c>Home address › Postcode</c>), or <c>null</c> if the schema doesn't offer it or the path is too long
    /// </returns>
    internal FieldSchema? FindField(string typeId, string path)
    {
        if (!Types.TryGetValue(typeId, out var type))
            return null;

        // The empty path is the item of a collection of plain values
        if (path.Length == 0)
            return type.Fields.FirstOrDefault(f => f.Path.Length == 0);

        var names = path.Split('.');

        if (names.Length > MaxNavigationDepth || names.Any(n => n.Length == 0))
            return null;

        var label = "";

        for (var i = 0; i < names.Length - 1; i++)
        {
            var step = type.Fields.FirstOrDefault(f => f.Path == names[i]);

            if (
                step is not { Kind: FieldKind.Object, ItemType: { } next }
                || !Types.TryGetValue(next, out type!)
            )
                return null;

            label += step.Label + RelationshipSeparator;
        }

        var field = type.Fields.FirstOrDefault(f => f.Path == names[^1]);

        return names.Length == 1 ? field
            : field is null ? null
            : field with { Path = path, Label = label + field.Label };
    }

    internal const string RelationshipSeparator = " › ";
}

/// <summary>
/// A class that is filtered: the root, a class it is related to, or the items of a collection.
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
/// Something that can be filtered on: a property, a relationship to another class, or a collection.
/// </summary>
/// <param name="Path">
/// The name of the property: <c>Name</c>. Empty for the item of a collection of plain values. A path
/// through relationships (<c>Birth.Country</c>) is made of these, one for each class on the way.
/// </param>
/// <param name="Label">What to call it</param>
/// <param name="Kind">What it holds</param>
/// <param name="Nullable">Whether it can be <c>null</c></param>
/// <param name="Input">The kind of input to collect a value with</param>
/// <param name="Operations">The names of the operations that suit it</param>
/// <param name="Step">For numbers, how much a value changes by (<c>1</c> or <c>any</c>)</param>
/// <param name="Options">For enums, the choices</param>
/// <param name="ItemType">
/// For collections, the id of the class of the items; for relationships (<see cref="FieldKind.Object"/>), the id
/// of the class they lead to
/// </param>
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
