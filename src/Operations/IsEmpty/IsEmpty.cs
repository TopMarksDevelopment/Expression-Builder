namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The text is empty.</summary>
[GenerateFilterExtensions(FilterExtensionShape.None)]
public struct IsEmpty : IOperation
{
    /// <summary>Creates the <see cref="IsEmpty"/> operation.</summary>
    public IsEmpty() { }

    /// <inheritdoc />
    public readonly string Name => "IsEmpty";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.All,
            NullHandler = OperationNullHandler.NotNullAnd,
        };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> _,
        IFilterStatementOptions? __
    ) => Expression.Equal(member.ToStringExpression(), Expression.Constant(""));

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
