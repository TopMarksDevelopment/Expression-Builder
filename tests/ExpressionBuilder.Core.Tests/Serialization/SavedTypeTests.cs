namespace ExpressionBuilder.Tests;

using System.IO;
using System.Text.Json;

/// <summary>
/// A saved filter names the type of each statement's values, so it can be rebuilt. The name has to be one that can be
/// found again from a different process, which for a type of the application means saying which assembly it is in.
/// </summary>
[Collection("Serialization checks")]
public class SavedTypeTests
{
    public enum Shade
    {
        Light,
        Dark,
    }

    public class Paint
    {
        public string Name { get; set; } = "";

        public Shade Shade { get; set; }

        public Shade? Optional { get; set; }
    }

    static readonly Paint[] Paints =
    [
        new() { Name = "a", Shade = Shade.Light },
        new() { Name = "b", Shade = Shade.Dark },
        new()
        {
            Name = "c",
            Shade = Shade.Dark,
            Optional = Shade.Dark,
        },
    ];

    static Filter<Paint> Filter()
    {
        var filter = new Filter<Paint>();
        filter.Add("Shade", new Equal(), new[] { Shade.Dark }).And();
        filter.Add("Name", new NotEqual(), new[] { "a" });

        return filter;
    }

    static List<string> Names(Filter<Paint> filter) =>
        [.. Paints.Where(filter).Select(p => p.Name)];

    [Fact(
        DisplayName = "JSON: values that are an enum of the application can be read back"
    )]
    public void JsonEnumOfTheApplication()
    {
        var json = JsonSerializer.Serialize(Filter());

        var back = JsonSerializer.Deserialize<Filter<Paint>>(json)!;

        Assert.Equal(Names(Filter()), Names(back));
        Assert.Equal(["b", "c"], Names(back));
    }

    [Fact(
        DisplayName = "Protobuf: values that are an enum of the application can be read back"
    )]
    public void ProtobufEnumOfTheApplication()
    {
        Filter().SerializeTo(out var bytes);

        var back = Filter<Paint>.DeserializeFrom(bytes);

        Assert.Equal(["b", "c"], Names(back));
    }

    [Fact(
        DisplayName = "Protobuf: a stream of values that are an enum of the application can be read back"
    )]
    public void ProtobufStreamEnumOfTheApplication()
    {
        using var stream = new MemoryStream();
        Filter().SerializeTo(stream);
        stream.Position = 0;

        Assert.Equal(["b", "c"], Names(Filter<Paint>.DeserializeFrom(stream)));
    }

    [Fact(DisplayName = "JSON: a nullable enum of the application too")]
    public void JsonNullableEnum()
    {
        var filter = new Filter<Paint>();
        filter.Add("Optional", new Equal(), new Shade?[] { Shade.Dark });

        var back = JsonSerializer.Deserialize<Filter<Paint>>(
            JsonSerializer.Serialize(filter)
        )!;

        Assert.Equal(["c"], Names(back));
    }

    [Fact(
        DisplayName = "The name of a type of the framework is written as it always was"
    )]
    public void FrameworkTypesAreWrittenAsBefore()
    {
        var filter = new Filter<Paint>();
        filter.Add("Name", new Equal(), new[] { "a" });

        Assert.Contains(
            "\"propertyType\":\"System.String\"",
            JsonSerializer.Serialize(filter)
        );
    }

    [Fact(
        DisplayName = "The name of a type of the application says which assembly it is in"
    )]
    public void ApplicationTypesNameTheirAssembly()
    {
        var statement = new FilterStatement<Shade>(
            "Shade",
            new Equal(),
            new[] { Shade.Dark }
        );

        Assert.EndsWith(
            ", " + typeof(Shade).Assembly.GetName().Name,
            statement.propertyType
        );
        Assert.Same(typeof(Shade), Type.GetType(statement.propertyType));
    }
}
