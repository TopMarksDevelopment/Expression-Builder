namespace TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// Keeps a property out of the filter builder: it is not offered, and a posted filter that uses it is
/// rejected.
/// </summary>
/// <remarks>
/// Use it for anything that should not be searchable by the people using the page, such as hashes,
/// internal costs or tokens. Even when a value is never shown, being able to filter on it reveals
/// it (<c>Token starts with "a"</c>).
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class FilterBuilderIgnoreAttribute : Attribute { }
