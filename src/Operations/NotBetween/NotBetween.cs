namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct NotBetween : IOperation
{
    public NotBetween() { }

    public readonly string Name => "NotBetween";

    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.All,
            NullHandler = OperationNullHandler.IsNullOr,
        };

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    ) =>
        Expression.Not(
            Expression.AndAlso(
                Expression.GreaterThanOrEqual(
                    member,
                    Expression.Constant(
                        options.ApplyManipulators(values.ElementAt(0))
                    )
                ),
                Expression.LessThanOrEqual(
                    member,
                    Expression.Constant(
                        options.ApplyManipulators(values.ElementAt(1))
                    )
                )
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
