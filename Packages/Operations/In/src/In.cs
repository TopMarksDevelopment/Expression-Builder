namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct In : IOperation
{
    public In() { }

    public readonly string Name => "In";

    public Matches Match { get; set; } = Matches.Any;

    public bool SkipNullMemberChecks { get; set; } = false;

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    )
    {
        var containsMethod = values
            .GetType()
            .GetMethod("Contains", [typeof(TPropertyType)]);

        if (containsMethod == null)
            throw new MissingMethodException(
                $"No contains method for {typeof(TPropertyType?)}"
            );

        return Expression.Call(
            Expression.Constant(values),
            containsMethod,
            Nullable.GetUnderlyingType(
                containsMethod.ReflectedType!.GenericTypeArguments[0]
            ) == null || member.Type == typeof(TPropertyType?)
                ? member
                : Expression.Convert(member, typeof(TPropertyType?))
        );
    }

    public readonly void Validate(IFilterStatement statement)
    {
        if (Match == Matches.All)
            throw new ArgumentOutOfRangeException(
                nameof(Match),
                "This method can only match `Any`"
            );

        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "There must be values to match"
            );
    }
}
