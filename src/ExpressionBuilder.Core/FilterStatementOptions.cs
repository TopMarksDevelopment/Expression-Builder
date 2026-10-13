using System.Text.Json.Serialization;
using ProtoBuf;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

namespace TopMarksDevelopment.ExpressionBuilder;

[ProtoContract(UseProtoMembersOnly = true)]
public class FilterStatementOptions : IFilterStatementOptions
{
    [JsonIgnore]
    public IEnumerable<IEntityManipulator>? Manipulators { get; set; }

    [ProtoMember(1)]
    public Matches? Match { get; set; }

    [JsonInclude]
    [ProtoMember(2)]
    List<ManipulatorInfo>? ManipulatorInfo
    {
        get =>
            Manipulators
                ?.Select(x =>
                    x is ExpressionMethodManipulator eMM
                        ? new ManipulatorInfo(eMM)
                        : new ManipulatorInfo(x.Name, x.Arguments)
                )
                .ToList();
        set
        {
            List<IEntityManipulator> manipulators = [];

            var i = TypeTracker.ManipulatorTypes;

            if (value != null)
                foreach (var item in value)
                    manipulators.Add(
                        SerializerBase.FindManipulator(item, ref i)
                    );

            Manipulators = manipulators;
        }
    }
}
