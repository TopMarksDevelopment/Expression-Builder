namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class IEntityManipulatorJsonConverter
    : JsonConverter<IEnumerable<IEntityManipulator>>
{
    ICollection<Type>? _foundTypes;

    public override IEnumerable<IEntityManipulator> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var elems = JsonSerializer.Deserialize<ManipulatorInfo[]>(
            ref reader,
            options
        );

        var manipulators = new List<IEntityManipulator>();

        if (elems == null || elems.Length == 0)
            return manipulators;

        foreach (var item in elems)
        {
            // foreach (var arg in item.Arguments)
            //     if (arg is JsonElement j)
            //         arg = JsonNode.Parse(j.GetRawText());

            manipulators.Add(
                SerializerBase.FindManipulator(item, ref _foundTypes)
            );
        }

        return manipulators;
    }

    public override void Write(
        Utf8JsonWriter writer,
        IEnumerable<IEntityManipulator> value,
        JsonSerializerOptions options
    ) =>
        JsonSerializer.Serialize(
            writer,
            value.Select(x =>
                x is ExpressionMethodManipulator ex
                    ? new ManipulatorInfo(ex)
                    : new ManipulatorInfo(x.Name, x.Arguments)
            ),
            options
        );
}
