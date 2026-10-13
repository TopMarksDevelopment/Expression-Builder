namespace TopMarksDevelopment.ExpressionBuilder.Tests;

using System.Reflection;

public class OperationTests
{
    /// <summary>
    /// The operations that ship in the main package. Adding one means adding it here, to
    /// <see cref="Operation"/> and to the package's references - this list keeps those honest.
    /// </summary>
    static readonly string[] BuiltInOperations =
    [
        "Between",
        "BetweenExclusive",
        "Contains",
        "DoesNotContain",
        "DoesNotEndWith",
        "DoesNotStartWith",
        "EndsWith",
        "Equal",
        "GreaterThan",
        "GreaterThanOrEqual",
        "In",
        "IsEmpty",
        "IsNotEmpty",
        "IsNotNull",
        "IsNotNullOrWhiteSpace",
        "IsNull",
        "IsNullOrWhiteSpace",
        "LessThan",
        "LessThanOrEqual",
        "NotBetween",
        "NotBetweenExclusive",
        "NotEqual",
        "NotIn",
        "SmartSearch",
        "StartsWith",
    ];

    static readonly PropertyInfo[] OperationProperties = typeof(Operation)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .ToArray();

    [Fact(DisplayName = "All operations have been found")]
    public void HasAllOperations()
    {
        var names = OperationRegistry.GetAll().Select(o => o.Name);

        Assert.Equal(
            BuiltInOperations.Order(StringComparer.Ordinal),
            names
                .Where(BuiltInOperations.Contains)
                .Order(StringComparer.Ordinal)
        );
    }

    [Fact(DisplayName = "Operation exposes every built-in operation")]
    public void OperationHasAllOperations() =>
        Assert.Equal(
            BuiltInOperations,
            OperationProperties
                .Select(p => p.Name)
                .Order(StringComparer.Ordinal)
        );

    [Theory(DisplayName = "Operation.X is the operation named X")]
    [MemberData(nameof(OperationNames))]
    public void OperationPropertyMatchesItsName(string name)
    {
        var property = OperationProperties.Single(p => p.Name == name);
        var operation = (IOperation)property.GetValue(null)!;

        Assert.Equal(name, operation.Name);
        Assert.Equal(property.PropertyType, operation.GetType());
    }

    [Theory(DisplayName = "Every operation can be found by name")]
    [MemberData(nameof(OperationNames))]
    public void OperationsAreFoundByName(string name)
    {
        var operation = OperationRegistry.Get(name);

        Assert.Equal(name, operation.Name);
        Assert.Equal(
            OperationProperties.Single(p => p.Name == name).PropertyType,
            operation.GetType()
        );
    }

    public static TheoryData<string> OperationNames()
    {
        var data = new TheoryData<string>();

        foreach (var name in BuiltInOperations)
            data.Add(name);

        return data;
    }
}
