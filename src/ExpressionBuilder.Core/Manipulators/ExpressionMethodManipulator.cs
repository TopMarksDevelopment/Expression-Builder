using System.Linq.Expressions;
using System.Reflection;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Extensions;

namespace TopMarksDevelopment.ExpressionBuilder;

/// <summary>
/// A manipulator that calls a method, such as <c>Trim()</c>, on the property and values.
/// </summary>
/// <param name="methodInfo">The method to call</param>
/// <param name="arguments">The arguments to call it with</param>
public struct ExpressionMethodManipulator(
    MethodInfo methodInfo,
    IEnumerable<ConstantExpression> arguments
) : IEntityManipulator
{
    readonly Type? _type = methodInfo.DeclaringType;

    internal readonly string? TypeName => _type?.FullName;

    /// <inheritdoc />
    public readonly string Name => methodInfo.Name;

    /// <inheritdoc />
    public readonly Type[] ExpectedTypes =>
        arguments.Select(x => x.Type).ToArray();

    /// <inheritdoc />
    public object?[] Arguments
    {
        readonly get => arguments.Select(x => x.Value).ToArray();
        set => arguments = value.Select(Expression.Constant);
    }

    /// <inheritdoc />
    public readonly Expression ManipulateMember(Expression member) =>
        Expression.Call(
            member,
            methodInfo,
            Arguments.Select(Expression.Constant)
        );

    /// <inheritdoc />
    public readonly object? ManipulateValue<TPropertyType>(
        TPropertyType? value
    ) => methodInfo.Invoke(value, Arguments);

    /// <inheritdoc />
    public readonly void Validate()
    {
        ArgumentNullException.ThrowIfNull(_type);
        ArgumentNullException.ThrowIfNull(methodInfo);

        // A manipulator is a method of the value it is applied to. A static method has no such value, and
        // when a saved filter is read the method is named by the data, so this keeps the data from choosing
        // any static method (such as Environment.GetEnvironmentVariable) to be run
        if (methodInfo.IsStatic)
            throw new InvalidOperationException(
                $"{_type.Name}.{Name} is static. A manipulator must be a method of the value it changes."
            );
    }

    /// <summary>
    /// Chain a set of methods we want to apply to the memeber and values
    /// </summary>
    /// <typeparam name="T">The expected input type</typeparam>
    /// <param name="expression">The expression which manipulates the member</param>
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
                        mcExp.Arguments.Select(x => x.ToConstant()).ToArray()
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
