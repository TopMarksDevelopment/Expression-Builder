namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Core functions of a group collection
/// </summary>
public interface IFilterGroup : IFilterItem
{
    /// <summary>
    /// The group that contains this one
    /// </summary>
    [JsonIgnore]
    IFilterGroup? LastGroup { get; set; }

    /// <summary>
    /// Path to the target collection, if we've targeted one
    /// </summary>
    string? ParentPropertyExpression { get; set; }

    /// <summary>
    /// The items within this group
    /// </summary>
    /// <remarks>Usually <see cref="IFilterStatement"/>'s or other <see cref="IFilterGroup"/>s
    ICollection<IFilterItem> Items { get; set; }

    /// <summary>
    /// A method that closes this group
    /// </summary>
    /// <returns>The <see cref="LastGroup">, if it exists</returns>
    /// <exception cref="InvalidOperationException">No group to return</exception>
    IFilterGroup Close();
}
