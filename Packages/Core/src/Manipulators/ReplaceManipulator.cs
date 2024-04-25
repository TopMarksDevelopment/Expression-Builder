using System.Linq.Expressions;
using System.Reflection;
using TopMarksDevelopment.ExpressionBuilder.Api;

namespace TopMarksDevelopment.ExpressionBuilder;

public struct ReplaceManipulator : IEntityManipulator
{
    static readonly MethodInfo _method = typeof(string).GetMethod(
        "Replace",
        [typeof(string), typeof(string)]
    )!;

    public readonly string Name => "ReplaceManipulator";

    public readonly Type[] ExpectedTypes => [typeof(string), typeof(string)];

    public object?[] Arguments { get; set; }

    public ReplaceManipulator() => Arguments = [];

    public ReplaceManipulator(string searchterm, string replacementTerm) =>
        Arguments = [searchterm, replacementTerm];

    public readonly Expression ManipulateMember(Expression member) =>
        Expression.Call(
            member.ToStringExpression(),
            _method,
            Arguments.Select(Expression.Constant)
        );

    public readonly object? ManipulateValue<TPropertyType>(
        TPropertyType? value
    ) =>
        _method.Invoke(
            typeof(TPropertyType) == typeof(string) ? value : value?.ToString(),
            Arguments
        );

    public readonly void Validate()
    {
        if (Arguments.Length != 2)
            throw new InvalidDataException("Only 2 values are permitted");
    }
}
