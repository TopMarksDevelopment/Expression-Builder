namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

/// <summary>
/// An operation that is performed within a <see cref="IFilterStatement"/>
/// </summary>
public interface IOperation
{
    /// <summary>
    /// The unique name for this operation
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The default options for this operation
    /// </summary>
    OperationDefaults Defaults { get; }

    /// <summary>
    /// The method that builds/applies the operation we're performing
    /// </summary>
    /// <remarks>
    /// Note: the <see cref="member"/> has already had any and all
    /// <see cref="IEntityManipulator"/>'s applied
    /// </remarks>
    /// <param name="member">
    /// This is a safe <see cref="MemberExpression" />/
    /// <see cref="ParameterExpression" /> to work on
    /// </param>
    /// <param name="values">The collection of values we're working on</param>
    /// <param name="options">Options to apply when building the expression</param>
    /// <returns>Some kind of true/false <see cref="Expression" /></returns>
    Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    );

    /// <summary>
    /// Confirm the statement works with this operation
    /// </summary>
    /// <param name="statement">The statement to check</param>
    void Validate(IFilterStatement statement);
}
