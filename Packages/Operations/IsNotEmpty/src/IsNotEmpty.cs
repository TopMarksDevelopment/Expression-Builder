namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public struct IsNotEmpty : IOperation
{
    static IsEmpty? _original;
    static IsEmpty Original => _original ??= new();

    public IsNotEmpty() { }

    public readonly string Name => "IsNotEmpty";

    public Matches Match { get; set; } = Matches.All;

    public bool SkipNullMemberChecks { get; set; } = Original.SkipNullMemberChecks;

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    ) => Expression.Not(Original.Build(member, values, manipulators));

    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must not have any values"
            );
    }
}
