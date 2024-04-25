namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct IsNullOrWhiteSpace : IOperation
{
    public IsNullOrWhiteSpace() { }

    public readonly string Name => "IsNullOrWhiteSpace";

    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.All, NullHandler = OperationNullHandler.Skip };

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

    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must not have any values"
            );
    }
}
