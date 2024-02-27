namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using H = Api.Helpers.HelperMethods;

public struct EndsWith : IOperation
{
    public EndsWith() { }

    public readonly string Name => "EndsWith";

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
                    var myValue = v.ToTrimLowerStringConstant();

                    if (manipulators?.Any() == true)
                        foreach (var manip in manipulators)
                            myValue = manip.ManipulateExpression(myValue);

                    return Expression.Call(
                        m,
                        H.EndsWithMethod,
                        v.ToTrimLowerStringConstant()
                    );
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

        if (Match == Matches.All && statement.Values.Count > 1)
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "No more than one value when matching `All`"
            );
    }
}
