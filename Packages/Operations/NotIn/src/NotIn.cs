namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct NotIn : IOperation
{
    public NotIn() { }

    public readonly string Name => "NotIn";

    public Matches Match { get; set; } = Matches.All;

    public bool SkipNullMemberChecks { get; set; } = false;

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    ) =>
        Expression.Not(
            new In().Build(
                member,
                values,
                manipulators
            )
        );

    public readonly void Validate(IFilterStatement statement)
    {
        if (Match == Matches.Any)
            throw new ArgumentException(
                nameof(Match),
                "This method can only match `All`"
            );

        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "There must be values to match"
            );
    }
}
