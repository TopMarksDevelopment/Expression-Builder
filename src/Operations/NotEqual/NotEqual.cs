namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct NotEqual : IOperation
{
    public NotEqual() { }

    public readonly string Name => "NotEqual";

    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.All, NullHandler = OperationNullHandler.Skip };

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    ) =>
        values.Count == 1
            ? Expression.NotEqual(
                member,
                Expression.Constant(
                    options.ApplyManipulators(values.ElementAt(0))
                )
            )
        : options?.Match == Matches.Any
            ? member.WorkOnValues(
                values,
                Matches.Any,
                (m, v) =>
                    Expression.NotEqual(
                        m,
                        Expression.Constant(options?.ApplyManipulators(v))
                    )
            )
        : Expression.Not(new In().Build(member, values, options));

    public readonly void Validate(IFilterStatement statement)
    {
        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must have at least one value"
            );
    }
}
