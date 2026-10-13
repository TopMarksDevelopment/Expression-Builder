namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The value is <c>null</c>.</summary>
[GenerateFilterExtensions(FilterExtensionShape.None)]
public struct IsNull : IOperation
{
    /// <summary>Creates the <see cref="IsNull"/> operation.</summary>
    public IsNull() { }

    /// <inheritdoc />
    public readonly string Name => "IsNull";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.All, NullHandler = OperationNullHandler.Skip };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> _,
        IFilterStatementOptions? __
    ) => Expression.Equal(member, Expression.Constant(null));

    /// <inheritdoc />
    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must not have any values"
            );
    }
}
