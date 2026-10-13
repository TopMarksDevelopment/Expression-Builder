namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The text is neither <c>null</c>, empty nor only white space.</summary>
[GenerateFilterExtensions(FilterExtensionShape.None)]
public struct IsNotNullOrWhiteSpace : IOperation
{
    /// <summary>Creates the <see cref="IsNotNullOrWhiteSpace"/> operation.</summary>
    public IsNotNullOrWhiteSpace() { }

    /// <inheritdoc />
    public readonly string Name => "IsNotNullOrWhiteSpace";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.All, NullHandler = OperationNullHandler.Skip };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> _,
        IFilterStatementOptions? __
    ) =>
        Expression.AndAlso(
            Expression.NotEqual(member, Expression.Constant(null)),
            Expression.NotEqual(
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
