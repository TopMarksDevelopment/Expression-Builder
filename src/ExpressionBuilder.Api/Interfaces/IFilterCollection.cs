namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections;

/// <summary>
/// A collection applied on a <see cref="IFilterStatement"/>
/// </summary>
public interface IFilterCollection : ICollection
{
    /// <summary>
    /// Checks if there are any elements inside the collection
    /// </summary>
    /// <returns>True if there is anything insdide the collection</returns>
    abstract bool Any();

    /// <summary>
    /// Adds a set of items to the collection.
    /// </summary>
    /// <param name="items">The item set we're adding</param>
    void AddRange(IEnumerable items);

    /// <summary>
    /// A friendly display of the collection
    /// </summary>
    /// <remarks>Usually a comma separeted list enclosed in square brackets</remarks>
    /// <example>[1, 2, 3]</example>
    /// <returns>A string representation of the collection</returns>
    abstract string ToString();
}
