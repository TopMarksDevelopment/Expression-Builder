namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct DoesNotEndWith : IOperation
{
    public DoesNotEndWith() { }

    public readonly string Name => "DoesNotEndWith";

    public Matches Match { get; set; } = Matches.All;

    public bool SkipNullMemberChecks { get; set; } = false;

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    ) => Expression.Not(new EndsWith().Build(member, values, manipulators));

    public readonly void Validate(IFilterStatement statement)
    {
        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must have at least one value"
            );
    }
}
