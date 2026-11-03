namespace TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// Describes an operation to the filter builder: what to call it, how many values it takes and which
/// kinds of property it can be used on.
/// </summary>
/// <remarks>
/// An <see cref="Api.IOperation"/> only knows how to build an expression, not how it should be
/// presented. Describe your own operations by registering them with
/// <see cref="OperationCatalog.Register(OperationDescriptor)"/>, or for one builder only with
/// <see cref="FilterBuilderOptions.AddOperation(OperationDescriptor)"/>. They are always listed apart
/// from the built-in operations.
/// </remarks>
/// <param name="Name">The <see cref="Api.IOperation.Name"/> of the operation</param>
/// <param name="Label">What the user sees</param>
/// <param name="Arity">How many values it takes</param>
/// <param name="Kinds">The kinds of property it works on</param>
/// <param name="NullableOnly">It only makes sense for properties that can be <c>null</c></param>
/// <param name="SupportsMatch">
/// With several values, the user can choose whether any or all of them must match
/// </param>
/// <param name="Group">
/// The heading a custom operation is listed under. <see cref="FilterBuilderLabels.CustomGroup"/> if
/// omitted. The built-in operations ignore it.
/// </param>
public sealed record OperationDescriptor(
    string Name,
    string Label,
    ValueArity Arity,
    IReadOnlySet<FieldKind> Kinds,
    bool NullableOnly = false,
    bool SupportsMatch = false,
    string? Group = null
)
{
    /// <summary>
    /// Whether this is one of the operations that ships with the package, rather than one added by the
    /// app. Built-in operations cannot be replaced or removed, only left out of a builder.
    /// </summary>
    public bool IsBuiltIn { get; internal init; }

    /// <summary>
    /// Whether the operation can be used on a property.
    /// </summary>
    /// <param name="kind">The kind of the property</param>
    /// <param name="nullable">Whether the property can be <c>null</c></param>
    public bool AppliesTo(FieldKind kind, bool nullable) =>
        Kinds.Contains(kind) && (nullable || !NullableOnly);
}
