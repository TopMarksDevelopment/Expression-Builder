namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct IsNotNullOrWhiteSpace : IOperation
{
    public IsNotNullOrWhiteSpace() { }

    public readonly string Name => "IsNotNullOrWhiteSpace";

    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.All, NullHandler = OperationNullHandler.Skip };

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

    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must not have any values"
            );
    }
}
