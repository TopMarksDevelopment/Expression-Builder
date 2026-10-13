namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The text is <c>null</c>, empty or only white space.</summary>
[GenerateFilterExtensions(FilterExtensionShape.None)]
public struct IsNullOrWhiteSpace : IOperation
{
    /// <summary>Creates the <see cref="IsNullOrWhiteSpace"/> operation.</summary>
    public IsNullOrWhiteSpace() { }

    /// <inheritdoc />
    public readonly string Name => "IsNullOrWhiteSpace";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.All, NullHandler = OperationNullHandler.Skip };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> _,
        IFilterStatementOptions? __
    ) =>
        Expression.OrElse(
            Expression.Equal(member, Expression.Constant(null)),
            Expression.Equal(
                member.ToTrimLowerStringExpression(),
                Expression.Constant("")
            )
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
