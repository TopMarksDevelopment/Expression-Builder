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

    internal readonly string? TypeName => _type?.FullName;

    public readonly string Name => methodInfo.Name;

    public readonly Type[] ExpectedTypes =>
        arguments.Select(x => x.Type).ToArray();

    public object?[] Arguments
    {
        readonly get => arguments.Select(x => x.Value).ToArray();
        set => arguments = value.Select(Expression.Constant);
    }

    public readonly Expression ManipulateMember(Expression member) =>
        Expression.Call(
            member,
            methodInfo,
            Arguments.Select(Expression.Constant)
        );

    public readonly object? ManipulateValue<TPropertyType>(
        TPropertyType? value
    ) => methodInfo.Invoke(value, Arguments);

    public readonly void Validate()
    {
        ArgumentNullException.ThrowIfNull(_type);
        ArgumentNullException.ThrowIfNull(methodInfo);
    }

    /// <summary>
    /// Chain a set of methods we want to apply to the memeber and values
    /// </summary>
    /// <typeparam name="T">The expected input type</typeparam>
    /// <param name="expr">The expression which manipulates the member</param>
    /// <returns>The <see cref="IEntityManipulator"/>'s applied in the expression</returns>
    /// <exception cref="NotSupportedException"></exception>
    public static IEnumerable<IEntityManipulator> GetAll<T>(
        Expression<Func<T, object>> expression
    )
    {
        IEnumerable<IEntityManipulator> manipulators = [];

        HandleExpression(expression.Body);

        return manipulators;

        void HandleExpression(Expression expr)
        {
            if (expr is MethodCallExpression mcExp)
            {
                manipulators = manipulators.Prepend(
                    new ExpressionMethodManipulator(
                        mcExp.Method,
                        mcExp.Arguments.Select(x => (ConstantExpression)x)
                    )
                );

                ArgumentNullException.ThrowIfNull(mcExp.Object);

                if (mcExp.Object is ParameterExpression)
                    return;

                HandleExpression(mcExp.Object);
            }
            else
                throw new NotSupportedException(
                    "You can only chain methods, no other expression is permitted"
                );
        }
    }
}
