namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// The binary (protobuf) form of a <see cref="FilterStatement{TPropertyType}"/>. It has no generic
/// type, so it can be saved and loaded without knowing the type of the property.
/// </summary>
[ProtoBuf.ProtoContract()]
public partial class ProtoFilterStatement : IProtoFilterItem
{
    /// <inheritdoc />
    [ProtoBuf.ProtoMember(1)]
    public Connector Connector { get; set; }

    /// <inheritdoc />
    [ProtoBuf.ProtoMember(2)]
    [System.ComponentModel.DefaultValue("")]
    public string propertyType { get; set; } = "";

    /// <inheritdoc />
    [ProtoBuf.ProtoMember(3)]
    [System.ComponentModel.DefaultValue("")]
    public string PropertyId { get; set; } = "";

    [ProtoBuf.ProtoMember(4)]
    [System.ComponentModel.DefaultValue("")]
    string OperationName
    {
        get => Operation.Name;
        set => Operation = SerializerBase.FindOperationByName(value);
    }

    /// <inheritdoc />
    public IOperation Operation { get; set; } = null!;

    /// <inheritdoc />
    [ProtoBuf.ProtoMember(5)]
    public List<byte[]?> ByteValues { get; set; } = [];

    /// <inheritdoc />
    [ProtoBuf.ProtoMember(6)]
    public FilterStatementOptions? Options { get; set; }

    /// <inheritdoc />
    public IProtoFilterItem Pack()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IFilterItem Unpack()
    {
        var sourceType = Type.GetType(propertyType)!;

        var instance = (IFilterStatement)
            Activator.CreateInstance(
                typeof(FilterStatement<>).MakeGenericType(sourceType)
            )!;

        instance.Connector = Connector;
        instance.PropertyId = PropertyId;
        instance.Operation = Operation;
        instance.Options = Options;
        instance.Values.AddRange(
            ByteValues
                .Select(i => ValueToByteConverters.UnPack(sourceType, i))
                .ToArray()
        );

        return instance;
    }
}
