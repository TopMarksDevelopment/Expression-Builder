namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections.Generic;

public interface IFilterStatementOptions
{
    /// <summary>
    /// A set of methods we can use to manipulate the property<br/>
    /// (<b>Note:</b> not the values)
    /// </summary>
    IEnumerable<IEntityManipulator>? Manipulators { get; set; }

    /// <summary>
    /// Override the default match type of the <see cref="IOperation"/> we're using
    /// </summary>
    Matches? Match { get; set; }
    
    /// <summary>
    /// If we're using a chain of properties, like <c>Birth.Country</c>, state if we should skip null checks
    /// </summary>
    bool? SkipNullChecks { get; set; }
}
