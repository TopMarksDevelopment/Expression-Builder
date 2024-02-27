namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections.Generic;

public interface IFilterCollection<TPropertyType>
    : IFilterCollection,
        ICollection<TPropertyType?>
{
    //
    // Summary:
    //     Gets the number of elements contained in the System.Collections.Generic.ICollection`1.
    //
    //
    // Returns:
    //     The number of elements contained in the System.Collections.Generic.ICollection`1.
    new int Count { get; }
}
