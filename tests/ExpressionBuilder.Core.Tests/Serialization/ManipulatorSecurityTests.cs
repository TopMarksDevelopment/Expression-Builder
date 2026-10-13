namespace ExpressionBuilder.Tests;

using System.Text.Json;
using System.Text.Json.Nodes;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

/// <summary>
/// A saved filter names the method of a manipulator by type and name, so a document that is not
/// trusted must not be able to choose just any method to run.
/// </summary>
[Collection("Manipulator security")]
public class ManipulatorSecurityTests
{
    static Filter<Category> FilterWithTrim()
    {
        var filter = new Filter<Category>();
        filter.Equal(x => x.Name.Trim(), "Category 1");

        return filter;
    }

    /// <summary>
    /// The saved form of a filter with a <c>Trim()</c> manipulator, with that manipulator pointed at a
    /// different method, as someone editing the stored document would.
    /// </summary>
    static string Tampered(
        string type,
        string method,
        string argumentType,
        string argument
    )
    {
        var json = JsonNode.Parse(JsonSerializer.Serialize(FilterWithTrim()))!;
        var manipulator = json["Items"]![0]!["Options"]!["ManipulatorInfo"]![
            0
        ]!;

        manipulator["Type"] = type;
        manipulator["Name"] = method;
        manipulator["ArgTypes"] = new JsonArray(argumentType);
        manipulator["Arguments"] = new JsonArray(argument);

        return json.ToJsonString();
    }

    [Fact(DisplayName = "A saved filter with a method manipulator loads")]
    public void MethodManipulatorsStillLoad()
    {
        var json = JsonSerializer.Serialize(FilterWithTrim());

        var loaded = JsonSerializer.Deserialize<Filter<Category>>(json);

        Assert.Equal(FilterWithTrim().ToString(), loaded!.ToString());
    }

    [Theory(
        DisplayName = "A saved filter can't make a manipulator of a static method"
    )]
    [InlineData(
        "System.Environment",
        "GetEnvironmentVariable",
        "System.String",
        "PATH"
    )]
    [InlineData("System.IO.File", "Exists", "System.String", "C:/")]
    [InlineData(
        "System.Diagnostics.Process",
        "Start",
        "System.String",
        "notepad"
    )]
    public void StaticMethodsAreRefused(
        string type,
        string method,
        string argumentType,
        string argument
    )
    {
        var json = Tampered(type, method, argumentType, argument);

        Assert.ThrowsAny<Exception>(
            () => JsonSerializer.Deserialize<Filter<Category>>(json)
        );
    }

    [Fact(DisplayName = "The same applies to the compact (protobuf) form")]
    public void StaticMethodsAreRefusedInProtobuf()
    {
        var info = new ManipulatorInfo
        {
            Name = "GetEnvironmentVariable",
            Type = "System.Environment",
            ArgTypes = ["System.String"],
            Arguments = ["PATH"],
        };

        Assert.Throws<InvalidOperationException>(() =>
        {
            SerializerBase.FindManipulator(info);
        });
    }

    [Fact(
        DisplayName = "A manipulator that isn't a method of the value is not found by name either"
    )]
    public void UnknownManipulatorsAreRefused()
    {
        var info = new ManipulatorInfo("NoSuchManipulator", []);

        Assert.Throws<TypeLoadException>(
            () => SerializerBase.FindManipulator(info)
        );
    }
}
