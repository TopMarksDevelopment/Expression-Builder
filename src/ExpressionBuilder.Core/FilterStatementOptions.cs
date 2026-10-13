using System.Text.Json.Serialization;
using ProtoBuf;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

namespace TopMarksDevelopment.ExpressionBuilder;

/// <summary>
/// The options of a statement: how it matches several values and the manipulators it applies.
/// </summary>
[ProtoContract(UseProtoMembersOnly = true)]
public class FilterStatementOptions : IFilterStatementOptions
{
    /// <inheritdoc />
    [JsonIgnore]
    public IEnumerable<IEntityManipulator>? Manipulators { get; set; }

    /// <inheritdoc />
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

            if (value != null)
                foreach (var item in value)
                    manipulators.Add(SerializerBase.FindManipulator(item));

            Manipulators = manipulators;
        }
    }
}
