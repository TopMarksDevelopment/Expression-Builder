namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Settings for the filter builder: what it may filter on, how much a user may ask for, and its text.
/// </summary>
/// <remarks>
/// <para>
/// The posted filter is untrusted input. Only properties that the filter builder offers (see
/// <see cref="FilterBuilderIgnoreAttribute"/> and <see cref="Exclude{T}"/>) can be filtered on, only
/// operations that suit a property can be used, and the limits below stop an oversized filter from
/// becoming an expensive query.
/// </para>
/// </remarks>
public sealed class FilterBuilderOptions
{
    readonly HashSet<PropertyInfo> _excluded = [];
    readonly HashSet<string> _excludedOperations = new(StringComparer.Ordinal);
    readonly Dictionary<string, OperationDescriptor> _addedOperations = new(
        StringComparer.Ordinal
    );
    readonly HashSet<string> _excludedManipulators = new(
        StringComparer.Ordinal
    );
    readonly Dictionary<string, ManipulatorDescriptor> _addedManipulators = new(
        StringComparer.Ordinal
    );

    /// <summary>
    /// How deeply groups can be nested. The top level group counts as the first.
    /// </summary>
    public int MaxGroupDepth { get; set; } = 5;

    /// <summary>
    /// The most conditions a filter can have, in total.
    /// </summary>
    public int MaxConditions { get; set; } = 100;

    /// <summary>
    /// The most values a single condition can have.
    /// </summary>
    public int MaxValuesPerCondition { get; set; } = 50;

    /// <summary>
    /// The longest a single value can be, in characters.
    /// </summary>
    public int MaxValueLength { get; set; } = 500;

    /// <summary>
    /// The longest posted filter that is read, in characters.
    /// </summary>
    public int MaxPayloadLength { get; set; } = 64 * 1024;

    /// <summary>
    /// How many names a property's path can have: <c>Name</c> is one and <c>Birth.Country</c> is two.
    /// </summary>
    public int MaxNavigationDepth { get; set; } = 2;

    /// <summary>
    /// The text of the filter builder.
    /// </summary>
    public FilterBuilderLabels Labels { get; set; } = new();

    /// <summary>
    /// Replacements for the text of operations, by <see cref="Api.IOperation.Name"/>, such as
    /// <c>["Equal"] = "Is"</c>.
    /// </summary>
    public IDictionary<string, string> OperationLabels { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Replacements for the text of manipulators, by name, such as <c>["Trim"] = "Remove spaces"</c>.
    /// </summary>
    public IDictionary<string, string> ManipulatorLabels { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// The most adjustments ("manipulators") a single condition can have.
    /// </summary>
    public int MaxManipulatorsPerCondition { get; set; } = 8;

    /// <summary>
    /// Leaves operations out of the filter builder, whether built-in or custom, by
    /// <see cref="Api.IOperation.Name"/>. A post that uses one is refused.
    /// </summary>
    /// <param name="names">The names of the operations</param>
    /// <returns>These options, so calls can be chained</returns>
    public FilterBuilderOptions ExcludeOperations(params string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);

        foreach (var name in names)
            _excludedOperations.Add(name);

        return this;
    }

    /// <summary>
    /// Offers an operation of your own in this filter builder only. It must also be findable by the
    /// <see cref="OperationRegistry"/>; use <see cref="AddOperation{TOperation}"/> to do both.
    /// </summary>
    /// <param name="descriptor">The description</param>
    /// <returns>These options, so calls can be chained</returns>
    /// <exception cref="InvalidOperationException">The name belongs to a built-in operation</exception>
    public FilterBuilderOptions AddOperation(OperationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        OperationCatalog.ThrowIfBuiltIn(descriptor.Name);

        _addedOperations[descriptor.Name] = descriptor with
        {
            IsBuiltIn = false,
        };

        return this;
    }

    /// <summary>
    /// Registers an operation so it can be found by name, and offers it in this filter builder only.
    /// </summary>
    /// <typeparam name="TOperation">The operation</typeparam>
    /// <param name="descriptor">The description, whose name must be the operation's name</param>
    /// <returns>These options, so calls can be chained</returns>
    public FilterBuilderOptions AddOperation<TOperation>(
        OperationDescriptor descriptor
    )
        where TOperation : Api.IOperation, new()
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        OperationCatalog.EnsureNamesMatch<TOperation>(descriptor);
        OperationRegistry.Register<TOperation>();

        return AddOperation(descriptor);
    }

    /// <summary>
    /// Leaves manipulators out of the filter builder, whether built-in or custom, by name. A post that uses
    /// one is refused.
    /// </summary>
    /// <param name="names">The names of the manipulators</param>
    /// <returns>These options, so calls can be chained</returns>
    public FilterBuilderOptions ExcludeManipulators(params string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);

        foreach (var name in names)
            _excludedManipulators.Add(name);

        return this;
    }

    /// <summary>
    /// Offers a manipulator of your own in this filter builder only.
    /// </summary>
    /// <param name="descriptor">The description</param>
    /// <returns>These options, so calls can be chained</returns>
    /// <exception cref="InvalidOperationException">The name belongs to a built-in manipulator</exception>
    public FilterBuilderOptions AddManipulator(ManipulatorDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ManipulatorCatalog.ThrowIfBuiltIn(descriptor.Name);

        _addedManipulators[descriptor.Name] = descriptor;

        return this;
    }

    /// <summary>
    /// The operations this builder offers: the ones described for every builder, and the ones added here,
    /// less the ones left out; as long as they can be found by the <see cref="OperationRegistry"/>.
    /// </summary>
    internal IReadOnlyList<OperationDescriptor> GetOperations() =>
        [
            .. OperationCatalog
                .GetAvailable()
                .Where(o =>
                    !_excludedOperations.Contains(o.Name)
                    && !_addedOperations.ContainsKey(o.Name)
                ),
            .. _addedOperations.Values.Where(o =>
                !_excludedOperations.Contains(o.Name)
                && OperationRegistry.TryGet(o.Name, out _)
            ),
        ];

    /// <summary>
    /// The manipulators this builder offers.
    /// </summary>
    internal IReadOnlyList<ManipulatorDescriptor> GetManipulators() =>
        [
            .. ManipulatorCatalog
                .GetAvailable()
                .Where(m =>
                    !_excludedManipulators.Contains(m.Name)
                    && !_addedManipulators.ContainsKey(m.Name)
                ),
            .. _addedManipulators.Values.Where(m =>
                !_excludedManipulators.Contains(m.Name)
            ),
        ];

    /// <summary>
    /// Stops properties from being filtered on, as <see cref="FilterBuilderIgnoreAttribute"/> does
    /// for properties you own.
    /// </summary>
    /// <typeparam name="T">The class that has the properties</typeparam>
    /// <param name="properties">The properties, such as <c>p => p.Cost</c></param>
    /// <returns>These options, so calls can be chained</returns>
    public FilterBuilderOptions Exclude<T>(
        params Expression<Func<T, object?>>[] properties
    )
    {
        foreach (var property in properties)
        {
            var body = property.Body is UnaryExpression { Operand: var inner }
                ? inner
                : property.Body;

            if (
                body
                is not MemberExpression
                {
                    Member: PropertyInfo info,
                    Expression: ParameterExpression,
                }
            )
                throw new ArgumentException(
                    "Each expression must be a property, such as p => p.Name",
                    nameof(properties)
                );

            _excluded.Add(info);
        }

        return this;
    }

    internal bool IsExcluded(PropertyInfo property) =>
        property.IsDefined(typeof(FilterBuilderIgnoreAttribute), inherit: true)
        || _excluded.Any(e =>
            e.Name == property.Name
            && e.DeclaringType!.IsAssignableFrom(property.DeclaringType)
        );
}
