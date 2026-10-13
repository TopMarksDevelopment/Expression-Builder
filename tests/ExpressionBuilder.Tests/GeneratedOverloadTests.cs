namespace TopMarksDevelopment.ExpressionBuilder.Tests;

using System.Reflection;

/// <summary>
/// The fluent methods of every operation (<c>filter.Equal(...)</c>) are generated, 25 operations ×
/// 6 receivers × up to 16 overloads. This calls every one of them and checks it did what its
/// signature says: the right operation, property, values, connector and options end up in the filter.
/// </summary>
public class GeneratedOverloadTests
{
    static readonly string[] Receivers =
    [
        "IFilter",
        "IFilter_TClass_",
        "IFilterable",
        "IFilterable_TClass_",
        "IQueryFilterable",
        "IQueryFilterable_TClass_",
    ];

    static readonly Assembly[] OperationAssemblies =
    [
        .. typeof(Operation)
            .Assembly.GetReferencedAssemblies()
            .Where(a => a.Name!.Contains(".Operations."))
            .Select(Assembly.Load),
    ];

    static IEnumerable<string> OperationNames =>
        typeof(Operation)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Select(p => p.Name);

    public static TheoryData<string, string> Cases()
    {
        var data = new TheoryData<string, string>();

        foreach (var operation in OperationNames)
        foreach (var receiver in Receivers)
            data.Add(operation, receiver);

        return data;
    }

    static Type ExtensionsOf(string receiver, string operation) =>
        OperationAssemblies
            .Select(a =>
                a.GetType(
                    $"TopMarksDevelopment.ExpressionBuilder.{receiver}{operation}Extensions"
                )
            )
            .Single(t => t != null)!;

    /// <summary>A new, empty target of the kind the extensions are for.</summary>
    static (object Receiver, Func<IFilter> Filter) NewReceiver(string receiver)
    {
        var products = new List<Product>();

        switch (receiver)
        {
            case "IFilter"
            or "IFilter_TClass_":
                var filter = new Filter<Product>();

                return (filter, () => filter);

            case "IFilterable"
            or "IFilterable_TClass_":
                var filterable = products.ToFilterable();

                return (filterable, () => filterable.Filter);

            default:
                var queryable = products.AsQueryable().AsFilterable();

                return (queryable, () => queryable.Filter);
        }
    }

    [Theory(
        DisplayName = "Every generated overload adds the statement it describes"
    )]
    [MemberData(nameof(Cases))]
    public void EveryOverloadDoesWhatItsSignatureSays(
        string operation,
        string receiverName
    ) =>
        CheckOverloads(
            ExtensionsOf(receiverName, operation),
            operation,
            receiverName,
            operation
        );

    [Theory(
        DisplayName = "Every overload of Add, which the generated methods build on, does what it says"
    )]
    [MemberData(nameof(AddCases))]
    public void EveryAddOverloadDoesWhatItsSignatureSays(string receiverName)
    {
        var extensions = typeof(IFilterAddExtensions).Assembly.GetType(
            $"TopMarksDevelopment.ExpressionBuilder.{receiverName}AddExtensions"
        )!;

        CheckOverloads(extensions, "Add", receiverName, "Equal");
    }

    public static TheoryData<string> AddCases()
    {
        var data = new TheoryData<string>();

        foreach (var receiver in Receivers)
            data.Add(receiver);

        return data;
    }

    /// <summary>
    /// Calls every overload of a method with arguments made up from its parameter names, and checks what it
    /// left in the filter.
    /// </summary>
    static void CheckOverloads(
        Type extensions,
        string methodName,
        string receiverName,
        string operation
    )
    {
        var methods = extensions
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName)
            .ToList();

        // 16 overloads for methods that take values, 8 for the ones that take none
        Assert.Contains(methods.Count, new[] { 8, 16 });

        var options = new FilterStatementOptions();
        Expression<Func<Product, string?>> property = x => x.Name;

        foreach (var method in methods)
        {
            var (receiver, filterOf) = NewReceiver(receiverName);

            var closed = method.IsGenericMethodDefinition
                ? method.MakeGenericMethod(
                    method.GetGenericArguments().Length == 2
                        ? [typeof(Product), typeof(string)]
                        : [typeof(string)]
                )
                : method;

            var parameters = closed.GetParameters();
            var arguments = parameters
                .Select(p => ArgumentFor(p, receiver, property, options))
                .ToArray();

            closed.Invoke(null, arguments);

            var statement = Assert.IsAssignableFrom<IFilterStatement>(
                Assert.Single(filterOf().Items)
            );
            var description = $"{receiverName}.{closed}";

            Assert.True(statement.Operation.Name == operation, description);
            Assert.True(statement.PropertyId == "Name", description);
            Assert.True(
                statement.Values.Count == ExpectedValueCount(parameters),
                description
            );
            Assert.True(
                statement.Connector
                    == (
                        parameters.Any(p =>
                            p.ParameterType == typeof(Connector)
                        )
                            ? Connector.Or
                            : Connector.And
                    ),
                description
            );
            Assert.True(
                ReferenceEquals(
                    statement.Options,
                    parameters.Any(p => p.Name == "options") ? options : null
                ),
                description
            );
        }
    }

    static object? ArgumentFor(
        ParameterInfo parameter,
        object receiver,
        Expression<Func<Product, string?>> property,
        FilterStatementOptions options
    ) =>
        parameter.Name switch
        {
            "filter" => receiver,
            "operation" => new Equal(),
            "propertyExpression"
                when parameter.ParameterType == typeof(string) => "Name",
            "propertyExpression" => property,
            "options" => options,
            "connector" => Connector.Or,
            "value" when parameter.ParameterType.IsArray => new string?[]
            {
                "a",
                "b",
            },
            "value" => "a",
            "value2" => "b",
            _ => throw new NotSupportedException(parameter.Name),
        };

    static int ExpectedValueCount(ParameterInfo[] parameters)
    {
        var value = parameters.SingleOrDefault(p => p.Name == "value");

        if (value == null)
            return 0;

        if (value.ParameterType.IsArray)
            return 2;

        return parameters.Any(p => p.Name == "value2") ? 2 : 1;
    }

    [Fact(DisplayName = "Every operation has fluent methods on every receiver")]
    public void NothingIsMissing()
    {
        var missing = OperationNames
            .SelectMany(o => Receivers.Select(r => (Operation: o, Receiver: r)))
            .Where(c =>
                OperationAssemblies.All(a =>
                    a.GetType(
                        $"TopMarksDevelopment.ExpressionBuilder.{c.Receiver}{c.Operation}Extensions"
                    ) == null
                )
            )
            .ToList();

        Assert.Empty(missing);
    }
}
