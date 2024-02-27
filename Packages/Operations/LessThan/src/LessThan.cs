namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct LessThan : IOperation
{
    public LessThan() { }

    public readonly string Name => "LessThan";

    public Matches Match { get; set; } = Matches.All;

    public bool SkipNullMemberChecks { get; set; } = false;

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    )
    {
        if (!values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(values),
                "Must have at least one value"
            );

        return member.WorkOnValues(
            values,
            Match,
            (m, v) => Expression.LessThan(m, Expression.Constant(v))
        );
    }

    public readonly void Validate(IFilterStatement statement)
    {
        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must have at least one value"
            );
    }
}
