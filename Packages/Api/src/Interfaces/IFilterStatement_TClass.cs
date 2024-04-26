namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

/// <summary>
/// A filter statement
/// </summary>
/// <typeparam name="TPropertyType">The underlying type of the property and its' values</typeparam>
public interface IFilterStatement<TPropertyType> : IFilterStatement
{
    /// <summary>
    /// The underlying type of the property (i.e. <c>System.String</c>)
    /// </summary>
    string propertyType { get; }

    /// <summary>
    /// The set of values we'll be comparing to/with
    /// </summary>
    new IFilterCollection<TPropertyType?> Values { get; set; }

    /// <summary>
    /// A helper method for creating an
    /// <c>IFilterStatement&lt;TPropertyType&gt;</c>
    /// from an expression instead of a string
    /// </summary>
    /// <typeparam name="TClass">The class of the <c>IFilter&lt;TClass&gt;</c></typeparam>
    /// <param name="propertyExpression">The expression pointing to the field/property</param>
    /// <param name="operation">The <see cref="IOperation"/> we're performing</param>
    /// <param name="values">The values to apply</param>
    /// <param name="options">Any <see cref="IFilterStatementOptions"/> to apply</param>
    /// <param name="connector">The way we're connecting to the next statement</param>
    /// <returns>A statement with a string interperation of the property expression</returns>
    static abstract IFilterStatement<TPropertyType> Create<TClass>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[]? values = null,
        IFilterStatementOptions? options = null,
        Connector connector = Connector.And
    )
        where TClass : class;
}
