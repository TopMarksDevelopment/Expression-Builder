namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

/// <summary>
/// Works out, by reflection, what of a class can be filtered and how.
/// </summary>
sealed partial class SchemaBuilder(FilterBuilderOptions options)
{
    const string PathSeparator = " › ";

    static readonly HashSet<Type> IntegralTypes =
    [
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
    ];

    static readonly HashSet<Type> NumberTypes =
    [
        .. IntegralTypes,
        typeof(float),
        typeof(double),
        typeof(decimal),
    ];

    readonly Dictionary<Type, string> _ids = [];
    readonly Dictionary<string, TypeSchema> _types = [];
    readonly IReadOnlyList<OperationDescriptor> _operations =
        options.GetOperations();

    public FilterSchema Build(Type root)
    {
        var rootId = AddType(root);
        var manipulators = options.GetManipulators();

        return new FilterSchema(
            rootId,
            _types,
            _operations
                .Select(o => new OperationSchema(
                    o.Name,
                    options.OperationLabels.TryGetValue(o.Name, out var label)
                        ? label
                        : o.Label,
                    o.Arity,
                    o.SupportsMatch,
                    OperationRegistry.Get(o.Name).Defaults.Match.ToString(),
                    GroupOf(o.IsBuiltIn, o.Group)
                ))
                .ToList(),
            manipulators.Select(ManipulatorSchemaFor).ToList(),
            options.Labels,
            _operations.ToDictionary(o => o.Name, StringComparer.Ordinal),
            manipulators.ToDictionary(m => m.Name, StringComparer.Ordinal)
        );
    }

    /// <summary>
    /// Built-in operations and manipulators have no heading; a custom one is under its own, or the
    /// general one for custom things.
    /// </summary>
    string? GroupOf(bool builtIn, string? group) =>
        builtIn
            ? null
            : (
                string.IsNullOrWhiteSpace(group)
                    ? options.Labels.CustomGroup
                    : group
            );

    ManipulatorSchema ManipulatorSchemaFor(ManipulatorDescriptor m)
    {
        var labels = options.Labels;

        // The built-in manipulators take their text from the labels, so they translate with the rest
        var (label, parameters) = m.IsBuiltIn
            ? m.Name switch
            {
                "Trim" => (labels.Trim, m.Parameters),
                "ToLower" => (labels.ToLower, m.Parameters),
                "ToUpper" => (labels.ToUpper, m.Parameters),
                "Replace" => (
                    labels.Replace,
                    (IReadOnlyList<ManipulatorParameter>)
                        [
                            new ManipulatorParameter(labels.ReplaceFind),
                            new ManipulatorParameter(
                                labels.ReplaceWith,
                                Required: false
                            ),
                        ]
                ),
                _ => (m.Label, m.Parameters),
            }
            : (m.Label, m.Parameters);

        return new ManipulatorSchema(
            m.Name,
            options.ManipulatorLabels.TryGetValue(m.Name, out var replaced)
                ? replaced
                : label,
            parameters,
            [.. m.Kinds],
            GroupOf(m.IsBuiltIn, m.Group)
        );
    }

    /// <summary>
    /// Adds a class (and, through its collections, the classes they hold) to the schema.
    /// </summary>
    string AddType(Type type)
    {
        if (_ids.TryGetValue(type, out var existing))
            return existing;

        // Registered first, as a class can be reached again through its own collections
        var id = $"t{_ids.Count}";
        _ids[type] = id;

        var fields = new List<FieldSchema>();
        AddFields(type, prefix: "", labelPrefix: "", depth: 0, fields);

        _types[id] = new TypeSchema(id, Humanize(type.Name), fields);

        return id;
    }

    void AddFields(
        Type type,
        string prefix,
        string labelPrefix,
        int depth,
        List<FieldSchema> fields
    )
    {
        foreach (var property in GetProperties(type))
        {
            var path = prefix + property.Name;
            var label = labelPrefix + GetLabel(property);
            var propertyType = property.PropertyType;

            if (TryGetCollectionItem(propertyType, out var itemType))
            {
                fields.Add(CollectionField(path, label, itemType));
            }
            else if (TryClassifyValue(propertyType, property, out var kind))
            {
                fields.Add(ValueField(path, label, kind, propertyType));
            }
            else if (IsNavigation(propertyType))
            {
                fields.Add(
                    ValueField(path, label, FieldKind.Object, propertyType)
                );

                // Reach into one-to-one relationships, as far as the options allow
                if (depth < options.MaxNavigationDepth - 1)
                    AddFields(
                        propertyType,
                        path + ".",
                        label + PathSeparator,
                        depth + 1,
                        fields
                    );
            }
        }
    }

    IEnumerable<PropertyInfo> GetProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p =>
                p.GetMethod is { IsPublic: true }
                && p.GetIndexParameters().Length == 0
                && !options.IsExcluded(p)
            );

    FieldSchema CollectionField(string path, string label, Type itemType)
    {
        string itemTypeId;

        if (TryClassifyValue(itemType, null, out var kind))
        {
            // A collection of plain values (strings, numbers...): its items are filtered by a
            // single, unnamed field
            var scalarKey = itemType;
            if (!_ids.TryGetValue(scalarKey, out itemTypeId!))
            {
                itemTypeId = $"t{_ids.Count}";
                _ids[scalarKey] = itemTypeId;
                _types[itemTypeId] = new TypeSchema(
                    itemTypeId,
                    Humanize(itemType.Name),
                    [ValueField("", "Value", kind, itemType)]
                );
            }
        }
        else if (IsNavigation(itemType))
        {
            itemTypeId = AddType(itemType);
        }
        else
        {
            // Not something we can filter, so show nothing for it
            return new FieldSchema(
                path,
                label,
                FieldKind.Collection,
                false,
                "",
                []
            )
            {
                ValueType = itemType,
            };
        }

        return new FieldSchema(
            path,
            label,
            FieldKind.Collection,
            Nullable: false,
            Input: "",
            Operations: [],
            ItemType: itemTypeId
        )
        {
            ValueType = itemType,
        };
    }

    FieldSchema ValueField(
        string path,
        string label,
        FieldKind kind,
        Type clrType
    )
    {
        var underlying = Nullable.GetUnderlyingType(clrType) ?? clrType;
        var nullable = !clrType.IsValueType || underlying != clrType;

        return new FieldSchema(
            path,
            label,
            kind,
            nullable,
            Input: InputFor(kind),
            Operations: _operations
                .Where(o => o.AppliesTo(kind, nullable))
                .Select(o => o.Name)
                .ToList(),
            Step: kind == FieldKind.Number
                ? (IntegralTypes.Contains(underlying) ? "1" : "any")
                : null,
            Options: kind == FieldKind.Enum ? EnumOptions(underlying) : null
        )
        {
            ValueType = underlying,
        };
    }

    static string InputFor(FieldKind kind) =>
        kind switch
        {
            FieldKind.Number => "number",
            FieldKind.Date => "date",
            FieldKind.DateTime => "datetime-local",
            FieldKind.Time => "time",
            FieldKind.Boolean => "boolean",
            FieldKind.Enum => "select",
            FieldKind.Object => "",
            _ => "text",
        };

    static List<EnumOption> EnumOptions(Type enumType) =>
        enumType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f =>
                !f.IsDefined(typeof(FilterBuilderIgnoreAttribute), false)
            )
            .Select(f => new EnumOption(f.Name, GetLabel(f)))
            .ToList();

    /// <summary>
    /// Works out what a value of this type is, if it is one the filter builder handles directly.
    /// </summary>
    static bool TryClassifyValue(
        Type type,
        PropertyInfo? property,
        out FieldKind kind
    )
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        kind = default;

        if (underlying == typeof(string))
            kind = FieldKind.String;
        else if (underlying == typeof(bool))
            kind = FieldKind.Boolean;
        else if (underlying.IsEnum)
            kind = FieldKind.Enum;
        else if (underlying == typeof(Guid))
            kind = FieldKind.Guid;
        else if (NumberTypes.Contains(underlying))
            kind = FieldKind.Number;
        else if (underlying == typeof(DateOnly))
            kind = FieldKind.Date;
        else if (underlying == typeof(DateTime))
            kind =
                property?.GetCustomAttribute<DataTypeAttribute>()?.DataType
                == DataType.Date
                    ? FieldKind.Date
                    : FieldKind.DateTime;
        else if (underlying == typeof(DateTimeOffset))
            kind = FieldKind.DateTime;
        else if (
            underlying == typeof(TimeOnly)
            || underlying == typeof(TimeSpan)
        )
            kind = FieldKind.Time;
        else
            return false;

        return true;
    }

    /// <summary>
    /// A one-to-one relationship: a class of our own or the user's, not one of the framework's.
    /// </summary>
    static bool IsNavigation(Type type) =>
        !type.IsPrimitive
        && !type.IsPointer
        && !type.IsGenericTypeDefinition
        && type != typeof(object)
        && !typeof(Delegate).IsAssignableFrom(type)
        && !typeof(Type).IsAssignableFrom(type)
        && !typeof(Stream).IsAssignableFrom(type)
        && !typeof(IDictionary).IsAssignableFrom(type)
        && !type.IsArray
        && type.Namespace is { } ns
        && !ns.StartsWith("System", StringComparison.Ordinal)
        && !ns.StartsWith("Microsoft", StringComparison.Ordinal);

    static bool TryGetCollectionItem(Type type, out Type itemType)
    {
        itemType = null!;

        if (
            type == typeof(string)
            || type == typeof(byte[])
            || typeof(IDictionary).IsAssignableFrom(type)
            || !typeof(IEnumerable).IsAssignableFrom(type)
        )
            return false;

        var enumerable =
            type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? type
                : type.GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    );

        if (enumerable == null)
            return false;

        itemType = enumerable.GetGenericArguments()[0];

        return true;
    }

    static string GetLabel(MemberInfo member) =>
        member.GetCustomAttribute<DisplayAttribute>()?.GetName()
        ?? member.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
        ?? Humanize(member.Name);

    /// <summary>
    /// Turns <c>OtherSearchField</c> into "Other search field".
    /// </summary>
    internal static string Humanize(string name)
    {
        var words = WordBoundary()
            .Split(name.Replace('_', ' '))
            .Where(w => w.Length > 0);
        var text = string.Join(
            " ",
            words.Select(w =>
                // Keep acronyms (ID, URL) as they are
                w.Length > 1
                && w.All(char.IsUpper)
                    ? w
                    : w.ToLowerInvariant()
            )
        );

        return text.Length == 0
            ? name
            : char.ToUpperInvariant(text[0]) + text[1..];
    }

    [GeneratedRegex(@"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|\s+")]
    private static partial Regex WordBoundary();
}
