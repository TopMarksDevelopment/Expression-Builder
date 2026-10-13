namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct IsNotNull : IOperation
{
    public IsNotNull() { }

    public readonly string Name => "IsNotNull";

    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.All, NullHandler = OperationNullHandler.Skip };

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> _,
        IFilterStatementOptions? __
    ) => Expression.NotEqual(member, Expression.Constant(null));

    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must not have any values"
            );
    }
}
