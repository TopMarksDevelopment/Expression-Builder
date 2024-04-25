namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct LessThanOrEqual : IOperation
{
    public LessThanOrEqual() { }

    public readonly string Name => "LessThanOrEqual";

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
        !values.Any()
            ? throw new ArgumentOutOfRangeException(
                nameof(values),
                "Must have at least one value"
            )
            : member.WorkOnValues(
                values,
                options?.Match ?? Defaults.Match,
                (m, v) => Expression.LessThanOrEqual(m, Expression.Constant(v))
            );

    public readonly void Validate(IFilterStatement statement)
    {
        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must have at least one value"
            );
    }
}
