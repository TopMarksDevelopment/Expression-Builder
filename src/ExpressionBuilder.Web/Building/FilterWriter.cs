namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Globalization;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// Turns an existing <see cref="Filter{TClass}"/> back into a <see cref="FilterDocument"/>, so a
/// saved filter can be shown (and changed) in the filter builder.
/// </summary>
/// <remarks>
/// It is lenient: it describes what it can. Anything the builder cannot show - a property it does not
/// offer, or a manipulator it does not know - makes it throw, rather than quietly showing a filter
/// that means something different from the one that was saved.
/// </remarks>
sealed class FilterWriter(FilterSchema schema)
{
    public FilterDocument Write(IEnumerable<IFilterItem> items) =>
        new()
        {
            Root = new FilterNode
            {
                Type = FilterNode.GroupType,
                Connector = nameof(Connector.And),
                Items = WriteItems(items, schema.Root),
            },
        };

    List<FilterNode> WriteItems(
        IEnumerable<IFilterItem> items,
        string typeId
    ) =>
        [
            .. items.Select(item =>
                item switch
                {
                    IFilterGroup group => WriteGroup(group, typeId),
                    IFilterStatement statement => WriteCondition(
                        statement,
                        typeId
                    ),
                    _ => throw new NotSupportedException(
                        $"A {item.GetType().Name} can't be shown in the filter builder."
                    ),
                }
            ),
        ];

    FilterNode WriteGroup(IFilterGroup group, string typeId)
    {
        var childType = typeId;
        string? collection = null;

        if (!string.IsNullOrEmpty(group.ParentPropertyExpression))
        {
            collection = group.ParentPropertyExpression.EndsWith(
                "[]",
                StringComparison.Ordinal
            )
                ? group.ParentPropertyExpression[..^2]
                : group.ParentPropertyExpression;

            childType =
                schema.FindField(typeId, collection)?.ItemType
                ?? throw new NotSupportedException(
                    $"\"{collection}\" is not a collection the filter builder offers."
                );
        }

        return new FilterNode
        {
            Type = FilterNode.GroupType,
            Connector = group.Connector.ToString(),
            Collection = collection,
            Items = WriteItems(group.Items, childType),
        };
    }

    FilterNode WriteCondition(IFilterStatement statement, string typeId)
    {
        var field =
            schema.FindField(typeId, statement.PropertyId)
            ?? throw new NotSupportedException(
                $"\"{statement.PropertyId}\" is not a property the filter builder offers."
            );

        return new FilterNode
        {
            Type = FilterNode.ConditionType,
            Connector = statement.Connector.ToString(),
            Field = field.Path,
            Operation = statement.Operation.Name,
            Values =
            [
                .. statement
                    .Values.Cast<object?>()
                    .Select(v => Format(v, field)),
            ],
            Match = statement.Options?.Match?.ToString(),
            Manipulators = WriteManipulators(statement.Options),
        };
    }

    List<ManipulatorNode>? WriteManipulators(IFilterStatementOptions? options)
    {
        if (options?.Manipulators?.Any() != true)
            return null;

        return
        [
            .. options.Manipulators.Select(m =>
            {
                foreach (var descriptor in schema.ManipulatorDescriptors.Values)
                    if (descriptor.Read?.Invoke(m) is { } text)
                        return new ManipulatorNode
                        {
                            Name = descriptor.Name,
                            Args = text.Count > 0 ? [.. text] : null,
                        };

                throw new NotSupportedException(
                    $"The \"{m.Name}\" adjustment can't be shown in the filter builder."
                );
            }),
        ];
    }

    static string Format(object? value, FieldSchema field)
    {
        var culture = CultureInfo.InvariantCulture;

        return value switch
        {
            null => "",
            bool b => b ? "true" : "false",
            DateTime dt when field.Kind == FieldKind.Date => dt.ToString(
                "yyyy-MM-dd",
                culture
            ),
            DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:ss", culture),
            DateTimeOffset dto => dto.UtcDateTime.ToString(
                "yyyy-MM-ddTHH:mm:ss",
                culture
            ),
            DateOnly d => d.ToString("yyyy-MM-dd", culture),
            TimeOnly t => t.ToString("HH:mm:ss", culture),
            TimeSpan ts => ts.ToString("hh\\:mm\\:ss", culture),
            Enum e => e.ToString(),
            IFormattable f => f.ToString(null, culture),
            _ => value.ToString() ?? "",
        };
    }
}
