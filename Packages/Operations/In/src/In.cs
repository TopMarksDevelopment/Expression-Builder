namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Collections;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct In : IOperation
{
    public In() { }

    public readonly string Name => "In";

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
    )
    {
        Expression constValues;
        var EnumType = values.GetType();

        if (options != null && options.Manipulators?.Any() == true)
            if (typeof(TPropertyType) == member.Type)
            {
                var cc =
                    (IFilterCollection<TPropertyType?>)
                        Activator.CreateInstance(EnumType)!;

                foreach (var val in values)
                    cc.Add((TPropertyType?)options.ApplyManipulators(val));

                constValues = Expression.Constant(cc);
            }
            else
            {
                var cc = (IList)
                    Activator.CreateInstance(
                        typeof(TinyFilterCollection<>).MakeGenericType(
                            Nullable.GetUnderlyingType(member.Type) == null
                                ? member.Type
                                : typeof(Nullable<>).MakeGenericType(
                                    member.Type
                                )
                        )
                    )!;

                EnumType = cc.GetType();

                foreach (var val in values)
                    cc.Add(options.ApplyManipulators(val));

                constValues = Expression.Constant(cc);
            }
        else
            constValues = Expression.Constant(values);

        var containsMethod =
            EnumType.GetMethod("Contains", [member.Type])
            ?? throw new MissingMethodException(
                $"No contains method for {typeof(TPropertyType?)} collection"
            );

        return Expression.Call(
            constValues,
            containsMethod,
            Nullable.GetUnderlyingType(
                containsMethod.ReflectedType!.GenericTypeArguments[0]
            ) == null
            || member.Type == typeof(TPropertyType?)
                ? member
                : Expression.Convert(member, typeof(TPropertyType?))
        );
    }

    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Options?.Match == Matches.All)
            throw new ArgumentOutOfRangeException(
                nameof(statement.Options.Match),
                "This method can only match `Any`"
            );

        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "There must be values to match"
            );
    }
}
