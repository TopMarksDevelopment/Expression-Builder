namespace ExpressionBuilder.Tests;

using System.IO;
using System.Text.Json;
using ProtoBuf.Meta;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

[Collection("Serialization checks")]
public class SerializationTests
{
    readonly Type _testType = typeof(FilterStatement<FilterGroup>);

    public SerializationTests()
    {
        // Prep for `ProtobufTypeTrackerTest` - faking runtime setup
        var protoTypes = TypeTracker.FilterStatementTypes;
        if (!protoTypes.Contains(_testType))
        {
            protoTypes.Add(typeof(FilterStatement<Connector>));
            protoTypes.Add(_testType);
        }
    }

    [Theory(DisplayName = "Multi Collection Test")]
    [ClassData(typeof(SerializationFilterTestData))]
    public void MultiCollectionTest(Filter<Category> filter)
    {
        var jsonString = JsonSerializer.Serialize(filter);
        var filterFromJson = JsonSerializer.Deserialize<Filter<Category>>(
            jsonString
        );

        Assert.Equal(filter.ToMatchString(), filterFromJson?.ToMatchString());
    }

    [Fact(DisplayName = "Protobuf Type Tracker Test")]
    public void ProtobufTypeTrackerTest()
    {
        // throw away filter
        var filter = new Filter<Category>();
        filter.Equal(x => x.Id, 2);

        filter.SerializeTo(out var _);

        var expectedFieldNumber = RuntimeTypeModel
            .Default.Add(typeof(IFilterStatement), false)
            .GetSubtypes()
            .FirstOrDefault(x => x.DerivedType.Type == _testType)
            ?.FieldNumber;

        Assert.Equal(81, expectedFieldNumber);
    }

    [Theory(DisplayName = "Protobuf default order is correct")]
    [ClassData(typeof(SerializationTypeTestData))]
    public void ProtobufDefaultTest((int FieldNumber, Type Type) matchData)
    {
        // throw away filter
        var filter = new Filter<Category>();
        filter.Equal(x => x.Id, 2);

        filter.SerializeTo(out var _);

        var expectedFieldNumber = RuntimeTypeModel
            .Default.Add(typeof(IFilterStatement), false)
            .GetSubtypes()
            .FirstOrDefault(x => x.DerivedType.Type == matchData.Type)
            ?.FieldNumber;

        Assert.Equal(expectedFieldNumber, matchData.FieldNumber);
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

    [Theory(DisplayName = "Protobuf byte[] Test")]
    [ClassData(typeof(SerializationFilterTestData))]
    public void ProtobufByteArrayTest(Filter<Category> filter)
    {
        filter.SerializeTo(out var bytes);

        var actual = Filter<Category>.DeserializeFrom(bytes);

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
