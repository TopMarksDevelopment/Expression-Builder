namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct IsEmpty : IOperation
{
    public IsEmpty() { }

    public readonly string Name => "IsEmpty";

    public Matches Match { get; set; } = Matches.All;

    public bool SkipNullMemberChecks { get; set; } = false;

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    ) =>
        Expression.Equal(
            member.ToStringExpression(),
            Expression.Constant("")
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
