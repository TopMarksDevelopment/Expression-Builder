namespace TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// A simple statement used to perform an operation on a property
/// </summary>
public interface IFilterStatement : IFilterItem
{
    /// <summary>
    /// The name used to identify the property
    /// </summary>
    string PropertyId { get; set; }

    /// <summary>
    /// The operation we're going to be performing
    /// </summary>
    IOperation Operation { get; set; }

    /// <summary>
    /// The set of values we'll be comparing to/with
    /// </summary>
    IFilterCollection Values { get; set; }

    /// <summary>
    /// A set of methods we can use to manipulate the property<br/>
    /// (<b>Note:</b> not the values)
    /// </summary>
    IEnumerable<IEntityManipulator> Manipulators { get; set; }
}