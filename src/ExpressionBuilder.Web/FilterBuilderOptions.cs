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
