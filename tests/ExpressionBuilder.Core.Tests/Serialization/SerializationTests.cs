namespace ExpressionBuilder.Tests;

using System.IO;
using System.Text.Json;

[Collection("Serialization checks")]
public class SerializationTests
{
    [Theory(DisplayName = "JSON String Test")]
    [ClassData(typeof(SerializationFilterTestData))]
    public void JsonStringTest(Filter<Category> filter)
    {
        var jsonString = JsonSerializer.Serialize(filter);
        var filterFromJson = JsonSerializer.Deserialize<Filter<Category>>(
            jsonString
        );

        Assert.Equal(filter.ToMatchString(), filterFromJson?.ToMatchString());
    }

    [Theory(DisplayName = "JSON File Test")]
    [ClassData(typeof(SerializationFilterTestData))]
    public void JsonFileTest(Filter<Category> filter)
    {
        using (var file = File.Create("../../Test.json"))
            file.Write(JsonSerializer.SerializeToUtf8Bytes(filter));

        var jsonFileData = File.ReadAllText("../../Test.json");

        var filterFromJson = JsonSerializer.Deserialize<Filter<Category>>(
            jsonFileData
        );

        Assert.Equal(filter.ToMatchString(), filterFromJson?.ToMatchString());
    }

    [Theory(DisplayName = "Protobuf byte[] Test")]
    [ClassData(typeof(SerializationFilterTestData))]
    public void ProtobufByteArrayTest(Filter<Category> filter)
    {
        filter.SerializeTo(out var bytes);

        var actual = Filter<Category>.DeserializeFrom(bytes);

        Assert.Equal(filter.ToMatchString(), actual.ToMatchString());
    }

    [Theory(DisplayName = "Protobuf File Test")]
    [ClassData(typeof(SerializationFilterTestData))]
    public void ProtobufFileTest(Filter<Category> filter)
    {
        using (var file = File.Create("../../Test.dat"))
            filter.SerializeTo(file);

        using var rFile = File.OpenRead("../../Test.dat");

        var actual = Filter<Category>.DeserializeFrom(rFile);

        Assert.Equal(filter.ToMatchString(), actual.ToMatchString());
    }

    [Fact(DisplayName = "Can create .proto string/file")]
    public void CreateProto()
    {
        string groupProto = ProtoBuf.Serializer.GetProto<FilterGroup>();

        File.WriteAllText("../../ExpressionBuilder.proto", groupProto);

        Assert.True(File.Exists("../../ExpressionBuilder.proto"));
    }
}
