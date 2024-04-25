namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public partial struct Equal : IOperation
{
    public Equal() { }

    public readonly string Name => "Equal";

    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.Any, NullHandler = OperationNullHandler.Skip };

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    )
    {
        if (values.Count > 1)
            return new In().Build(member, values, options);

        Expression value = Expression.Constant(values.First());

        if (
            member.Type != value.Type
            && Nullable.GetUnderlyingType(value.Type) == null
        )
            value = Expression.Convert(value, typeof(TPropertyType?));

        return Expression.Equal(member, value);
    }

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
