using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

namespace TopMarksDevelopment.ExpressionBuilder;

public struct ReplaceManipulator : IEntityManipulator
{
    public readonly string Name => "ReplaceManipulator";

    public readonly string? TypeName => null;

    public object?[] Arguments { get; set; }

    public ReplaceManipulator() => Arguments = [];

    public ReplaceManipulator(string searchterm, string replacementTerm) =>
        Arguments = [ searchterm, replacementTerm ];

    public readonly Expression ManipulateExpression(Expression member)
        => Expression.Call(
            member.ToStringExpression(),
            typeof(string).GetMethod("Replace", [typeof(string), typeof(string)])!,
            Expression.Constant(Arguments.ElementAt(0)),
            Expression.Constant(Arguments.ElementAt(1))
        );

    public readonly void Validate()
    {
        if (Arguments.Length != 2)
            throw new InvalidDataException("Only 2 values are permitted");
    }
}