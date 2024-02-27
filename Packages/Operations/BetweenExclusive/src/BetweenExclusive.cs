namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct BetweenExclusive : IOperation
{
    public BetweenExclusive() { }

    public readonly string Name => "BetweenExclusive";

    public Matches Match { get; set; } = Matches.All;

    public bool SkipNullMemberChecks { get; set; } = false;

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    ) =>
        Expression.AndAlso(
            Expression.GreaterThan(
                member,
                Expression.Constant(values.ElementAt(0))
            ),
            Expression.LessThan(
                member,
                Expression.Constant(values.ElementAt(1))
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
