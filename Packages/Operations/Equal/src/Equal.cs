namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public partial struct Equal : IOperation
{
    public Equal() { }

    public readonly string Name => "Equal";

    public Matches Match { get; set; } = Matches.Any;

    public bool SkipNullMemberChecks { get; set; } = false;

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    ) =>
        values.Count() > 1
            ? new In() { Match = Match }.Build(member, values, manipulators)
            : Expression.Equal(member, Expression.Constant(values.First()));

    public readonly void Validate(IFilterStatement statement)
    {
        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must have at least one value"
            );

        if (Match == Matches.All && statement.Values.Count > 1)
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "No more than one value when matching `All`"
            );
    }
}
