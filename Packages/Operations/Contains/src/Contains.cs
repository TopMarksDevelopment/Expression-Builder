namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using H = Api.Helpers.HelperMethods;

public struct Contains : IOperation
{
    public Contains() { }

    public readonly string Name => "Contains";

    public Matches Match { get; set; } = Matches.Any;

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

        return member
            .ToTrimLowerStringExpression()
            .WorkOnValues(
                values,
                Match,
                (m, v) =>
                {
                    var value = v.ToTrimLowerStringConstant();

                    if (manipulators?.Any() == true)
                        foreach (var manip in manipulators)
                            value = manip.ManipulateExpression(value);

                    return Expression.Call(m, H.ContainsMethod, value);
                }
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
