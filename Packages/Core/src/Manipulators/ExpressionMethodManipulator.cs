using System.Linq.Expressions;
using System.Reflection;
using TopMarksDevelopment.ExpressionBuilder.Api;

namespace TopMarksDevelopment.ExpressionBuilder;

public struct ExpressionMethodManipulator(
    MethodInfo methodInfo,
    IEnumerable<ConstantExpression> arguments
) : IEntityManipulator
{
    readonly Type? _type = methodInfo.DeclaringType;
    readonly MethodInfo _info = methodInfo;
    IEnumerable<ConstantExpression> _expressions = arguments;

    internal readonly string? TypeName => _type?.FullName;

    internal readonly IEnumerable<string>? ArgumentTypes =>
        _expressions.Select(x => x.Type.FullName ?? x.Type.Name);

    public string Name { get; set; } = methodInfo.Name;

    public object?[] Arguments
    {
        readonly get => _expressions.Select(x => x.Value).ToArray();
        set => _expressions = value.Select(Expression.Constant);
    }

    public readonly Expression ManipulateExpression(Expression member) =>
        _info == null
            ? throw new MissingMethodException(
                $"Can not find the {Name} method"
            )
            : Expression.Call(member, _info, _expressions);

    public readonly void Validate()
    {
        ArgumentNullException.ThrowIfNull(_type);
        ArgumentNullException.ThrowIfNull(_info);
    }
}
