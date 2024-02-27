using System.Text.Json;
using ProtoBuf;

namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

[ProtoContract(UseProtoMembersOnly = true)]
internal class ManipulatorInfo
{
    ICollection<string>? _types;

    public ManipulatorInfo() { }

    public ManipulatorInfo(ExpressionMethodManipulator manipulator)
    {
        Name = manipulator.Name;
        Arguments = manipulator.Arguments;
        Type = manipulator.TypeName;
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
    public ICollection<string>? ArgTypes
    {
        get =>
            Type == null
                ? null
                : _types ??= Arguments
                    .Select(x => x!.GetType())
                    .Select(x => x.FullName ?? x.Name)
                    .ToList();
        set => _types = value;
    }

    [ProtoMember(4)]
    string Args
    {
        get => JsonSerializer.Serialize(Arguments);
        set => Arguments = JsonSerializer.Deserialize<object?[]>(value) ?? [];
    }
}
