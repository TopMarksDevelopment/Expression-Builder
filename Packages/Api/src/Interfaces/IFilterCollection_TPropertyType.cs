namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections.Generic;

public interface IFilterCollection<TPropertyType>
    : IFilterCollection,
        ICollection<TPropertyType?>
{
    /// <summary>
    /// Gets the number of elements contained in the collection.
    /// </summary>
    /// <returns>
    /// The number of elements contained in the collection
    /// </returns>
    new int Count { get; }
}
