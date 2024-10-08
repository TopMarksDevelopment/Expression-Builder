namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using H = Api.Helpers.HelperMethods;

public struct EndsWith : IOperation
{
    public EndsWith() { }

    public readonly string Name => "EndsWith";

    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.Any,
            NullHandler = OperationNullHandler.NotNullAnd,
        };

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    ) =>
        member
            .ToTrimLowerStringExpression()
            .WorkOnValues(
                values,
                options?.Match ?? Defaults.Match,
                (m, v) =>
                    Expression.Call(
                        m,
                        H.EndsWithMethod,
                        options.ApplyManipulators(v.ToTrimLowerStringConstant())
                    )
            );

    public readonly void Validate(IFilterStatement statement)
    {
        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must have at least one value"
            );

        if (
            statement.Options?.Match == Matches.All
            && statement.Values.Count > 1
        )
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "No more than one value when matching `All`"
            );
    }
}
