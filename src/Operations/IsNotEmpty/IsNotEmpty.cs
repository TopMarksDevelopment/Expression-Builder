namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The text is not empty.</summary>
[GenerateFilterExtensions(FilterExtensionShape.None)]
public struct IsNotEmpty : IOperation
{
    /// <summary>Creates the <see cref="IsNotEmpty"/> operation.</summary>
    public IsNotEmpty() { }

    /// <inheritdoc />
    public readonly string Name => "IsNotEmpty";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.All,
            NullHandler = OperationNullHandler.IsNullOr,
        };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> _,
        IFilterStatementOptions? __
    ) =>
        Expression.NotEqual(
            member.ToStringExpression(),
            Expression.Constant("")
        );

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
