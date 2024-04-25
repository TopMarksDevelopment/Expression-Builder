namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public readonly struct Between : IOperation
{
    public Between() { }

    public readonly string Name => "Between";

    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.All,
            NullHandler = OperationNullHandler.NotNullAnd
        };

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    ) =>
        Expression.AndAlso(
            Expression.GreaterThanOrEqual(
                member,
                Expression.Constant(
                    options.ApplyManipulators(values.ElementAt(0))
                )
            ),
            Expression.LessThanOrEqual(
                member,
                Expression.Constant(
                    options.ApplyManipulators(values.ElementAt(1))
                )
            )
        );

    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Values.Count != 2)
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Can only have 2 values"
            );
    }
}
