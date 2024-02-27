namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct IsNullOrWhiteSpace : IOperation
{
    public IsNullOrWhiteSpace() { }

    public readonly string Name => "IsNullOrWhiteSpace";

    public Matches Match { get; set; } = Matches.All;

    public bool SkipNullMemberChecks { get; set; } = true;

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    ) =>
        Expression.OrElse(
            Expression.Equal(
                member,
                Expression.Constant(null)
            ),
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
