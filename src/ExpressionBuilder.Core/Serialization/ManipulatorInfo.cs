using System.Text.Json;
using ProtoBuf;

namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

[ProtoContract(UseProtoMembersOnly = true)]
internal struct ManipulatorInfo
{
    public ManipulatorInfo() { }

    public ManipulatorInfo(ExpressionMethodManipulator manipulator)
    {
        Name = manipulator.Name;
        Arguments = manipulator.Arguments;
        Type = manipulator.TypeName;
        ArgTypes = manipulator
            .ExpectedTypes.Select(x => x.FullName ?? x.Name)
            .ToList();
    }

    public ManipulatorInfo(string name, object?[] arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    [ProtoMember(1)]
    public string Name { get; set; } = string.Empty;

    public object?[] Arguments { get; set; } = [];

    [ProtoMember(2)]
    public string? Type { get; set; }

    [ProtoMember(3)]
    public ICollection<string>? ArgTypes { get; set; }

    [ProtoMember(4)]
    string Args
    {
        readonly get => JsonSerializer.Serialize(Arguments);
        set => Arguments = JsonSerializer.Deserialize<object?[]>(value) ?? [];
    }
}
