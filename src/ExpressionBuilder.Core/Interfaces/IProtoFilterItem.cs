using TopMarksDevelopment.ExpressionBuilder.Api;

namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

/// <summary>
/// A filter item that can be saved in, and loaded from, the compact binary (protobuf) form.
/// </summary>
// PBN0023 warns that a member declared on the interface and on an implementation is written twice. This interface
// declares no data members (only Pack and Unpack), and turning it into a base class would change the public API.
#pragma warning disable PBN0023
[ProtoBuf.ProtoContract()]
[ProtoBuf.ProtoInclude(45, typeof(ProtoFilterStatement))]
[ProtoBuf.ProtoInclude(46, typeof(FilterGroup))]
public interface IProtoFilterItem
#pragma warning restore PBN0023
{
    /// <summary>
    /// Gets the item in a form protobuf can save
    /// </summary>
    /// <returns>The item to save</returns>
    IProtoFilterItem Pack();

    /// <summary>
    /// Gets the filter item back from its saved form
    /// </summary>
    /// <returns>The filter item</returns>
    IFilterItem Unpack();
}
