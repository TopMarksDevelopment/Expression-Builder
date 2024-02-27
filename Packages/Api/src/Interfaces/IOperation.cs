namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

public interface IOperation
{
    string Name { get; }

    Matches Match { get; set; }

    bool SkipNullMemberChecks { get; set; }

    /// <summary>
    /// Operation builder
    /// </summary>
    /// <param name="member">This is a safe <see cref="MemberExpression" />/<see cref="ParameterExpression" /> to work on</param>
    /// <param name="values">The collection of values we're working on</param>
    /// <param name="matches">How multiple values will be matched</param>
    /// <returns>Some kind of true/false <see cref="Expression" /></returns>
    Expression Build<TPropertyType>(
        Expression member, 
        IFilterCollection<TPropertyType?> values,
        IEnumerable<IEntityManipulator>? manipulators
    );
    
    void Validate(IFilterStatement statement);
}