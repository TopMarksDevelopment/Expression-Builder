namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using H = Api.Helpers.HelperMethods;

public struct Contains : IOperation
{
    public Contains() { }

    public readonly string Name => "Contains";

    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.Any,
            NullHandler = OperationNullHandler.NotNullAnd
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
                        H.ContainsMethod,
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
    }
}
