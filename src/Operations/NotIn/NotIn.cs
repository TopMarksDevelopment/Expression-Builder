namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct NotIn : IOperation
{
    public NotIn() { }

    public readonly string Name => "NotIn";

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
    ) => Expression.Not(new In().Build(member, values, options));

    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Options?.Match == Matches.Any)
            throw new ArgumentException(
                nameof(statement.Options.Match),
                "This method can only match `All`"
            );

        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "There must be values to match"
            );
    }
}
