namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using TopMarksDevelopment.ExpressionBuilder.Api;

[ProtoBuf.ProtoContract()]
public partial class ProtoFilterStatement : IProtoFilterItem
{
    [ProtoBuf.ProtoMember(1)]
    public Connector Connector { get; set; }

    [ProtoBuf.ProtoMember(2)]
    [System.ComponentModel.DefaultValue("")]
    public string propertyType { get; set; } = "";

    [ProtoBuf.ProtoMember(3)]
    [System.ComponentModel.DefaultValue("")]
    public string PropertyId { get; set; } = "";

    [ProtoBuf.ProtoMember(4)]
    [System.ComponentModel.DefaultValue("")]
    string OperationName
    {
        get => Operation.Name;
        set
        {
            var i = TypeTracker.Operations;
            Operation = SerializerBase.FindOperationByName(value, ref i);
            TypeTracker.Operations = i;
        }
    }

    public IOperation Operation { get; set; } = null!;

    [ProtoBuf.ProtoMember(5)]
    public List<byte[]?> ByteValues { get; set; } = [];

    [ProtoBuf.ProtoMember(6)]
    public FilterStatementOptions? Options { get; set; }

    public IProtoFilterItem Pack()
    {
        throw new NotImplementedException();
    }

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
