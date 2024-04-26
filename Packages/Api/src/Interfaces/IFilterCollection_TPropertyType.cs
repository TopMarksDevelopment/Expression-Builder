namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections.Generic;

/// <summary>
/// A collection applied on a <see cref="IFilterStatement"/>
/// </summary>
/// <typeparam name="TPropertyType">The type of the values inside the collection</typeparam>
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
