namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Globalization;
using System.Text.Json;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// Turns a posted <see cref="FilterDocument"/> into the items of a <see cref="Filter{TClass}"/>,
/// checking everything against the <see cref="FilterSchema"/> on the way.
/// </summary>
/// <remarks>
/// Nothing in the posted document is trusted: properties must be ones the schema offers, operations
/// must suit the property, values must parse as the property's type, and the sizes in
/// <see cref="FilterBuilderOptions"/> are enforced. A property path is only ever looked up in the
/// schema, never used to reach into the class directly.
/// </remarks>
sealed class FilterCompiler(FilterSchema schema, FilterBuilderOptions options)
{
    static readonly string[] DateTimeFormats =
    [
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
    ];

    static readonly string[] TimeFormats =
    [
        "HH:mm",
        "HH:mm:ss",
        "HH:mm:ss.FFFFFFF",
    ];

    readonly List<FilterBuildError> _errors = [];
    int _conditions;

    /// <summary>
    /// Reads the posted text.
    /// </summary>
    /// <returns>The filter, or what was wrong</returns>
    public (
        ICollection<IFilterItem>? Items,
        IReadOnlyList<FilterBuildError> Errors
    ) Compile(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return ([], _errors);

        if (json.Length > options.MaxPayloadLength)
        {
            Error("", "The filter is too large.");

            return (null, _errors);
        }

        FilterDocument? document;

        try
        {
            document = JsonSerializer.Deserialize<FilterDocument>(
                json,
                FilterDocument.JsonOptions
            );
        }
        catch (JsonException)
        {
            Error("", "The filter could not be read.");

            return (null, _errors);
        }

        if (
            document?.Root == null
            || document.Version != FilterDocument.CurrentVersion
        )
        {
            Error("", "The filter is not in a form that is understood.");

            return (null, _errors);
        }

        if (document.Root.Type != FilterNode.GroupType)
        {
            Error("root", "The filter must start with a group.");

            return (null, _errors);
        }

        var items = CompileItems(
            document.Root.Items,
            schema.Root,
            "root",
            depth: 1
        );

        return (_errors.Count == 0 ? items : null, _errors);
    }

    List<IFilterItem> CompileItems(
        List<FilterNode>? nodes,
        string typeId,
        string path,
        int depth
    )
    {
        var items = new List<IFilterItem>();

        for (var i = 0; i < (nodes?.Count ?? 0); i++)
        {
            var node = nodes![i];
            var nodePath = $"{path}.items[{i}]";

            if (node == null)
            {
                Error(nodePath, "An item is missing.");
                continue;
            }

            IFilterItem? item = node.Type switch
            {
                FilterNode.ConditionType => CompileCondition(
                    node,
                    typeId,
                    nodePath
                ),
                FilterNode.GroupType => CompileGroup(
                    node,
                    typeId,
                    nodePath,
                    depth
                ),
                _ => ErrorItem(
                    nodePath,
                    "An item must be a condition or a group."
                ),
            };

            if (item == null)
                continue;

            if (!TryParseConnector(node.Connector, out var connector))
            {
                Error(
                    nodePath,
                    "A condition can only be joined with And or Or."
                );
                continue;
            }

            item.Connector = connector;
            items.Add(item);
        }

        return items;
    }

    FilterGroup? CompileGroup(
        FilterNode node,
        string typeId,
        string path,
        int depth
    )
    {
        if (depth + 1 > options.MaxGroupDepth)
        {
            Error(path, "Groups are nested too deeply.");

            return null;
        }

        var group = new FilterGroup();
        var childType = typeId;

        if (!string.IsNullOrEmpty(node.Collection))
        {
            var field = schema.FindField(typeId, node.Collection);

            if (field is not { Kind: FieldKind.Collection, ItemType: not null })
            {
                Error(
                    path,
                    $"\"{node.Collection}\" is not a collection that can be filtered."
                );

                return null;
            }

            group.ParentPropertyExpression = field.Path + "[]";
            childType = field.ItemType;
        }

        if (node.Items is not { Count: > 0 })
        {
            Error(path, "A group needs at least one condition.");

            return null;
        }

        var errorsBefore = _errors.Count;
        var items = CompileItems(node.Items, childType, path, depth + 1);

        if (_errors.Count != errorsBefore)
            return null;

        group.Items = items;

        return group;
    }

    IFilterStatement? CompileCondition(
        FilterNode node,
        string typeId,
        string path
    )
    {
        if (++_conditions > options.MaxConditions)
        {
            if (_conditions == options.MaxConditions + 1)
                Error(
                    path,
                    $"A filter can have at most {options.MaxConditions} conditions."
                );

            return null;
        }

        var fieldPath = node.Field ?? "";
        var field = schema.FindField(typeId, fieldPath);

        if (field == null || field.Kind == FieldKind.Collection)
            return ErrorStatement(
                path,
                $"\"{fieldPath}\" is not a property that can be filtered on."
            );

        var operationName = node.Operation ?? "";

        if (
            !field.Operations.Contains(operationName)
            || !OperationCatalog.TryGet(operationName, out var descriptor)
            || !OperationRegistry.TryGet(operationName, out var operation)
        )
            return ErrorStatement(
                path,
                $"\"{operationName}\" cannot be used on {field.Label}."
            );

        if (!TryReadValues(node, field, descriptor, path, out var values))
            return null;

        if (
            !TryReadOptions(
                node,
                field,
                descriptor,
                path,
                out var statementOptions
            )
        )
            return null;

        try
        {
            return CreateStatement(field, operation, values, statementOptions);
        }
        catch (ArgumentException ex)
        {
            // The operation's own validation, such as the wrong number of values
            return ErrorStatement(path, ex.Message);
        }
    }

    bool TryReadValues(
        FilterNode node,
        FieldSchema field,
        OperationDescriptor descriptor,
        string path,
        out List<object> values
    )
    {
        values = [];

        var posted = node.Values ?? [];

        if (descriptor.Arity == ValueArity.None)
            return true;

        var expected = descriptor.Arity switch
        {
            ValueArity.One => 1,
            ValueArity.Two => 2,
            _ => 0,
        };

        if (
            (expected > 0 && posted.Count != expected)
            || (expected == 0 && posted.Count == 0)
        )
        {
            Error(
                path,
                expected switch
                {
                    1 => $"{descriptor.Label} needs one value.",
                    2 => $"{descriptor.Label} needs two values.",
                    _ => $"{descriptor.Label} needs at least one value.",
                }
            );

            return false;
        }

        if (posted.Count > options.MaxValuesPerCondition)
        {
            Error(
                path,
                $"A condition can have at most {options.MaxValuesPerCondition} values."
            );

            return false;
        }

        for (var i = 0; i < posted.Count; i++)
        {
            if (
                !TryParseValue(posted[i], field, out var value, out var problem)
            )
            {
                Error($"{path}.values[{i}]", problem);

                return false;
            }

            values.Add(value);
        }

        return true;
    }

    bool TryReadOptions(
        FilterNode node,
        FieldSchema field,
        OperationDescriptor descriptor,
        string path,
        out FilterStatementOptions? statementOptions
    )
    {
        statementOptions = null;

        Matches? match = null;

        if (descriptor.SupportsMatch && !string.IsNullOrEmpty(node.Match))
        {
            if (
                !Enum.TryParse(node.Match, ignoreCase: true, out Matches parsed)
                || !Enum.IsDefined(parsed)
            )
            {
                Error(path, "Choose to match any, or all, of the values.");

                return false;
            }

            match = parsed;
        }

        List<IEntityManipulator>? manipulators = null;

        if (node.Manipulators is { Count: > 0 })
        {
            if (field.Kind != FieldKind.String)
            {
                Error(path, "Text can only be adjusted on a text property.");

                return false;
            }

            if (node.Manipulators.Count > 4)
            {
                Error(path, "Too many adjustments were asked for.");

                return false;
            }

            manipulators = [];

            foreach (var posted in node.Manipulators)
            {
                var manipulator = CreateManipulator(posted, path);

                if (manipulator == null)
                    return false;

                manipulators.Add(manipulator);
            }
        }

        if (match != null || manipulators != null)
            statementOptions = new FilterStatementOptions
            {
                Match = match,
                Manipulators = manipulators,
            };

        return true;
    }

    IEntityManipulator? CreateManipulator(ManipulatorNode? posted, string path)
    {
        // Only these are offered. A posted name can never reach any other method.
        switch (posted?.Name)
        {
            case ManipulatorNode.Trim:
            case ManipulatorNode.ToLower:
            case ManipulatorNode.ToUpper:
                return new ExpressionMethodManipulator(
                    typeof(string).GetMethod(posted.Name, Type.EmptyTypes)!,
                    []
                );

            case ManipulatorNode.Replace:
                if (
                    string.IsNullOrEmpty(posted.Find)
                    || posted.Find.Length > options.MaxValueLength
                    || (posted.With?.Length ?? 0) > options.MaxValueLength
                )
                {
                    Error(path, "Replacing text needs the text to find.");

                    return null;
                }

                return new ReplaceManipulator(posted.Find, posted.With ?? "");

            default:
                Error(path, "That adjustment is not available.");

                return null;
        }
    }

    bool TryParseValue(
        string? text,
        FieldSchema field,
        out object value,
        out string problem
    )
    {
        value = null!;
        problem = "";

        if (string.IsNullOrEmpty(text))
        {
            problem = "A value is required.";

            return false;
        }

        if (text.Length > options.MaxValueLength)
        {
            problem =
                $"A value can be at most {options.MaxValueLength} characters.";

            return false;
        }

        var type = field.ValueType;
        var culture = CultureInfo.InvariantCulture;
        object? parsed = null;

        switch (field.Kind)
        {
            case FieldKind.String:
                parsed = text;
                break;

            case FieldKind.Boolean:
                if (bool.TryParse(text, out var boolean))
                    parsed = boolean;
                break;

            case FieldKind.Guid:
                if (Guid.TryParse(text, out var guid))
                    parsed = guid;
                break;

            case FieldKind.Enum:
                // Only members that were offered, by name - never a number or a combination
                if (field.Options?.Any(o => o.Value == text) == true)
                    parsed = Enum.Parse(type, text);
                break;

            case FieldKind.Number:
                parsed = ParseNumber(text, type, culture);
                break;

            case FieldKind.Date:
            case FieldKind.DateTime:
                parsed = ParseDate(text, type, culture);
                break;

            case FieldKind.Time:
                parsed = ParseTime(text, type, culture);
                break;
        }

        if (parsed == null)
        {
            problem =
                $"\"{Shorten(text)}\" is not a valid {field.Kind.ToString().ToLowerInvariant()} for {field.Label}.";

            return false;
        }

        value = parsed;

        return true;
    }

    static object? ParseNumber(string text, Type type, CultureInfo culture)
    {
        const NumberStyles Integer = NumberStyles.AllowLeadingSign;
        const NumberStyles Float = NumberStyles.Float;

        if (type == typeof(int))
            return int.TryParse(text, Integer, culture, out var i) ? i : null;
        if (type == typeof(long))
            return long.TryParse(text, Integer, culture, out var l) ? l : null;
        if (type == typeof(short))
            return short.TryParse(text, Integer, culture, out var s) ? s : null;
        if (type == typeof(byte))
            return byte.TryParse(text, Integer, culture, out var b) ? b : null;
        if (type == typeof(sbyte))
            return sbyte.TryParse(text, Integer, culture, out var sb)
                ? sb
                : null;
        if (type == typeof(uint))
            return uint.TryParse(text, Integer, culture, out var ui)
                ? ui
                : null;
        if (type == typeof(ulong))
            return ulong.TryParse(text, Integer, culture, out var ul)
                ? ul
                : null;
        if (type == typeof(ushort))
            return ushort.TryParse(text, Integer, culture, out var us)
                ? us
                : null;
        if (type == typeof(decimal))
            return decimal.TryParse(text, Float, culture, out var m) ? m : null;
        if (type == typeof(double))
            return
                double.TryParse(text, Float, culture, out var d)
                && double.IsFinite(d)
                ? d
                : null;
        if (type == typeof(float))
            return
                float.TryParse(text, Float, culture, out var f)
                && float.IsFinite(f)
                ? f
                : null;

        return null;
    }

    static object? ParseDate(string text, Type type, CultureInfo culture)
    {
        if (type == typeof(DateOnly))
            return DateOnly.TryParseExact(
                text,
                "yyyy-MM-dd",
                culture,
                DateTimeStyles.None,
                out var d
            )
                ? d
                : null;

        if (type == typeof(DateTimeOffset))
            return DateTimeOffset.TryParseExact(
                text,
                DateTimeFormats,
                culture,
                DateTimeStyles.AssumeUniversal,
                out var o
            )
                ? o
                : null;

        return DateTime.TryParseExact(
            text,
            DateTimeFormats,
            culture,
            DateTimeStyles.None,
            out var dt
        )
            ? dt
            : null;
    }

    static object? ParseTime(string text, Type type, CultureInfo culture)
    {
        if (type == typeof(TimeOnly))
            return TimeOnly.TryParseExact(
                text,
                TimeFormats,
                culture,
                DateTimeStyles.None,
                out var t
            )
                ? t
                : null;

        // Durations are posted as a time of day: 01:30 or 01:30:15
        return TimeSpan.TryParseExact(
            text,
            ["hh\\:mm", "hh\\:mm\\:ss", "hh\\:mm\\:ss\\.FFFFFFF"],
            culture,
            out var ts
        )
            ? ts
            : null;
    }

    static IFilterStatement CreateStatement(
        FieldSchema field,
        IOperation operation,
        List<object> values,
        FilterStatementOptions? statementOptions
    )
    {
        // The statement is generic over the type of the property, which is only known now. The values
        // were parsed as that type, so they fit.
        var valueType =
            field.Kind == FieldKind.Object ? typeof(object) : field.ValueType;
        var array = Array.CreateInstance(valueType, values.Count);

        for (var i = 0; i < values.Count; i++)
            array.SetValue(values[i], i);

        return (IFilterStatement)
            Activator.CreateInstance(
                typeof(FilterStatement<>).MakeGenericType(valueType),
                field.Path,
                operation,
                array,
                statementOptions,
                Connector.And
            )!;
    }

    static bool TryParseConnector(string? text, out Connector connector)
    {
        connector = Connector.And;

        return string.IsNullOrEmpty(text)
            || (
                Enum.TryParse(text, ignoreCase: true, out connector)
                && Enum.IsDefined(connector)
            );
    }

    static string Shorten(string text) =>
        text.Length <= 30 ? text : text[..30] + "…";

    void Error(string path, string message) => _errors.Add(new(path, message));

    FilterGroup? ErrorItem(string path, string message)
    {
        Error(path, message);

        return null;
    }

    IFilterStatement? ErrorStatement(string path, string message)
    {
        Error(path, message);

        return null;
    }
}
